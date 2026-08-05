using System;
using System.Collections.Generic;
using UnityEngine;

public class DirectorLevel : MonoBehaviour
{
    public static DirectorLevel Instance { get; private set; }

    [Header("Configuración de Misiones")]
    [SerializeField] private List<MissionObjective> missionList;
    private int _currentMissionIndex = 0;

    [Header("Configuración de Recursos")]
    [SerializeField] private InventoryItemData batteryItemData;

    // ✅ CAMBIO 1: Cambiamos la referencia del script antiguo al nuevo
    [SerializeField] private LevelItemSpawner levelSpawner;

    [Header("Conexiones")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private GamePacingManager pacingManager;

    private int _totalBatteriesInLevel = 1;
    private int _playerBatteryCount = 0;

    public event Action<string> OnObjectiveUpdated;
    public event Action OnLevelVictory;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // ✅ CAMBIO 2: Usamos el nuevo método para preguntar específicamente por baterías
        if (levelSpawner != null && batteryItemData != null)
        {
            _totalBatteriesInLevel = levelSpawner.GetTotalCountForItem(batteryItemData);

            // Seguridad por si el spawner dice 0 (evitar división por cero luego)
            if (_totalBatteriesInLevel == 0) _totalBatteriesInLevel = 1;

            Debug.Log($"[Director] Total baterías registradas para balanceo: {_totalBatteriesInLevel}");
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnItemAdded += HandleItemAdded;
        }

        UpdateMissionUI();
    }

    // ... El resto del script se queda EXACTAMENTE IGUAL ...
    // (OnDestroy, HandleItemAdded, RecalculateDifficulty, etc.)

    private void OnDestroy()
    {
        if (inventorySystem != null) inventorySystem.OnItemAdded -= HandleItemAdded;
    }

    private void HandleItemAdded(InventoryItemData item)
    {
        // --- LÓGICA DE BALANCEO ---
        if (item == batteryItemData)
        {
            _playerBatteryCount++;
            RecalculateDifficulty();
        }

        // --- LÓGICA DE MISIONES ---
        if (_currentMissionIndex < missionList.Count)
        {
            MissionObjective currentObj = missionList[_currentMissionIndex];
            if (currentObj.requiredItem != null && item == currentObj.requiredItem)
            {
                CompleteCurrentObjective();
            }
        }
    }

    private void CompleteCurrentObjective()
    {
        // ... (Tu código original)
        if (missionList[_currentMissionIndex].completesLevel)
        {
            OnLevelVictory?.Invoke();
        }
        else
        {
            _currentMissionIndex++;
            UpdateMissionUI();
            RecalculateDifficulty();
        }
    }

    private void UpdateMissionUI()
    {
        if (_currentMissionIndex < missionList.Count)
        {
            OnObjectiveUpdated?.Invoke(missionList[_currentMissionIndex].description);
        }
    }

    private void RecalculateDifficulty()
    {
        if (pacingManager == null) return;

        float objectiveProgress = (float)_currentMissionIndex / missionList.Count;
        float resourcePower = (float)_playerBatteryCount / _totalBatteriesInLevel;

        pacingManager.UpdateGameState(objectiveProgress, resourcePower);
    }
}
