using System;
using UnityEngine;

public class FlashlightBurnStateLogic : IEnemyState, IDisposable
{
    private readonly EnemyController enemy;
    private readonly IVisibilityController visualController;
    private readonly IRespawnHandler respawnHandler;

    private float burnTimeElapsed = 0f;
    private const float TotalBurnDuration = 1.5f; // Duraci�n del efecto de quemado/desaparici�n
    private bool respawnRequested;

    public FlashlightBurnStateLogic(EnemyController enemy, IVisibilityController visuals, IRespawnHandler respawner)
    {
        this.enemy = enemy;
        this.visualController = visuals;
        this.respawnHandler = respawner;

        // Suscribirse para saber cu�ndo termin� de reaparecer
        if (respawnHandler != null) respawnHandler.OnRespawnComplete += HandleRespawnComplete;
    }

    public void Enter()
    {
        // Cuando entra: el enemigo se detiene y comienza a desaparecer
        enemy.GetComponent<IEnemyMovement>()?.Stop();
        visualController?.FadeOut(TotalBurnDuration);
        burnTimeElapsed = 0f;
        respawnRequested = false;
    }

    public void Tick()
    {
        burnTimeElapsed += Time.deltaTime;

        if (burnTimeElapsed >= TotalBurnDuration && !respawnRequested)
        {
            respawnRequested = true;
            // 1. Asegurar la invisibilidad y reaparecer
            visualController?.SetVisibility(false);
            respawnHandler?.Respawn(enemy.transform);
            // La transici�n al nuevo estado se har� en HandleRespawnComplete
        }
    }

    public void Exit()
    {
        // Asegurarse de que el enemigo est� visible y el efecto se detenga
        visualController?.SetVisibility(true);
    }

    private void HandleRespawnComplete()
    {
        visualController?.SetVisibility(true);
        visualController?.FadeIn(0.5f);
        enemy.ReturnToStealthAfterRespawn();
    }

    public void Dispose()
    {
        if (respawnHandler != null) respawnHandler.OnRespawnComplete -= HandleRespawnComplete;
    }
}
