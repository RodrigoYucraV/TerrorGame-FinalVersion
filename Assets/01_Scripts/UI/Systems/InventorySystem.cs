using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour, IInventoryProvider
{
    public event Action<int> OnSlotUpdated = delegate { };

    // 1. DECLARAMOS EL EVENTO QUE FALTA
    public event Action<InventoryItemData> OnItemAdded; // <--- �ESTA L�NEA FALTABA!

    [Header("Configuraci�n")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private int slotCount = 4;

    // ESTADO
    [SerializeField] private InventorySlot[] slots;

    public int SlotCount => slots != null ? slots.Length : slotCount;

    private void Awake()
    {
        if (slots == null || slots.Length != slotCount)
        {
            slots = new InventorySlot[slotCount];
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) slots[i] = new InventorySlot();
        }

        if (itemDatabase != null) itemDatabase.Initialize();
        else Debug.LogError("CR�TICO: �No has asignado la ItemDatabase en el InventorySystem!");
    }

    public InventorySlot GetSlot(int slotIndex) => IsValidSlot(slotIndex) ? slots[slotIndex] : null;

    public IInventoryItem GetSlotData(int slotIndex) => GetSlot(slotIndex);

    public void UseItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || slots[slotIndex].IsEmpty) return;
        slots[slotIndex].RemoveQuantity(1);
        OnSlotUpdated?.Invoke(slotIndex);
    }

    public bool HasItem(string itemID)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.ItemID == itemID) return true;
        }
        return false;
    }

    public void AddItemToSlot(string nameItem, int slotIndex, string itemID, int quantity = 1)
    {
        if (!IsValidSlot(slotIndex)) return;
        if (itemDatabase == null) return;

        InventoryItemData data = itemDatabase.GetItem(itemID);
        if (data == null) return;
        quantity = Mathf.Max(1, quantity);

        InventorySlot slot = slots[slotIndex];

        if (slot.IsEmpty)
        {
            int amountToAdd = data.isStackable ? Mathf.Min(quantity, Mathf.Max(1, data.maxStack)) : 1;
            slot.SetItem(data, amountToAdd);
        }
        else if (slot.ItemID == itemID && data.isStackable)
        {
            int availableSpace = Mathf.Max(0, data.maxStack - slot.Quantity);
            if (availableSpace <= 0) return;

            slot.AddQuantity(Mathf.Min(quantity, availableSpace));
        }
        else
        {
            Debug.LogWarning($"Slot {slotIndex} ocupado. No se sobrescribió con {itemID}.");
            return;
        }

        OnSlotUpdated?.Invoke(slotIndex);

        // 2. DISPARAMOS EL EVENTO AL AGREGAR
        OnItemAdded?.Invoke(data); // <--- �ESTA L�NEA FALTABA!
    }

    public bool AddItem(string itemID, int quantity = 1)
    {
        if (itemDatabase == null) return false;

        InventoryItemData data = itemDatabase.GetItem(itemID);
        if (data == null) return false;
        quantity = Mathf.Max(1, quantity);

        // Intentar apilar
        if (data.isStackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemID == itemID)
                {
                    int availableSpace = Mathf.Max(0, data.maxStack - slots[i].Quantity);
                    if (availableSpace <= 0) continue;

                    slots[i].AddQuantity(Mathf.Min(quantity, availableSpace));
                    OnSlotUpdated?.Invoke(i);
                    OnItemAdded?.Invoke(data); // <--- �ESTA L�NEA FALTABA!

                    return true;
                }
            }
        }

        // Intentar hueco vac�o
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                int amountToAdd = data.isStackable ? Mathf.Min(quantity, Mathf.Max(1, data.maxStack)) : 1;
                slots[i].SetItem(data, amountToAdd);
                OnSlotUpdated?.Invoke(i);
                OnItemAdded?.Invoke(data); // <--- �ESTA L�NEA FALTABA!

                return true;
            }
        }

        Debug.Log("Inventario lleno.");
        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        if (IsValidSlot(slotIndex))
        {
            slots[slotIndex].Clear();
            OnSlotUpdated?.Invoke(slotIndex);
        }
    }

    private bool IsValidSlot(int index) => index >= 0 && index < slots.Length;
}
