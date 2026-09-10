using System;
using UnityEngine;

public interface IItemUsable
{
    string ItemID { get; }
    bool CanUse();
    void Use();
    string GetUseDescription();
}

[CreateAssetMenu(menuName = "Inventory/Consumable Item")]
public class ConsumableItemData : InventoryItemData
{
    public enum ConsumableType
    {
        Battery,
        Medkit,
        Key,
        Note
    }

    public ConsumableType consumableType;
    public float value = 30f; // Carga de batería o curación
    public string requiredKeyID; // Para puertas
    public string unlockMessage;
}

public class ConsumableSystem : MonoBehaviour
{
    private HealthSystem playerHealth;
    private FlashlightSystem flashlight;
    private InventorySystem inventory;

    private void Start()
    {
        playerHealth = PlayerManager.Instance?.GetComponent<HealthSystem>();
        flashlight = PlayerManager.Instance?.GetComponent<FlashlightSystem>();
        inventory = GetComponent<InventorySystem>();

        if (inventory != null)
        {
            // Evento para usar items
            // Se conectará desde el UI
        }
    }

    public bool UseConsumable(int slotIndex)
    {
        if (inventory == null) return false;

        var slot = inventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return false;

        var itemData = slot.Data as ConsumableItemData;
        if (itemData == null) return false;

        bool used = false;

        switch (itemData.consumableType)
        {
            case ConsumableItemData.ConsumableType.Battery:
                if (flashlight != null)
                {
                    var battery = flashlight.GetComponent<FlashLigthBattery>();
                    if (battery != null)
                    {
                        battery.Recharge(itemData.value);
                        used = true;
                        Debug.Log($"Batería recargada: +{itemData.value}");
                    }
                }
                break;

            case ConsumableItemData.ConsumableType.Medkit:
                if (playerHealth != null)
                {
                    playerHealth.Heal(itemData.value);
                    used = true;
                    Debug.Log($"Salud restaurada: +{itemData.value}");
                }
                break;
        }

        if (used)
        {
            inventory.UseItem(slotIndex);
        }

        return used;
    }
}