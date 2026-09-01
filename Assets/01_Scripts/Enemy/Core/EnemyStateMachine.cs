using System;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private IEnemyState currentState;

    public void SetState(IEnemyState newState)
    {
        currentState?.Exit();
        if (currentState is IDisposable disposableState) disposableState.Dispose();

        currentState = newState;
        currentState?.Enter();
    }

    public void ManualTick()
    {
        currentState?.Tick();
    }
}
