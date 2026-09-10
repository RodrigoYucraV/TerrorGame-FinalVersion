using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Componentes Básicos")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;

    public bool IsEmpty => _icon != null && _icon.sprite == null;

    private void Awake()
    {
        if (_icon == null)
        {
            Debug.LogError(
                $"[UI CRITICAL] Falta asignar la imagen '_icon' en {gameObject.name}.",
                gameObject
            );
        }
    }

    public void UpdateSlot(InventorySlot slotData)
    {
        if (_icon == null)
            return;

        bool hasItem = slotData != null && !slotData.IsEmpty;

        if (hasItem)
        {
            _icon.sprite = slotData.Icon;
            _icon.enabled = true;

            if (_nameText != null)
                _nameText.text = slotData.Data.displayName;
        }
        else
        {
            _icon.sprite = null;
            _icon.enabled = false;

            if (_nameText != null)
                _nameText.text = "";
        }

        if (_nameText != null)
            _nameText.gameObject.SetActive(hasItem);
    }
}
