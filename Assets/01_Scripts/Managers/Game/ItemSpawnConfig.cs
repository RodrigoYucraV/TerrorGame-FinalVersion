using System.Collections.Generic;
using UnityEngine;

// 1. Categorías para saber qué puede ir en cada punto
public enum SpawnCategory
{
    General,    // Mesas, suelo (Cualquier cosa)
    Battery,    // Estanterías de electrónica
    Key,        // Clavos en la pared, escritorios importantes
    Healing     // Botiquines
}

// 2. La configuración de la oleada (Ahora usa tu InventoryItemData)
[System.Serializable]
public class ItemSpawnConfig
{
    public string name;             // Solo para que ordenes en el Inspector (ej: "Llaves Nivel 1")

    // ✅ EL CAMBIO CLAVE: Referenciamos tu Data, no el prefab directo
    public InventoryItemData itemData;

    public SpawnCategory category;  // ¿Dónde puede aparecer?
    public int amountToSpawn;       // Cantidad
}
