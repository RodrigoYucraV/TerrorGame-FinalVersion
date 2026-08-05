using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<InventoryItemData> allItems;

    // Diccionario para búsquedas instantáneas (O(1))
    private Dictionary<string, InventoryItemData> _itemLookup;

    public void Initialize()
    {
        _itemLookup = new Dictionary<string, InventoryItemData>();

        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemID))
            {
                if (!_itemLookup.ContainsKey(item.itemID))
                {
                    _itemLookup.Add(item.itemID, item);
                }
                else
                {
                    Debug.LogWarning($"[ItemDatabase] ID Duplicado detectado: {item.itemID}");
                }
            }
        }
        Debug.Log($"[ItemDatabase] Inicializada con {allItems.Count} ítems.");
    }

    public InventoryItemData GetItem(string id)
    {
        if (_itemLookup == null) Initialize();

        if (_itemLookup.TryGetValue(id, out InventoryItemData item))
        {
            return item;
        }

        Debug.LogError($"[ItemDatabase] El ítem con ID '{id}' no se encuentra en la base de datos.");
        return null;
    }
}