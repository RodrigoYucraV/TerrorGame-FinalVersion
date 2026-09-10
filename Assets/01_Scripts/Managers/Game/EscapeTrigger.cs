using UnityEngine;

public class EscapeTrigger : MonoBehaviour
{
    [SerializeField] private GameStateUI gameStateUI;
    [SerializeField] private bool requireAllObjectives = true;
    [SerializeField] private ObjectiveSystem objectiveSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameStateUI == null)
            {
                gameStateUI = FindFirstObjectByType<GameStateUI>();
            }

            if (objectiveSystem == null)
            {
                objectiveSystem = FindFirstObjectByType<ObjectiveSystem>();
            }

            if (requireAllObjectives && objectiveSystem != null)
            {
                //if (!objectiveSystem.AllObjectivesCompleted())
                {
                    Debug.Log("[EscapeTrigger] Aún no has completado todos los objetivos.");
                    return;
                }
            }

            if (gameStateUI != null)
            {
                gameStateUI.ShowWinPanel();
                Debug.Log("[EscapeTrigger] ¡Escape exitoso! Victoria.");
            }
            else
            {
                Debug.LogWarning("[EscapeTrigger] No se encontró GameStateUI en la escena.");
            }
        }
    }
}
