using UnityEngine;
using UnityEngine.AI;

public class EnemyPathDebug : MonoBehaviour
{
    private NavMeshAgent agent;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.yellow;
    }

    private void Update()
    {
        if (agent == null || !agent.hasPath)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Dibuja la ruta que el enemigo QUIERE tomar
        lineRenderer.positionCount = agent.path.corners.Length;
        lineRenderer.SetPositions(agent.path.corners);

        // Muestra el estado en consola si hay problemas
        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning("ENEMIGO ATASCADO: La ruta es Parcial (No llega al destino). ¿Falta un Link?");
        }
    }
}
