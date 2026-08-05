using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotUIController : MonoBehaviour
{
    private readonly InventoryHighlightController[] _highlightControllers;

    // Recibe los controllers de highlight, NO los slots UI
    public SlotUIController(InventoryHighlightController[] highlightControllers)
    {
        _highlightControllers = highlightControllers;
    }

    public void SetHighlight(int index, bool state)
    {
        if (IsValidIndex(index))
        {
            _highlightControllers[index].SetHighlight(state);
        }
    }

    private bool IsValidIndex(int index)=> index >= 0 && index < _highlightControllers.Length;
}
