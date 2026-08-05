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
        inventoryUI.Initialize(inventorySystem);
        sanityUI.Initialize(sanitySystem);
        objectiveSystem.AddObjective("find_key", "Encuentra la llave del sótano");
        sanitySystem.OnSanityDepleted += gameStateUI.OnSanityDepleted;
    }
}
