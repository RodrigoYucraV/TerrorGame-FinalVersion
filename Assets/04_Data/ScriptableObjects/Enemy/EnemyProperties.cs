using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Scriptable Objects/EnemyProperiti")]
public class EnemyProperties : ScriptableObject, IEnemyMovementConfig, IFollowSettings
{
    [Header("Movimiento Base")]
    [SerializeField] private float followSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 6.0f;
    [SerializeField] private float desiredFollowDistance = 5.0f;

    [Header("Configuración de Persecución (Stealth)")]
    [SerializeField] private float followDistance = 8.0f;
    [SerializeField] private float lateralAngle = 45.0f;
    [SerializeField] private float approachDistance = 2.0f;
    [SerializeField] private float chaseThreshold = 10.0f;
    [SerializeField] private Vector2 offsetAngleRange = new Vector2(-30, 30);

    // --- Implementación de IEnemyMovementConfig ---
    public float FollowSpeed => followSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float DesiredFollowDistance => desiredFollowDistance;

    // --- Implementación de IFollowSettings ---
    public float FollowDistance => followDistance;
    public float LateralAngle => lateralAngle;
    public float ApproachDistance => approachDistance;
    public float ChaseThreshold => chaseThreshold;
    public Vector2 OffsetAngleRange => offsetAngleRange;
}
