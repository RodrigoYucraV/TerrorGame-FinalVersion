using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour, IEnemyMovement
{
    private NavMeshAgent agent;

    // Variables para optimización
    private Vector3 lastTargetPosition;
    private float updateThreshold = 0.5f;
    private bool hasPath = false;

    // Variables para Anti-Spam de Logs
    private float lastErrorTime = 0f;
    private float errorLogInterval = 2.0f; // Solo mostrar error cada 2 segundos

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lastTargetPosition = Vector3.positiveInfinity;
    }

    public void MoveTo(Vector3 targetPosition, float speed)
    {
        if (!agent.isOnNavMesh || !agent.isActiveAndEnabled) return;

        // PROTECCIÓN 1: Si ya está saltando (en el aire), NO le des órdenes.
        if (agent.isOnOffMeshLink) return;

        // PROTECCIÓN 2: Si está calculando una ruta compleja (PathPending), 
        // NO le des otra orden o cancelarás el cálculo actual y se quedará "pensando".
        if (agent.pathPending) return;

        agent.speed = speed;
        agent.isStopped = false;

        // PROTECCIÓN 3: Distancia mínima para recalcular.
        // Si el objetivo (jugador) se movió menos de 1 metro, NO recalculamos.
        // Esto es vital cuando el jugador se mueve un poco en el techo.
        if (hasPath && Vector3.Distance(targetPosition, lastTargetPosition) < 1.0f)
        {
            return;
        }

        // ... (Tu código de SamplePosition y SetDestination sigue igual) ...
        float sampleRadius = 10f;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            lastTargetPosition = targetPosition;
            hasPath = true;
        }
    }

    public void Stop()
    {
        // Verificamos isOnNavMesh para evitar errores si el agente fue desactivado
        if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            hasPath = false;
            lastTargetPosition = Vector3.positiveInfinity;
        }
    }
}
