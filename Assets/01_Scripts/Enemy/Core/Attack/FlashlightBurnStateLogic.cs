using System;
using UnityEngine;

public class FlashlightBurnStateLogic : IEnemyState, IDisposable
{
    private readonly EnemyController enemy;
    private readonly IVisibilityController visualController;
    private readonly IRespawnHandler respawnHandler;
    private readonly EnemyStateMachine stateMachine;

    private float burnTimeElapsed = 0f;
    private const float TotalBurnDuration = 1.5f; // Duración del efecto de quemado/desaparición

    public FlashlightBurnStateLogic(EnemyController enemy, EnemyStateMachine stateMachine, IVisibilityController visuals, IRespawnHandler respawner)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.visualController = visuals;
        this.respawnHandler = respawner;

        // Suscribirse para saber cuándo terminó de reaparecer
        respawnHandler.OnRespawnComplete += HandleRespawnComplete;
    }

    public void Enter()
    {
        // Cuando entra: el enemigo se detiene y comienza a desaparecer
        enemy.GetComponent<IEnemyMovement>().Stop();
        visualController.FadeOut(TotalBurnDuration);
        burnTimeElapsed = 0f;
    }

    public void Tick()
    {
        burnTimeElapsed += Time.deltaTime;

        if (burnTimeElapsed >= TotalBurnDuration)
        {
            // 1. Asegurar la invisibilidad y reaparecer
            visualController.SetVisibility(false);
            respawnHandler.Respawn(enemy.transform);
            // La transición al nuevo estado se hará en HandleRespawnComplete
        }
    }

    public void Exit()
    {
        // Asegurarse de que el enemigo esté visible y el efecto se detenga
        visualController.SetVisibility(true);
    }

    private void HandleRespawnComplete()
    {
        visualController.FadeIn(0.5f);
        stateMachine.SetState(new StealthFollowStateLogic(
            enemy,
            enemy.GetComponent<IPlayerDetector>(),          
            enemy.GetComponent<IPlayerPositionProvider>(),
            enemy.GetComponent<IFollowSettings>(),          
            enemy.GetComponent<IEnemyMovementConfig>(),      
            false                                           
        ));
    }

    public void Dispose()
    {
        respawnHandler.OnRespawnComplete -= HandleRespawnComplete;
    }
}