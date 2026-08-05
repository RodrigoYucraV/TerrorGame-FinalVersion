using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollector : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private InventoryItemData _itemData;
    public string ItemID => _itemData.itemID;
    [SerializeField] private float highlightDuration = 0.5f;

    private IInventoryProvider _inventory;
    private bool _canCollect = true;

    private void Awake()
    {
        // Buscar el InventorySystem de forma segura
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _inventory = player.GetComponent<IInventoryProvider>();
            if (_inventory == null)
            {
                Debug.LogError("¡El jugador no tiene un IInventoryProvider!");
            }
        }
        else
        {
            Debug.LogError("¡No se encontró un GameObject con tag 'Player'!");
        }
    }

    public void OnGrabbed(Transform holder)
    {
        if (!_canCollect) return;

        int emptySlot = FindEmptySlot();
        if (emptySlot != -1)
        {
            StartCoroutine(CollectItemProcess(emptySlot));
        }
    }

    private IEnumerator CollectItemProcess(int slotIndex)
    {

        inventoryUI.HighlightSlot(slotIndex, true);
        yield return new WaitForSeconds(highlightDuration);

        // Añadir ítem
        if (_inventory != null)
        {
            _inventory.AddItemToSlot(_itemData.displayName,slotIndex,ItemID, 1);
            inventoryUI.HighlightSlot(slotIndex, false);
            Destroy(gameObject);
        }
    }

    private int FindEmptySlot()
    {
        if (_inventory == null) return -1;

        for (int i = 0; i < 4; i++)
        {
            if (_inventory.GetSlot(i).IsEmpty) return i;
        }
        Debug.LogWarning("Inventario lleno!");
        return -1;
    }

    public void OnReleased() { }
}