using UnityEngine;

public class LevelPhaseTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private EnemyPhase phaseToTrigger;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {// 1. ¿Detectamos colisión?
        Debug.Log($"[Trigger] Algo entró: {other.gameObject.name} | Tag: {other.tag}");

        if (triggerOnce && hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[Trigger] ¡Es el JUGADOR!");

            // Buscamos TODOS los enemigos activos, no solo uno
            var allEnemies = FindObjectsByType<EnemyEvolutionManager>(FindObjectsSortMode.None);

            if (allEnemies.Length > 0)
            {
                Debug.Log($"[Trigger] Se encontraron {allEnemies.Length} enemigos. Evolucionando...");
                foreach (var enemy in allEnemies)
                {
                    enemy.SetPhase(phaseToTrigger);
                }
                hasTriggered = true;
            }
            else
            {
                Debug.LogError("[Trigger] CRÍTICO: No se encontró ningún 'EnemyEvolutionManager' en la escena. ¿El enemigo está vivo/activo?");
            }
        }
    }

    // Dibujar el cubo en el editor para verlo
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 1, 0.3f); // Morado semitransparente
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
