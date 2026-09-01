using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Systems References")]
    [SerializeField] private SanitySystem sanitySystem;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private ObjectiveSystem objectiveSystem;
    [SerializeField] private GameStateUI gameStateUI;
    [Header("UI Controllers")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private SanityUIController sanityUI;

    private void Awake()
    {
        InitializeSystems();
    }

    private void InitializeSystems()
    {
        if (inventoryUI != null && inventorySystem != null) inventoryUI.Initialize(inventorySystem);
        else Debug.LogWarning("UIManager: faltan referencias de inventario.");

        if (sanityUI != null && sanitySystem != null) sanityUI.Initialize(sanitySystem);
        else Debug.LogWarning("UIManager: faltan referencias de cordura.");

        if (objectiveSystem != null) objectiveSystem.AddObjective("find_key", "Encuentra la llave del s�tano");
        else Debug.LogWarning("UIManager: falta ObjectiveSystem.");

        if (sanitySystem != null && gameStateUI != null) sanitySystem.OnSanityDepleted += gameStateUI.OnSanityDepleted;
        else Debug.LogWarning("UIManager: no se pudo conectar cordura con GameStateUI.");
    }
}
