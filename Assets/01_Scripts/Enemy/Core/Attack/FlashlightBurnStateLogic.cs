using System;
using UnityEngine;

public class FlashlightBurnStateLogic : IEnemyState, IDisposable
{
    private readonly EnemyController enemy;
    private readonly IVisibilityController visualController;
    private readonly IRespawnHandler respawnHandler;

    private const float TotalBurnDuration = 1.5f;
    private float burnTimeElapsed;
    private bool respawnRequested;

    public FlashlightBurnStateLogic(
        EnemyController enemy,
        IVisibilityController visuals,
        IRespawnHandler respawner)
    {
        this.enemy = enemy;
        this.visualController = visuals;
        this.respawnHandler = respawner;

        if (respawnHandler != null)
            respawnHandler.OnRespawnComplete += HandleRespawnComplete;
    }

    public void Enter()
    {
        burnTimeElapsed = 0f;
        respawnRequested = false;

        // El enemigo ya llegó al refugio antes de entrar aquí.
        // Este estado se encarga únicamente de desaparecer y reaparecer.
        visualController?.FadeOut(TotalBurnDuration);
    }

    public void Tick()
    {
        burnTimeElapsed += Time.deltaTime;

        if (burnTimeElapsed < TotalBurnDuration)
            return;

        if (respawnRequested)
            return;

        respawnRequested = true;

        // El objeto permanece invisible mientras EnemyManager
        // mueve el MISMO Transform a la nueva posición.
        visualController?.SetVisibility(false);

        respawnHandler?.Respawn(enemy.transform);
    }

    public void Exit()
    {
        // No forzamos visibilidad aquí.
        // HandleRespawnComplete controla la aparición mediante FadeIn().
    }

    private void HandleRespawnComplete()
    {
        // El transform ya fue trasladado por EnemyManager.
        // Se mantiene invisible y se inicia la aparición progresiva.
        visualController?.SetVisibility(false);
        visualController?.FadeIn(0.5f);

        enemy.ReturnToStealthAfterRespawn();
    }

    public void Dispose()
    {
        if (respawnHandler != null)
            respawnHandler.OnRespawnComplete -= HandleRespawnComplete;
    }
}
