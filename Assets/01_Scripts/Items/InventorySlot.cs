using UnityEngine;
[System.Serializable]
public class InventorySlot : IInventoryItem
{
    [SerializeField] private InventoryItemData _itemData;
    [SerializeField] private int _quantity;

    // Propiedades de la Interfaz
    public string ItemID => _itemData != null ? _itemData.itemID : "";
    public Sprite Icon => _itemData != null ? _itemData.icon : null;
    public int Quantity => _quantity;
    public bool IsEmpty => _itemData == null || _quantity <= 0;

    // Acceso directo a datos (útil para el sistema)
    public InventoryItemData Data => _itemData;

    public void SetItem(InventoryItemData data, int amount)
    {
        _itemData = data;
        _quantity = amount;
    }

    public void AddQuantity(int amount)
    {
        _quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        _quantity -= amount;
        if (_quantity <= 0) Clear();
    }

    public void Clear()
    {
        _itemData = null;
        _quantity = 0;
    }
}
