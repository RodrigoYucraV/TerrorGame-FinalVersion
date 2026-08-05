using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    private struct SlotData
    {
        public InventorySlotUI UISlot;
        public InventoryHighlightController HighlightController;
    }

    [Header("Referencias")]
    [SerializeField] private SlotData[] _slots;

    // Referencia al script que acabamos de crear arriba
    [SerializeField] private InventoryVisibilityController _visibilityController;

    private IInventoryProvider _inventory;
    private int _selectedSlotIndex = 0; // Para saber cuál estamos seleccionando

    public void Initialize(IInventoryProvider inventory)
    {
        _inventory = inventory;
        _inventory.OnSlotUpdated += UpdateSlot;

        // Inicialización
        UpdateAllSlots();
        SelectSlot(0); // Empezar seleccionando el primero
    }

    private void Update()
    {
        HandleScrollInput();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Cambiar índice basado en el scroll
            if (scroll > 0) _selectedSlotIndex--;
            else _selectedSlotIndex++;

            // Matemáticas para hacer ciclo (0 -> 1 -> 2 -> 3 -> 0)
            if (_selectedSlotIndex < 0) _selectedSlotIndex = _slots.Length - 1;
            if (_selectedSlotIndex >= _slots.Length) _selectedSlotIndex = 0;

            // Aplicar selección y mostrar UI
            SelectSlot(_selectedSlotIndex);

            // ¡AQUÍ ESTÁ LA MAGIA! Mostramos la UI al mover la rueda
            if (_visibilityController != null) _visibilityController.ShowBriefly();
        }
    }

    private void SelectSlot(int index)
    {
        // Apagar todos los highlights
        for (int i = 0; i < _slots.Length; i++)
        {
            HighlightSlot(i, false);
        }
        // Encender solo el seleccionado
        HighlightSlot(index, true);
    }

    private void UpdateSlot(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return;

        var slot = _inventory.GetSlot(slotIndex);
        _slots[slotIndex].UISlot.UpdateSlot(slot);

        if (!slot.IsEmpty)
        {
            // ✅ AQUÍ ESTÁ LA SOLUCIÓN:
            // Le pasamos la data del ítem que está dentro del 'slot'
            _slots[slotIndex].HighlightController.PlayPickupAnimation(slot.Data);

            // ¡MAGIA 2! Mostramos la UI al recoger algo
            if (_visibilityController != null) _visibilityController.ShowBriefly();
        }
    }

    // ... Resto de métodos (HighlightSlot, IsValidIndex, OnDestroy) se mantienen igual ...
    public void HighlightSlot(int slotIndex, bool state)
    {
        if (IsValidIndex(slotIndex))
        {
            _slots[slotIndex].HighlightController.SetHighlight(state);
        }
    }

    private void UpdateAllSlots()
    {
        for (int i = 0; i < _slots.Length; i++) UpdateSlot(i);
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _slots.Length;

    private void OnDestroy()
    {
        if (_inventory != null) _inventory.OnSlotUpdated -= UpdateSlot;
    }
}
