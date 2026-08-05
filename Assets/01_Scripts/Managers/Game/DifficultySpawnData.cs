using UnityEngine;
public class DifficultySpawnData:MonoBehaviour
{
    public string difficultyName;
    public int batteryCount = 10;
    [Range(0f, 1f)] public float batteryCharge; // Con cuánta carga vienen
}