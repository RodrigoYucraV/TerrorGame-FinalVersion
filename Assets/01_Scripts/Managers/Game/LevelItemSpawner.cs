using System.Collections.Generic;
using System.Linq; // Necesario para filtrar listas (Where, ToList)
using UnityEngine;

public class LevelItemSpawner : MonoBehaviour
{
    [Header("Configuración de Dificultad")]
    [SerializeField] private List<DifficultyProfile> difficultyLevels;

    [Tooltip("0 = Fácil, 1 = Normal, 2 = Difícil")]
    [SerializeField] private int selectedDifficultyIndex = 0;

    [Header("Organización")]
    [SerializeField] private Transform itemsContainer; // ✅ MEJORA 1: Para no ensuciar la jerarquía

    // Referencia interna cacheada
    private List<ItemSpawnConfig> _currentItemsToSpawn;

    private void Start()
    {
        // Si no asignaste un contenedor manual, usamos este mismo objeto para agrupar
        if (itemsContainer == null) itemsContainer = transform;

        SelectDifficultyProfile();
        SpawnAllItems();
    }

    private void SelectDifficultyProfile()
    {
        if (difficultyLevels == null || difficultyLevels.Count == 0)
        {
            Debug.LogError("[Spawner] ❌ ERROR CRÍTICO: No hay perfiles de dificultad asignados.");
            return;
        }

        // Clamp para evitar errores de índice fuera de rango
        selectedDifficultyIndex = Mathf.Clamp(selectedDifficultyIndex, 0, difficultyLevels.Count - 1);

        // Cargamos la configuración desde el ScriptableObject
        _currentItemsToSpawn = difficultyLevels[selectedDifficultyIndex].waveConfig;

        Debug.Log($"[Spawner] ✅ Dificultad cargada: {difficultyLevels[selectedDifficultyIndex].name}");
    }

    private void SpawnAllItems()
    {
        if (_currentItemsToSpawn == null) return;

        foreach (var config in _currentItemsToSpawn)
        {
            SpawnSpecificItem(config);
        }
    }

    private void SpawnSpecificItem(ItemSpawnConfig config)
    {
        // ✅ MEJORA 2: Validación de Seguridad. 
        // Si la data está vacía o no tiene prefab, saltamos para no romper el juego.
        if (config.itemData == null || config.itemData.prefab == null)
        {
            Debug.LogWarning($"[Spawner] ⚠️ El item '{config.name}' tiene la Data o el Prefab vacíos. Se omitió.");
            return;
        }

        // Verificamos si hay puntos disponibles en la lista estática
        if (SpawnPoint.AllPoints.Count == 0)
        {
            Debug.LogWarning("[Spawner] ⚠️ No hay SpawnPoints registrados en la escena.");
            return;
        }

        // Filtramos puntos válidos (Categoría correcta + No ocupados)
        var validPoints = SpawnPoint.AllPoints
            .Where(p => !p.IsOccupied && (p.category == config.category || p.category == SpawnCategory.General))
            .ToList();

        // Check si nos faltan puntos para la cantidad deseada
        if (validPoints.Count < config.amountToSpawn)
        {
            Debug.LogWarning($"[Spawner] ⚠️ Faltan puntos para '{config.name}'. Se pidieron {config.amountToSpawn}, hay {validPoints.Count} libres.");
        }

        Shuffle(validPoints);

        int count = Mathf.Min(config.amountToSpawn, validPoints.Count);

        for (int i = 0; i < count; i++)
        {
            SpawnPoint point = validPoints[i];

            // ✅ MEJORA 1 (Aplicada): Instanciamos como HIJO del itemsContainer
            Instantiate(config.itemData.prefab, point.transform.position, point.transform.rotation, itemsContainer);

            point.SetOccupied(true);
        }
    }

    // Método vital para el Director (Balanceo Kari Kari)
    public int GetTotalCountForItem(InventoryItemData dataToSearch)
    {
        // Lazy Initialization: Si el Director pregunta antes del Start, cargamos la dificultad al vuelo
        if (_currentItemsToSpawn == null) SelectDifficultyProfile();
        if (_currentItemsToSpawn == null) return 0; // Si sigue null, retornamos 0

        int total = 0;
        foreach (var config in _currentItemsToSpawn)
        {
            if (config.itemData == dataToSearch)
            {
                total += config.amountToSpawn;
            }
        }
        return total;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Utilidad para pruebas rápidas
    public void SetDifficultyIndex(int index)
    {
        selectedDifficultyIndex = index;
    }
}