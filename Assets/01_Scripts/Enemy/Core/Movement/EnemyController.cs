using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Evolución / Dificultad")]
    [SerializeField] private bool isEnraged = false;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private AudioClip attackSound;

    private EnemyStateMachine stateMachine;

    [Header("Componentes")]
    [SerializeField] private IPlayerDetector playerDetector;
    [SerializeField] private IEnemyMovement enemyMovement;
    [SerializeField] private IVisibilityController visibilityController;
    [SerializeField] private IRespawnHandler respawnHandler;

    // ✅ CORRECCIÓN 1: Referencia directa a las propiedades por defecto (Fallback)
    // Arrastra el script EnemyProperties aquí en el inspector del Prefab.
    [SerializeField] private EnemyProperties defaultStats;

    private ISafeZoneProvider safeZoneProvider;
    private EnemyLightSensitivity healthSystem;
    private IEnemyStatsProvider statsProvider;
    private IPlayerPositionProvider playerPositionProvider;
    private IEnemyState currentState;
    private bool isDying = false;
    private bool isInvulnerable = false;
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
        // ✅ CORRECCIÓN 2: Autodetectar si se olvidó asignar en el inspector
        if (defaultStats == null)
            defaultStats = GetComponent<EnemyProperties>();

        if (healthSystem == null) Debug.LogError("Falta EnemyLightSensitivity");
        if (defaultStats == null) Debug.LogError("Falta el componente EnemyProperties (Configuración por defecto)");
    }

    public void Initialize(IRespawnHandler respawnHandler, ISafeZoneProvider safeZoneProvider)
    {
        this.respawnHandler = respawnHandler;
        this.safeZoneProvider = safeZoneProvider;
    }

    private void Start()
    {
        // ... (Tu lógica de SafeZone y Respawn igual) ...
        if (safeZoneProvider == null)
        {
            var manager = FindFirstObjectByType<EnemyManager>();
            safeZoneProvider = manager;
            respawnHandler = manager;
        }

        if (PlayerManager.Instance != null) playerPositionProvider = PlayerManager.Instance;
        if (GamePacingManager.Instance != null) statsProvider = GamePacingManager.Instance;

        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken += HandleDamageTaken;
            healthSystem.OnHealthDepleted += HandleDeathByLight;
        }

        InitializeState();
    }

    private void InitializeState()
    {
        if (playerPositionProvider != null) GoToStealthState();
    }

    private void Update()
    {
        if (playerDetector is PlayerVisibilityDetector detector) detector.ManualDetect();
        stateMachine.ManualTick();

        if (currentState is RetreatStateLogic retreatState && retreatState.IsRetreatComplete())
        {
            GoToStealthState();
        }
    }
    public void HandlePhaseChange(EnemyPhase newPhase)
    {
        StartCoroutine(EvolveRoutine(newPhase));
    }

    private void HandleDamageTaken(float amount)
    {
        if (isInvulnerable) return; // Si se está transformando, no siente dolor
        if (isDying) return;
        if (isEnraged) return;
        if (currentState is RetreatStateLogic) return;

        HandleLightScare();
    }

    // ✅ CORRECCIÓN 3: El método conflictivo ahora es seguro y limpio
    private void GoToStealthState()
    {
        // Lógica de selección: ¿Usamos el manager global o los stats locales del prefab?
        IEnemyMovementConfig moveConfig = (statsProvider != null) ? statsProvider.CurrentMovementConfig : defaultStats;
        IFollowSettings followConfig = (statsProvider != null) ? statsProvider.CurrentFollowSettings : defaultStats;

        // Validación de seguridad extra
        if (moveConfig == null || followConfig == null)
        {
            Debug.LogError("CRÍTICO: No hay configuración de movimiento (ni Global ni Local en EnemyProperties).");
            return;
        }

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
        if (isDying || isInvulnerable) return;
        currentState = new AttackStateLogic(this, stateMachine, playerPositionProvider, enemyMovement, audioSource, attackSound);
        stateMachine.SetState(currentState);
    }

    public void ReturnToStealthAfterRespawn()
    {
        isDying = false;
        if (healthSystem != null) healthSystem.ResetHealth();
        GoToStealthState();
    }

    private void HandleLightScare()
    {// GUARDIA DE SEGURIDAD
        if (safeZoneProvider == null)
        {
            var manager = FindFirstObjectByType<EnemyManager>();
            if (manager != null) safeZoneProvider = manager;
            else
            {
                Debug.LogError("EnemyController: Imposible entrar en RetreatState. No hay EnemyManager/SafeZoneProvider.");
                return;
            }
        }

        if (audioSource != null && screamSound != null) audioSource.PlayOneShot(screamSound);

        // --- CORRECCIÓN AQUÍ ---
        // Antes: usabas GetCurrentProfile().retreatDuration
        // Ahora: usas la propiedad directa CurrentRetreatDuration
        float retreatDuration = (GamePacingManager.Instance != null)
            ? GamePacingManager.Instance.CurrentRetreatDuration
            : 2f; // Valor por defecto si no hay manager
        // -----------------------

        IEnemyMovementConfig moveConfig = (statsProvider != null) ? statsProvider.CurrentMovementConfig : defaultStats;

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
        if (isDying) return;
        isDying = true;
        var burnState = new FlashlightBurnStateLogic(this, visibilityController, respawnHandler);
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
        // 1. "PAUSA DRAMÁTICA": El enemigo se vuelve invulnerable y grita
        isInvulnerable = true;

        if (audioSource != null && screamSound != null)
            audioSource.PlayOneShot(screamSound);

        // Opcional: Forzar una animación de dolor aquí
        // animator.SetTrigger("Evolve");

        // 2. HUIDA FORZADA: Busca refugio para transformarse
        // Nota: Pasamos 'true' (o similar) si tuviéramos un flag de 'ForceRetreat'
        HandleLightScare();

        // Esperamos a que llegue al refugio o un tiempo fijo (ej. 3 segundos)
        yield return new WaitForSeconds(3.0f);

        // 3. APLICAR LOS PODERES DE LA FASE
        ApplyPhaseStats(phase);

        // 4. Terminar transición
        isInvulnerable = false;

        // Forzamos volver a Stealth con los nuevos poderes activos
        GoToStealthState();
    }
    private void ApplyPhaseStats(EnemyPhase phase)
    {
        switch (phase)
        {
            case EnemyPhase.Phase1_Stealth:
                isEnraged = false;
                // En fase 1 NO salta. Es una amenaza terrestre.
                if (jumpHandler != null) jumpHandler.EnableJumping(false);
                break;

            case EnemyPhase.Phase2_Jumper:
                // LÓGICA FASE 2:
                // 1. Se vuelve agresivo (ignora si lo miras)
                isEnraged = true;

                // 2. HABILITAMOS EL SALTO
                // Ahora el Update del JumpHandler empezará a buscar OffMeshLinks
                if (jumpHandler != null) jumpHandler.EnableJumping(true);

                // 3. Aumentamos velocidad ligeramente para hacerlo más aterrador
                if (enemyMovement != null)
                {
                    // Asumiendo que puedes modificar la velocidad en tu config o agente
                    GetComponent<NavMeshAgent>().speed += 2.0f;
                }

                Debug.Log("<color=yellow>FASE 2: JUMPER ACTIVADO</color>");
                break;

            case EnemyPhase.Phase3_Flyer:
                isEnraged = true;
                // En fase 3 vuela, así que la lógica de salto de NavMesh ya no aplica igual
                if (jumpHandler != null) jumpHandler.EnableJumping(false);
                break;
        }
    }
}

