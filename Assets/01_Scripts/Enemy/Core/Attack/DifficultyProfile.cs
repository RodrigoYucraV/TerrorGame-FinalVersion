using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Scriptable Objects/Difficulty Profile")]
public class DifficultyProfile : ScriptableObject
{
    [Header("--- ENEMIGOS ---")]
    public EnemyProperties enemySettings;
    public float lightDamagePerSecond = 20f;
    public float retreatDuration = 2f;

    [Header("--- ITEMS & RECURSOS ---")]
    [Tooltip("Configura aquí cuántas baterías y llaves salen en esta dificultad")]
    // ✅ AQUI AGREGAMOS LA LISTA DE ITEMS
    public List<ItemSpawnConfig> waveConfig;
}