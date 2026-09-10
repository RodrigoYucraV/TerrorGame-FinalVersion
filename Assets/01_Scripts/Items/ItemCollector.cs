using System.Collections;
using UnityEngine;

public class ItemCollector : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private InventoryItemData _itemData;
    public string ItemID => _itemData != null ? _itemData.itemID : string.Empty;
    [SerializeField] private float highlightDuration = 0.5f;

    private IInventoryProvider _inventory;
    private bool _canCollect = true;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _inventory = player.GetComponent<IInventoryProvider>();
            if (_inventory == null)
            {
                Debug.LogError("�El jugador no tiene un IInventoryProvider!");
            }
        }
        else
        {
            Debug.LogError("�No se encontr� un GameObject con tag 'Player'!");
        }
    }

    public string GetInteractionPrompt()
    {
        return _itemData != null ? $"Recoger {_itemData.displayName}" : "Recoger objeto";
    }

    public void OnInteract()
    {
        OnGrabbed(null);
    }

    public void OnGrabbed(Transform holder)
    {
        if (!_canCollect) return;
        if (_itemData == null)
        {
            Debug.LogError($"ItemCollector: falta InventoryItemData en {name}.");
            return;
        }

        int emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            StartCoroutine(CollectItemProcess(emptySlot));
        }
    }

 private IEnumerator CollectItemProcess(int slotIndex)
    {
        // Mostrar animación del item recogido
        if (inventoryUI != null)
        {
            inventoryUI.HighlightSlot(slotIndex, true);
        }

        // Esperar únicamente el tiempo necesario para completar la animación
        yield return new WaitForSeconds(highlightDuration);

        if (_inventory == null)
            yield break;

        // Agregar el item al inventario
        _inventory.AddItemToSlot(
            _itemData.displayName,
            slotIndex,
            ItemID,
            1
        );

        // IMPORTANTE:
        // No volver a tocar la animación del icono aquí.
        // Solo terminar la selección del slot.
        if (inventoryUI != null)
        {
            inventoryUI.HighlightSlot(slotIndex, false);
        }

        Destroy(gameObject);
    }



    private int FindEmptySlot()
    {
        if (_inventory == null) return -1;

        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            InventorySlot slot = _inventory.GetSlot(i);
            if (slot != null && slot.IsEmpty) return i;
        }
        Debug.LogWarning("Inventario lleno!");
        return -1;
    }

    public void OnReleased() { }
}
