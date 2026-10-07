using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Evolución / Dificultad")]
    private bool isEnraged;

    public bool IsEnraged => isEnraged;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private AudioClip attackSound;

    private EnemyStateMachine stateMachine;

    [Header("Componentes")]
    [SerializeField] private IPlayerDetector playerDetector;
    [SerializeField] private IEnemyMovement enemyMovement;
    [SerializeField] private IVisibilityController visibilityController;
    [SerializeField] private IRespawnHandler respawnHandler;

    // Fallback de configuración local del enemigo.
    [SerializeField] private EnemyProperties defaultStats;

    private ISafeZoneProvider safeZoneProvider;
    private EnemyLightSensitivity healthSystem;
    private IEnemyStatsProvider statsProvider;
    private IPlayerPositionProvider playerPositionProvider;
    private IEnemyState currentState;

    private bool isDying = false;
    private bool isInvulnerable = false;

    // True solamente cuando la retirada actual fue provocada por la linterna.
    // Esto evita que la retirada de una evolución termine en un respawn.
    private bool retreatLeadsToFlashlightBurn = false;

    private EnemyJumpHandler jumpHandler;

    private void Awake()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        playerDetector = GetComponent<IPlayerDetector>();
        enemyMovement = GetComponent<IEnemyMovement>();
        visibilityController = GetComponent<IVisibilityController>();
        respawnHandler = GetComponent<IRespawnHandler>();
        healthSystem = GetComponent<EnemyLightSensitivity>();
        jumpHandler = GetComponent<EnemyJumpHandler>();

        if (defaultStats == null)
            defaultStats = GetComponent<EnemyProperties>();

        if (healthSystem == null)
            Debug.LogError("Falta EnemyLightSensitivity");

        if (defaultStats == null)
            Debug.LogError("Falta el componente EnemyProperties (Configuración por defecto)");
    }

    public void Initialize(IRespawnHandler respawnHandler, ISafeZoneProvider safeZoneProvider)
    {
        this.respawnHandler = respawnHandler;
        this.safeZoneProvider = safeZoneProvider;
    }

    private void Start()
    {
        if (safeZoneProvider == null)
        {
            var manager = FindFirstObjectByType<EnemyManager>();

            if (manager != null)
            {
                safeZoneProvider = manager;
                respawnHandler = manager;
            }
        }

        if (PlayerManager.Instance != null)
            playerPositionProvider = PlayerManager.Instance;

        if (GamePacingManager.Instance != null)
            statsProvider = GamePacingManager.Instance;

        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += HandleDamageTaken;
            healthSystem.OnHealthDepleted += HandleDeathByLight;
        }

        InitializeState();
    }

    private void InitializeState()
    {
        retreatLeadsToFlashlightBurn = false;

        if (playerPositionProvider != null)
            GoToStealthState();
    }

    private void Update()
    {
        if (playerDetector is PlayerVisibilityDetector detector)
            detector.ManualDetect();

        stateMachine?.ManualTick();

        // La retirada por linterna termina cuando el enemigo llega físicamente
        // al refugio. En ese momento comienza la desaparición/respawn.
        if (currentState is RetreatStateLogic retreatState &&
            retreatLeadsToFlashlightBurn &&
            retreatState.HasReachedDestination)
        {
            retreatLeadsToFlashlightBurn = false;
            BeginFlashlightBurn();
        }
    }

    public void HandlePhaseChange(EnemyPhase newPhase)
    {
        StartCoroutine(EvolveRoutine(newPhase));
    }

    private void HandleDamageTaken(float amount)
    {
        if (isInvulnerable) return;
        if (isDying) return;
        if (isEnraged) return;

        HandleFlashlightReaction();
    }

    private void GoToStealthState()
    {
        IEnemyMovementConfig moveConfig =
            (statsProvider != null) ? statsProvider.CurrentMovementConfig : defaultStats;

        IFollowSettings followConfig =
            (statsProvider != null) ? statsProvider.CurrentFollowSettings : defaultStats;

        if (moveConfig == null || followConfig == null)
        {
            Debug.LogError(
                "CRÍTICO: No hay configuración de movimiento " +
                "(ni Global ni Local en EnemyProperties).");
            return;
        }

        retreatLeadsToFlashlightBurn = false;

        currentState = new StealthFollowStateLogic(
            this,
            playerDetector,
            playerPositionProvider,
            followConfig,
            moveConfig,
            isEnraged
        );

        stateMachine.SetState(currentState);
    }

    public void GoToStealthStatePublic()
    {
        GoToStealthState();
    }

    public void GoToAttackState()
    {
        if (isDying || isInvulnerable)
            return;

        currentState = new AttackStateLogic(
            this,
            stateMachine,
            playerPositionProvider,
            enemyMovement,
            audioSource,
            attackSound
        );

        stateMachine.SetState(currentState);
    }

    public void HandleFlashlightReaction()
    {
        if (isDying || isInvulnerable || isEnraged)
            return;

        if (currentState is RetreatStateLogic)
            return;

        // Esta retirada sí debe terminar en desaparición y respawn.
        HandleLightScare(true);
    }

    public void ReturnToStealthAfterRespawn()
    {
        isDying = false;
        retreatLeadsToFlashlightBurn = false;

        if (healthSystem != null)
            healthSystem.ResetHealth();

        GoToStealthState();
    }

    private void HandleLightScare(bool continueIntoFlashlightBurn)
    {
        retreatLeadsToFlashlightBurn = continueIntoFlashlightBurn;

        if (safeZoneProvider == null)
        {
            var manager = FindFirstObjectByType<EnemyManager>();

            if (manager != null)
            {
                safeZoneProvider = manager;
            }
            else
            {
                Debug.LogError(
                    "EnemyController: Imposible entrar en RetreatState. " +
                    "No hay EnemyManager/SafeZoneProvider."
                );

                retreatLeadsToFlashlightBurn = false;
                return;
            }
        }

        if (audioSource != null && screamSound != null)
            audioSource.PlayOneShot(screamSound);

        float retreatDuration =
            (GamePacingManager.Instance != null)
                ? GamePacingManager.Instance.CurrentRetreatDuration
                : 2f;

        IEnemyMovementConfig moveConfig =
            (statsProvider != null) ? statsProvider.CurrentMovementConfig : defaultStats;

        if (moveConfig == null)
        {
            Debug.LogError(
                "EnemyController: No existe configuración de movimiento " +
                "para RetreatState."
            );

            retreatLeadsToFlashlightBurn = false;
            return;
        }

        currentState = new RetreatStateLogic(
            this,
            playerPositionProvider,
            safeZoneProvider,
            moveConfig,
            retreatDuration
        );

        stateMachine.SetState(currentState);
    }

    private void HandleDeathByLight()
    {
        // Si todavía está retirándose por la linterna,
        // NO debemos saltarnos el refugio.
        if (currentState is RetreatStateLogic)
            return;

        BeginFlashlightBurn();
    }

    private void BeginFlashlightBurn()
    {
        if (isDying)
            return;

        if (respawnHandler == null)
        {
            Debug.LogError(
                "EnemyController: No hay IRespawnHandler. " +
                "No se puede completar el respawn por linterna."
            );
            return;
        }

        isDying = true;
        retreatLeadsToFlashlightBurn = false;

        var burnState = new FlashlightBurnStateLogic(
            this,
            visibilityController,
            respawnHandler
        );

        currentState = burnState;
        stateMachine.SetState(currentState);
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnHealthDepleted -= HandleDeathByLight;
        }
    }

    private IEnumerator EvolveRoutine(EnemyPhase phase)
    {
        isInvulnerable = true;

        if (audioSource != null && screamSound != null)
            audioSource.PlayOneShot(screamSound);

        // La evolución usa retirada, pero NO debe activar respawn por linterna.
        HandleLightScare(false);

        yield return new WaitForSeconds(3.0f);

        ApplyPhaseStats(phase);

        isInvulnerable = false;

        GoToStealthState();
    }

    private void ApplyPhaseStats(EnemyPhase phase)
    {
        switch (phase)
        {
            case EnemyPhase.Phase1_Stealth:
                isEnraged = false;

                if (jumpHandler != null)
                    jumpHandler.EnableJumping(false);

                break;

            case EnemyPhase.Phase2_Jumper:
                isEnraged = true;

                if (jumpHandler != null)
                    jumpHandler.EnableJumping(true);

                if (enemyMovement != null)
                {
                    NavMeshAgent agent = GetComponent<NavMeshAgent>();

                    if (agent != null)
                        agent.speed += 2.0f;
                }

                Debug.Log("<color=yellow>FASE 2: JUMPER ACTIVADO</color>");
                break;

            case EnemyPhase.Phase3_Flyer:
                isEnraged = true;

                if (jumpHandler != null)
                    jumpHandler.EnableJumping(false);

                break;
        }
    }
}
