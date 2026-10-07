using UnityEngine;

public class RetreatStateLogic : IEnemyState
{
    private const float RepathInterval = 0.15f;
    private const float ArrivalDistance = 1.2f;

    private readonly EnemyController enemy;
    private readonly IEnemyMovement movement;
    private readonly ISafeZoneProvider safeZoneProvider;
    private readonly IPlayerPositionProvider playerProvider;
    private readonly IEnemyMovementConfig movementConfig;

    private float retreatDuration;
    private float timer;
    private float panicSpeed;
    private float nextRepathTime;
    private Vector3 retreatDestination;
    private bool hasReachedDestination;

    public RetreatStateLogic(
         EnemyController enemy,
         IPlayerPositionProvider playerProvider,
         ISafeZoneProvider safeZoneProvider,
         IEnemyMovementConfig movementConfig,
         float duration)
    {
        this.enemy = enemy;
        this.playerProvider = playerProvider;
        this.safeZoneProvider = safeZoneProvider;
        this.movementConfig = movementConfig;
        this.retreatDuration = Mathf.Max(0f, duration);
        this.movement = enemy != null ? enemy.GetComponent<IEnemyMovement>() : null;
        this.timer = 0f;
        this.nextRepathTime = 0f;
        this.hasReachedDestination = false;
    }

    public void Enter()
    {
        timer = 0f;
        nextRepathTime = 0f;
        hasReachedDestination = false;

        // 1. GUARDIA DE SEGURIDAD
        if (enemy == null)
        {
            Debug.LogError("CRÍTICO: enemy es NULL en RetreatState.");
            return;
        }

        if (safeZoneProvider == null)
        {
            Debug.LogError("CRÍTICO: safeZoneProvider es NULL en RetreatState. El enemigo no sabe a dónde huir.");
            return;
        }

        if (playerProvider == null)
        {
            Debug.LogError("CRÍTICO: playerProvider es NULL. El enemigo no sabe dónde está el jugador.");
            return;
        }

        if (movement == null)
        {
            Debug.LogError("CRÍTICO: IEnemyMovement no encontrado en el enemigo.");
            return;
        }

        if (movementConfig == null)
        {
            Debug.LogError("CRÍTICO: movementConfig es NULL en RetreatState.");
            return;
        }

        // 2. Obtener el destino (Zona Segura)
        retreatDestination = safeZoneProvider.GetRetreatPosition(
            enemy.transform.position,
            playerProvider.PlayerBodyPosition
        );

        // 3. Aplicar velocidad de pánico
        panicSpeed = Mathf.Max(0.1f, movementConfig.ChaseSpeed * 1.5f);

        // 4. Solicitar el movimiento inicial.
        // Se volverá a solicitar periódicamente porque MoveTo puede
        // ignorar una petición mientras el NavMeshAgent todavía calcula su path.
        RequestRetreatMovement();
    }

    public void Tick()
    {
        timer += Time.deltaTime;

        if (enemy == null || movement == null)
            return;

        // Comprobar si ya llegó al refugio.
        if (!hasReachedDestination)
        {
            float distanceToDestination = HorizontalDistance(
                enemy.transform.position,
                retreatDestination
            );

            if (distanceToDestination <= ArrivalDistance)
            {
                hasReachedDestination = true;
                movement.Stop();
                return;
            }

            // Reintentar periódicamente el movimiento.
            // Esto evita que el retiro falle si la primera llamada ocurrió
            // mientras el NavMeshAgent tenía el path pendiente.
            if (Time.time >= nextRepathTime)
            {
                RequestRetreatMovement();
            }
        }
        else
        {
            // Permanece quieto dentro del refugio hasta terminar la duración
            // del estado.
            movement.Stop();
        }
    }

    public void Exit()
    {
        movement?.Stop();
    }

    public bool IsRetreatComplete() => timer >= retreatDuration;

    // Expuesto para que la siguiente etapa del sistema pueda saber
    // cuándo el enemigo realmente alcanzó el refugio.
    public bool HasReachedDestination => hasReachedDestination;

    private void RequestRetreatMovement()
    {
        if (movement == null || hasReachedDestination)
            return;

        movement.MoveTo(retreatDestination, panicSpeed);
        nextRepathTime = Time.time + RepathInterval;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
