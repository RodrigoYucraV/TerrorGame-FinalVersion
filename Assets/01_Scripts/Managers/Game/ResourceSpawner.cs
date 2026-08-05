using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject resourcePrefab; // Renombrado de 'battery' a 'resource' para ser genérico
    [SerializeField] private Transform pointsContainer; // El padre "ItemsPoints"

    [Header("Configuración")]
    [SerializeField] private List<DifficultySpawnData> settings;
    [SerializeField] private int selectedDifficultyIndex = 0;

    // Ya no es serializada, se llena sola
    private List<Transform> validSpawnPoints = new List<Transform>();

    private void Awake()
    {
        // ✅ MEJORA: Recolectar puntos automáticamente al iniciar
        // Esto cumple mejor SRP: El spawner se encarga de buscar sus dependencias.
        GatherSpawnPoints();
    }

    private void Start()
    {
        SpawnResources();
    }

    private void GatherSpawnPoints()
    {
        if (pointsContainer == null)
        {
            Debug.LogError("¡Falta asignar el contenedor de puntos (ItemsPoints)!");
            return;
        }

        // Opción A: Buscar por componente (Más seguro y robusto)
        var points = pointsContainer.GetComponentsInChildren<SpawnPoint>();
        foreach (var point in points)
        {
            validSpawnPoints.Add(point.transform);
        }

        // Opción B (Si sigues usando GameObjects vacíos sin script):
        // foreach (Transform child in pointsContainer) { validSpawnPoints.Add(child); }
    }

    private void SpawnResources()
    {
        if (selectedDifficultyIndex >= settings.Count) return; // Tu fix de seguridad

        var data = settings[selectedDifficultyIndex];
        int countToSpawn = Mathf.Min(data.batteryCount, validSpawnPoints.Count);

        // Usamos una copia para no alterar la lista original por si quieres respawnear luego
        List<Transform> shuffledPoints = new List<Transform>(validSpawnPoints);
        Shuffle(shuffledPoints);

        for (int i = 0; i < countToSpawn; i++)
        {
            Instantiate(resourcePrefab, shuffledPoints[i].position, shuffledPoints[i].rotation);
        }

        Debug.Log($"[Spawner] Generados {countToSpawn} items.");
    }

    // Tu método Shuffle se mantiene igual...
    private void Shuffle<T>(List<T> list) { /* ... */ }
}
