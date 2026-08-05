using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text quantityText;

    // Configuración basada en datos
    public void Setup(string itemID, int quantity)
    {
        // Obtener datos del ScriptableObject (ej: Resources.Load<InventoryItemData>($"Items/{itemID}"))
        //iconImage.sprite = /* ... */;
        quantityText.text = quantity.ToString();
    }
}
