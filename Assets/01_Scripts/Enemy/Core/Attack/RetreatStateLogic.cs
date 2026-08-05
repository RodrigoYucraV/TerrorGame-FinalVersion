using UnityEngine;
using UnityEngine.AI;

public class RetreatStateLogic : IEnemyState
{
    private readonly EnemyController enemy;
    private readonly IEnemyMovement movement;
    private readonly ISafeZoneProvider safeZoneProvider;
    private readonly IPlayerPositionProvider playerProvider;
    private readonly IEnemyMovementConfig movementConfig;

    private float retreatDuration;
    private float timer;
    private Vector3 retreatDestination;

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
        this.retreatDuration = duration;
        this.movement = enemy.GetComponent<IEnemyMovement>();
        this.timer = 0f;
    }

    public void Enter()
    {
        // 1. GUARDIA DE SEGURIDAD (Evita el Crash)
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

        // 2. Obtener el destino (Zona Segura)
        retreatDestination = safeZoneProvider.GetRetreatPosition(
             enemy.transform.position,
             playerProvider.PlayerBodyPosition
         );

        // 3. Aplicar velocidad de pánico
        float panicSpeed = movementConfig.ChaseSpeed * 1.5f; // Ajustado a 1.5x, 3.0x suele ser demasiado y rompe el NavMesh

        // 4. Moverse UNA SOLA VEZ hacia el destino final
        if (movement != null)
        {
            movement.MoveTo(retreatDestination, panicSpeed);
        }
    }

    public void Tick()
    {
        timer += Time.deltaTime;
    }

    public void Exit()
    {
        if (movement != null) movement.Stop();
    }

    // Método helper para el controller (no es de la interfaz, pero se usa)
    public bool IsRetreatComplete() => timer >= retreatDuration;
}
