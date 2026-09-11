using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackDetector : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    private readonly HashSet<Collider> playerCollidersInside = new();

    public bool IsPlayerInside => playerCollidersInside.Count > 0;

    private void Awake()
    {
        if (enemyController == null)
            enemyController = GetComponentInParent<EnemyController>();

        if (enemyController == null)
        {
            Debug.LogError(
                "EnemyAttackDetector: No se encontró EnemyController en el padre."
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        bool wasInside = IsPlayerInside;

        playerCollidersInside.Add(other);

        // Solo queremos disparar la entrada la primera vez.
        if (wasInside)
            return;

        Debug.Log("[Enemy] Jugador entró en DetectionAttack.");

        enemyController.GoToAttackState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerCollidersInside.Remove(other);

        // Solo mostramos salida cuando ya no queda
        // ningún collider del jugador dentro.
        if (!IsPlayerInside)
        {
            Debug.Log("[Enemy] Jugador salió de DetectionAttack.");
        }
    }

    private bool IsPlayer(Collider other)
    {
        return other.GetComponentInParent<PlayerManager>() != null;
    }
}