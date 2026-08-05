using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour, IInventoryProvider
{
    public event Action<int> OnSlotUpdated = delegate { };

    // 1. DECLARAMOS EL EVENTO QUE FALTA
    public event Action<InventoryItemData> OnItemAdded; // <--- ¡ESTA LÍNEA FALTABA!

    [Header("Configuración")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private int slotCount = 4;

    // ESTADO
    [SerializeField] private InventorySlot[] slots;

    public int SlotCount => slots.Length;

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
        else Debug.LogError("CRÍTICO: ¡No has asignado la ItemDatabase en el InventorySystem!");
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

        InventoryItemData data = itemDatabase.GetItem(itemID);
        if (data == null) return;

        InventorySlot slot = slots[slotIndex];

        if (slot.IsEmpty)
        {
            slot.SetItem(data, quantity);
        }
        else if (slot.ItemID == itemID && data.isStackable)
        {
            slot.AddQuantity(quantity);
        }
        else
        {
            Debug.LogWarning($"Sobrescribiendo slot {slotIndex} con {itemID}");
            slot.SetItem(data, quantity);
        }

        OnSlotUpdated?.Invoke(slotIndex);

        // 2. DISPARAMOS EL EVENTO AL AGREGAR
        OnItemAdded?.Invoke(data); // <--- ¡ESTA LÍNEA FALTABA!
    }

    public bool AddItem(string itemID, int quantity = 1)
    {
        InventoryItemData data = itemDatabase.GetItem(itemID);
        if (data == null) return false;

        // Intentar apilar
        if (data.isStackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].ItemID == itemID)
                {
                    slots[i].AddQuantity(quantity);
                    OnSlotUpdated?.Invoke(i);
                    OnItemAdded?.Invoke(data); // <--- ¡ESTA LÍNEA FALTABA!

                    return true;
                }
            }
        }

        // Intentar hueco vacío
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].SetItem(data, quantity);
                OnSlotUpdated?.Invoke(i);
                OnItemAdded?.Invoke(data); // <--- ¡ESTA LÍNEA FALTABA!

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
