using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventorySlotUI : MonoBehaviour
{
    [Header("Componentes Básicos")]
    [SerializeField] private Image _icon;
    //[SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private TextMeshProUGUI _nameText;
    public bool IsEmpty => _icon != null && _icon.sprite == null;
    // -----------------------------------------

    private void Awake()
    {
        if (_icon == null)
            Debug.LogError($"[UI CRITICAL] Falta asignar la imagen '_icon' en el objeto {gameObject.name}.", gameObject);

        //if (_quantityText == null)
            //Debug.LogError($"[UI CRITICAL] Falta asignar el texto '_quantityText' en el objeto {gameObject.name}.", gameObject);
    }

    public void UpdateSlot(InventorySlot slotData)
    {
        if (_icon == null) return;

        bool hasItem = slotData != null && !slotData.IsEmpty;

        _icon.gameObject.SetActive(hasItem);
        //if (_quantityText) _quantityText.gameObject.SetActive(hasItem);
        if (_nameText) _nameText.gameObject.SetActive(hasItem);

        if (hasItem)
        {
            _icon.sprite = slotData.Icon;
            //if (_quantityText) _quantityText.text = slotData.Quantity.ToString();
            if (_nameText) _nameText.text = slotData.Data.displayName;
        }
        else
        {
         
            _icon.sprite = null;
        }
    }
}


