using System;
using UnityEngine;

public class EnemyDeathNotifier : MonoBehaviour
{
    public static event Action<GameObject> OnEnemyDied;

    private void OnDestroy()
    {
        OnEnemyDied?.Invoke(this.gameObject);
    }
}
