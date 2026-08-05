using UnityEngine;
using System.Collections.Generic;

public class GamePacingManager : MonoBehaviour, IEnemyStatsProvider
{
    public static GamePacingManager Instance { get; private set; }

    [Header("1. Curvas de Tensión")]
    [Tooltip("Eje X: Minutos jugados, Eje Y: Multiplicador de estrés (0 a 1)")]
    [SerializeField] private AnimationCurve timeDifficultyCurve;
    [Tooltip("Eje X: % Objetivos, Eje Y: Multiplicador de estrés (0 a 1)")]
    [SerializeField] private AnimationCurve progressDifficultyCurve;

    [Header("2. Stats Dinámicos (Rango)")]
    [Tooltip("Velocidad cuando el estrés es 0")]
    [SerializeField] private float minChaseSpeed = 3.5f;
    [Tooltip("Velocidad cuando el estrés es 1 (Máxima tensión)")]
    [SerializeField] private float maxChaseSpeed = 7.5f;

    [Space(10)]
    [SerializeField] private float minLightDamage = 10f;
    [SerializeField] private float maxLightDamage = 50f;

    [Header("3. Configuración de Comportamiento")]
    [SerializeField] private float followDistance = 10f;
    [SerializeField] private float approachDistance = 5f;
    [SerializeField] private float lateralAngle = 45f;
    [SerializeField] private float chaseThreshold = 15f;

    [Header("4. Configuración de Huida (Retreat)")]
    [Tooltip("Tiempo de huida cuando el estrés es bajo (fácil)")]
    [SerializeField] private float maxRetreatDuration = 5.0f;
    [Tooltip("Tiempo de huida cuando el estrés es alto (difícil)")]
    [SerializeField] private float minRetreatDuration = 2.0f;


    [Header("Curva de Recursos")]
    [Tooltip("Si el jugador tiene muchas pilas, ¿cuánto sube la dificultad?")]
    [SerializeField] private AnimationCurve resourceTensionCurve;

    private float _objectiveProgress;
    private float _playerResourcePower;

    // Estado interno
    private float _currentMissionProgress = 0f;
    private float _levelStartTime;
    private DynamicEnemyConfig _dynamicConfig;

    // --- INTERFAZ IEnemyStatsProvider ---
    public IEnemyMovementConfig CurrentMovementConfig => _dynamicConfig;
    public IFollowSettings CurrentFollowSettings => _dynamicConfig;
    public float CurrentLightDamage => Mathf.Lerp(minLightDamage, maxLightDamage, GetCurrentStress());

    // --- NUEVA PROPIEDAD PARA ARREGLAR TU ERROR ---
    // Cuanto más estrés, MENOS tiempo huye (vuelve más rápido a por ti)
    public float CurrentRetreatDuration => Mathf.Lerp(maxRetreatDuration, minRetreatDuration, GetCurrentStress());

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _dynamicConfig = new DynamicEnemyConfig(this);
        _levelStartTime = Time.time;
    }

    private void Update()
    {
        float stress = GetCurrentStress();
        float currentSpeed = Mathf.Lerp(minChaseSpeed, maxChaseSpeed, stress);
        _dynamicConfig.UpdateDynamicValues(currentSpeed);
    }

    public void UpdateMissionProgress(float progress01)
    {
        _currentMissionProgress = Mathf.Clamp01(progress01);
    }

    public float GetCurrentStress()
    {
        // 1. Tiempo
        float timeMin = (Time.time - _levelStartTime) / 60f;
        float stressTime = timeDifficultyCurve.Evaluate(timeMin);

        // 2. Objetivos (Lineal)
        float stressObj = progressDifficultyCurve.Evaluate(_objectiveProgress);

        // 3. Recursos (Nuevo)
        // Si resourcePower es alto (tienes muchas pilas), la curva debería devolver un valor alto
        // para que el enemigo sea más rápido y te obligue a gastarlas.
        float stressResources = resourceTensionCurve.Evaluate(_playerResourcePower);

        // FÓRMULA MAESTRA:
        // El estrés es el máximo entre el Tiempo y (Progreso + Penalización por tener muchas pilas)
        float totalStress = Mathf.Max(stressTime, (stressObj + stressResources * 0.5f));

        return Mathf.Clamp01(totalStress);
    }

    private class DynamicEnemyConfig : IEnemyMovementConfig, IFollowSettings
    {
        private GamePacingManager _manager;
        private float _currentChaseSpeed;

        public DynamicEnemyConfig(GamePacingManager manager)
        {
            _manager = manager;
        }

        public void UpdateDynamicValues(float speed)
        {
            _currentChaseSpeed = speed;
        }

        public float ChaseSpeed => _currentChaseSpeed;
        public float FollowSpeed => _currentChaseSpeed * 0.6f;
        public float FollowDistance => _manager.followDistance;
        public float LateralAngle => _manager.lateralAngle;
        public float ApproachDistance => _manager.approachDistance;
        public float ChaseThreshold => _manager.chaseThreshold;
        public Vector2 OffsetAngleRange => new Vector2(30, 90);
        public float DesiredFollowDistance => 2.0f;
    }
    public void UpdateGameState(float objectiveProg, float resourcePow)
    {
        _objectiveProgress = objectiveProg;
        _playerResourcePower = resourcePow;
    }
}