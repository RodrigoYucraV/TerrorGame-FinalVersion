using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; 

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [Tooltip("Prefab del enemigo a instanciar (Debe tener EnemyDeathNotifier.cs)")]
    [SerializeField] private GameObject prefabEnemy;

    [Tooltip("Número total de enemigos a spawnear inicialmente.")]
    [SerializeField] private int numObjects = 10;

    [Tooltip("Radio de spawn alrededor de este objeto Manager.")]
    [SerializeField] private float radius = 5f;

    private int activeEnemiesCount;

    private void Start()
    {
        EnemyDeathNotifier.OnEnemyDied += HandleEnemyDeath;
        SpawnObjects();
    }

    private void OnDestroy()
    {
        EnemyDeathNotifier.OnEnemyDied -= HandleEnemyDeath;
    }

    private void SpawnObjects()
    {
        if (prefabEnemy == null)
        {
            Debug.LogError("El Prefab del enemigo no está asignado en EnemySpawnManager.");
            return;
        }

        for (int i = 0; i < numObjects; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2);

            // Usa coseno y seno para obtener un punto en el círculo
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            float y = transform.position.y;
            Vector3 position = transform.position + new Vector3(x, y, z);

            // Instanciar el enemigo
            GameObject enemy = Instantiate(prefabEnemy, position, Quaternion.identity);
        }

        activeEnemiesCount = numObjects;
        Debug.Log($"[Spawn] {numObjects} enemigos spawneados.");
    }

    private void HandleEnemyDeath(GameObject diedEnemy)
    {
        // Esta función se llama SOLO cuando un enemigo muere
        activeEnemiesCount--;
        Debug.Log($"[Death] Enemigo destruido. Restantes: {activeEnemiesCount}");

        if (activeEnemiesCount <= 0)
        {
            // Lógica de "todos destruidos"
            Debug.Log("¡Todos los enemigos han sido destruidos! Reiniciando spawn...");

            // Ejemplo: Esperar un momento y volver a spawnear todo el grupo
            StartCoroutine(RespawnAllAfterDelay(5f));
        }
    }

    private IEnumerator RespawnAllAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Iniciando nueva oleada de enemigos.");
        SpawnObjects();
    }
}