using UnityEngine;

public class AttackStateLogic : IEnemyState
{
    private readonly EnemyController enemy;
    private readonly EnemyStateMachine stateMachine;
    private readonly IPlayerPositionProvider playerPositionProvider;
    private readonly IEnemyMovement movement;
    private readonly EnemyAttackDetector attackDetector;

    private float attackCooldown = 1.5f;
    private float lastAttackTime = -999f;
    private float attackDamage = 25f;

    private AudioClip attackSound;
    private AudioSource audioSource;

    public AttackStateLogic(
        EnemyController enemy,
        EnemyStateMachine stateMachine,
        IPlayerPositionProvider playerPositionProvider,
        IEnemyMovement movement,
        AudioSource audioSource,
        AudioClip attackSound)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.playerPositionProvider = playerPositionProvider;
        this.movement = movement;
        this.audioSource = audioSource;
        this.attackSound = attackSound;

        attackDetector = enemy.GetComponentInChildren<EnemyAttackDetector>();

        if (attackDetector == null)
        {
            Debug.LogError(
                "[Enemy] AttackStateLogic: No se encontró EnemyAttackDetector dentro del enemigo."
            );
        }
    }

    public void Enter()
    {
        if (movement != null)
            movement.Stop();
    }

    public void Tick()
    {
        if (playerPositionProvider == null)
            return;

        // El rango de ataque ahora lo controla exclusivamente
        // el DetectionAttack + EnemyAttackDetector.
        if (attackDetector == null)
            return;

        if (!attackDetector.IsPlayerInside)
        {
            enemy.GoToStealthStatePublic();
            return;
        }

        Vector3 playerPos = playerPositionProvider.PlayerBodyPosition;

        // Enfocar la mirada hacia el jugador
        Vector3 dir = (playerPos - enemy.transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 10f
            );
        }

        // Lógica de ataque con cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    public void Exit()
    {
        if (movement != null)
            movement.Stop();
    }

    private void PerformAttack()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Debug.Log("[Enemy] ¡Atacando al jugador!");

        if (PlayerManager.Instance != null)
        {
            IDamageable playerDamageable =
                PlayerManager.Instance.GetComponent<IDamageable>();

            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(attackDamage);
            }
        }
    }
}