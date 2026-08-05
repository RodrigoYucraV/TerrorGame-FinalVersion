using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Data")]
public class InventoryItemData : ScriptableObject
{
    public string itemID;        // ID único (ej: "llave_sotano")
    public string displayName;   // Nombre visible (ej: "Llave Oxidada")
    public Sprite icon;         
    [TextArea] public string description;
    public GameObject prefab;   
    public bool isStackable;     
    public int maxStack = 1;    

}