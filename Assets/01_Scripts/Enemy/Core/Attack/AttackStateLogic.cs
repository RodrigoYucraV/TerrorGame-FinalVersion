using UnityEngine;

public class AttackStateLogic : IEnemyState
{
    private readonly EnemyController enemy;
    private readonly IPlayerPositionProvider playerPositionProvider;
    private readonly IEnemyMovement movement;
    private readonly AudioSource audioSource;
    private readonly AudioClip attackSound;
    private readonly IPlayerDetector playerDetector;

    [Header("Configuración del ataque")]
    private readonly float attackCooldown = 3.5f;
    private readonly float attackRange = 2.2f;
    private readonly float attackDamage = 10f;

    private float lastAttackTime = -999f;

    public AttackStateLogic(
        EnemyController enemy,
        EnemyStateMachine stateMachine,
        IPlayerPositionProvider playerPositionProvider,
        IEnemyMovement movement,
        AudioSource audioSource,
        AudioClip attackSound)
    {
        this.enemy = enemy;
        this.playerPositionProvider = playerPositionProvider;
        this.movement = movement;
        this.audioSource = audioSource;
        this.attackSound = attackSound;

        playerDetector = enemy != null
            ? enemy.GetComponent<IPlayerDetector>()
            : null;
    }

    public void Enter()
    {
        movement?.Stop();
        lastAttackTime = -999f;

        Debug.Log("[Enemy] Entró en AttackState.");
    }

    public void Tick()
    {
        if (enemy == null || playerPositionProvider == null)
            return;

        // =========================================================
        // PROTECCIÓN DE FASE 1
        // =========================================================

        if (!enemy.IsEnraged && playerDetector != null)
        {
            // -----------------------------------------------------
            // 1. SI LA LINTERNA LO ESTÁ GOLPEANDO
            // -----------------------------------------------------

            if (playerDetector.IsHitByFlashlight)
            {
                enemy.HandleFlashlightReaction();
                return;
            }

            // -----------------------------------------------------
            // 2. SI EL JUGADOR LO ESTÁ MIRANDO
            // -----------------------------------------------------

            if (playerDetector.IsPlayerLookingAtEnemy)
            {
                Debug.Log("[Enemy] Attack cancelado: el jugador está mirando.");

                enemy.GoToStealthStatePublic();
                return;
            }
        }

        // =========================================================
        // DISTANCIA
        // =========================================================

        Vector3 playerPosition =
            playerPositionProvider.PlayerBodyPosition;

        Vector3 direction =
            playerPosition - enemy.transform.position;

        float distance = direction.magnitude;

        // Si el jugador escapa del rango de combate,
        // volvemos al estado de acecho.
        if (distance > attackRange + 0.8f)
        {
            enemy.GoToStealthStatePublic();
            return;
        }

        // =========================================================
        // ORIENTACIÓN
        // =========================================================

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }

        // =========================================================
        // RANGO REAL DEL GOLPE
        // =========================================================

        if (distance > attackRange)
            return;

        // =========================================================
        // COOLDOWN
        // =========================================================

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        PerformAttack();

        lastAttackTime = Time.time;
    }

    public void Exit()
    {
        // No hay recursos adicionales que liberar por ahora.
    }

    private void PerformAttack()
    {
        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        Debug.Log(
            "[Enemy] ¡Ataque normal! Daño: " + attackDamage
        );

        PlayerManager player = PlayerManager.Instance;

        if (player == null)
            return;

        IDamageable damageable =
            player.GetComponent<IDamageable>();

        if (damageable == null)
        {
            Debug.LogWarning(
                "[Enemy] El jugador no tiene un componente IDamageable."
            );
            return;
        }

        damageable.TakeDamage(attackDamage);
    }
}