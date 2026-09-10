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

    [Header("Visibilidad")]
    [SerializeField] private InventoryVisibilityController _visibilityController;


    private IInventoryProvider _inventory;

    private int _selectedSlotIndex = 0;


    // ============================================================
    // INITIALIZE
    // ============================================================

    public void Initialize(
        IInventoryProvider inventory)
    {
        _inventory = inventory;

        _inventory.OnSlotUpdated += UpdateSlot;

        // Actualizar los slots sin reproducir
        // animaciones de pickup.
        UpdateAllSlots();

        // Seleccionar primer slot.
        SelectSlot(0);
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        HandleScrollInput();
    }


    // ============================================================
    // SCROLL
    // ============================================================

    private void HandleScrollInput()
    {
        float scroll =
            Input.GetAxis("Mouse ScrollWheel");

        if (scroll == 0f)
            return;


        // --------------------------------------------------------
        // CAMBIAR SLOT
        // --------------------------------------------------------

        if (scroll > 0)
            _selectedSlotIndex--;
        else
            _selectedSlotIndex++;


        // --------------------------------------------------------
        // HACER CICLO
        // --------------------------------------------------------

        if (_selectedSlotIndex < 0)
        {
            _selectedSlotIndex =
                _slots.Length - 1;
        }

        if (_selectedSlotIndex >= _slots.Length)
        {
            _selectedSlotIndex = 0;
        }


        // --------------------------------------------------------
        // SELECCIONAR
        // --------------------------------------------------------

        SelectSlot(
            _selectedSlotIndex
        );


        // --------------------------------------------------------
        // MOSTRAR INVENTARIO
        // --------------------------------------------------------

        if (_visibilityController != null)
        {
            _visibilityController.ShowBriefly();
        }
    }


    // ============================================================
    // SELECT SLOT
    // ============================================================

    private void SelectSlot(int index)
    {
        // Apagar todos los highlights.

        for (int i = 0;
             i < _slots.Length;
             i++)
        {
            HighlightSlot(
                i,
                false
            );
        }


        // Encender seleccionado.

        HighlightSlot(
            index,
            true
        );
    }


    // ============================================================
    // UPDATE SLOT
    // ============================================================

    private void UpdateSlot(int slotIndex)
    {
        UpdateSlotInternal(
            slotIndex,
            true
        );
    }


    // ============================================================
    // UPDATE SLOT INTERNAL
    // ============================================================

    private void UpdateSlotInternal(
        int slotIndex,
        bool playPickupAnimation)
    {
        if (!IsValidIndex(slotIndex))
            return;


        var slot =
            _inventory.GetSlot(
                slotIndex
            );


        // --------------------------------------------------------
        // ACTUALIZAR SLOT NORMAL
        // --------------------------------------------------------

        _slots[slotIndex]
            .UISlot
            .UpdateSlot(slot);


        // --------------------------------------------------------
        // PICKUP
        // --------------------------------------------------------

        if (!slot.IsEmpty &&
            playPickupAnimation)
        {
            // ----------------------------------------------------
            // PRIMERO:
            // Mostrar TODA la UI inmediatamente.
            // ----------------------------------------------------

            if (_visibilityController != null)
            {
                _visibilityController
                    .ShowImmediately();
            }


            // ----------------------------------------------------
            // DESPUÉS:
            // Ejecutar animación del item.
            // ----------------------------------------------------

            if (_slots[slotIndex]
                    .HighlightController != null)
            {
                _slots[slotIndex]
                    .HighlightController
                    .PlayPickupAnimation(
                        slot.Data
                    );
            }
        }
    }


    // ============================================================
    // HIGHLIGHT SLOT
    // ============================================================

    public void HighlightSlot(
        int slotIndex,
        bool state)
    {
        if (!IsValidIndex(slotIndex))
            return;

        if (_slots[slotIndex]
                .HighlightController == null)
            return;

        _slots[slotIndex]
            .HighlightController
            .SetHighlight(state);
    }


    // ============================================================
    // UPDATE ALL SLOTS
    // ============================================================

    private void UpdateAllSlots()
    {
        for (int i = 0;
             i < _slots.Length;
             i++)
        {
            // IMPORTANTE:
            //
            // false = actualizar visualmente
            // pero NO reproducir pickup.
            //
            UpdateSlotInternal(
                i,
                false
            );
        }
    }


    // ============================================================
    // VALIDAR ÍNDICE
    // ============================================================

    private bool IsValidIndex(int index)
    {
        return index >= 0 &&
               index < _slots.Length;
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        if (_inventory != null)
        {
            _inventory.OnSlotUpdated -= UpdateSlot;
        }
    }
}