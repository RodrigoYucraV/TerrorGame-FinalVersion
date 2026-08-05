using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Game/Mission Objective")]
public class MissionObjective : ScriptableObject
{
    [Header("Datos UI")]
    public string title; // Ej: "Busca una salida"
    [TextArea] public string description; // Ej: "El guardia perdió las llaves..."

    [Header("Condición de Completado")]
    public InventoryItemData requiredItem; // ¿Qué ítem completa esto? (Null si es por zona)
    public bool completesLevel; // ¿Es el último?
}