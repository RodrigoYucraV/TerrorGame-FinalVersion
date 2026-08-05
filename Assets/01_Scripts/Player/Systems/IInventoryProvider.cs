using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryProvider
{
    event Action<int> OnSlotUpdated;
    InventorySlot GetSlot(int slotIndex);
    void UseItem(int slotIndex);
    void AddItemToSlot(string nameItem,int slotIndex, string itemID, int quantity = 1);
    bool HasItem(string itemID);
}

public interface IInventoryReader
{
    event Action<int> OnSlotUpdated;
    IInventoryItem GetSlotData(int slotIndex);
    bool HasItem(string itemID);
}

public interface IInventoryWriter
{
    void AddItemToSlot(string nameItem, int slotIndex, string itemID, int quantity = 1);
    void RemoveItem(int slotIndex);
    void UseItem(int slotIndex);
}