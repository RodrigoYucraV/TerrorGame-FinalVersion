
using UnityEngine;

public class CaptureStateLogic : IEnemyState
{
    private readonly EnemyController enemy;
    private readonly IEnemyMovement movement;
    private readonly PlayerCaptureController playerCaptureController;

    private bool captureStarted;

    public CaptureStateLogic(
        EnemyController enemy,
        IEnemyMovement movement,
        PlayerCaptureController playerCaptureController)
    {
        this.enemy = enemy;
        this.movement = movement;
        this.playerCaptureController = playerCaptureController;
    }

    public void Enter()
    {
        captureStarted = false;

        // Detener al enemigo durante la captura.
        movement?.Stop();

        if (enemy == null)
        {
            Debug.LogError(
                "[CaptureStateLogic] EnemyController no asignado."
            );
            return;
        }

        if (playerCaptureController == null)
        {
            Debug.LogError(
                "[CaptureStateLogic] PlayerCaptureController no asignado."
            );
            return;
        }

        // Iniciar la captura del jugador.
        captureStarted = playerCaptureController.StartCapture();

        if (!captureStarted)
        {
            Debug.LogWarning(
                "[CaptureStateLogic] No se pudo iniciar la captura."
            );
            return;
        }

        Debug.Log("[Enemy] Captura iniciada.");
    }

    public void Tick()
    {
        if (!captureStarted)
            return;

        // La captura permanece activa.
        // La salida se implementará cuando conectemos
        // la duración de la absorción y la mecánica de escape.
    }

    public void Exit()
    {
        // No liberamos al jugador automáticamente aquí.
        // La liberación deberá producirse cuando termine
        // la secuencia de captura o el jugador consiga escapar.
    }
}