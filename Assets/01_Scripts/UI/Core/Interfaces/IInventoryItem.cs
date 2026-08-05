using UnityEngine;

public interface IInventoryItem
{
    string ItemID { get; }
    Sprite Icon { get; }
    int Quantity { get; }
    bool IsEmpty { get; }
}
