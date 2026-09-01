using UnityEngine;
using System.Collections.Generic;

public class GamePacingManager : MonoBehaviour, IEnemyStatsProvider
{
    public static GamePacingManager Instance { get; private set; }

    [Header("1. Curvas de Tensi�n")]
    [Tooltip("Eje X: Minutos jugados, Eje Y: Multiplicador de estr�s (0 a 1)")]
    [SerializeField] private AnimationCurve timeDifficultyCurve;
    [Tooltip("Eje X: % Objetivos, Eje Y: Multiplicador de estr�s (0 a 1)")]
    [SerializeField] private AnimationCurve progressDifficultyCurve;

    [Header("2. Stats Din�micos (Rango)")]
    [Tooltip("Velocidad cuando el estr�s es 0")]
    [SerializeField] private float minChaseSpeed = 3.5f;
    [Tooltip("Velocidad cuando el estr�s es 1 (M�xima tensi�n)")]
    [SerializeField] private float maxChaseSpeed = 7.5f;

    [Space(10)]
    [SerializeField] private float minLightDamage = 10f;
    [SerializeField] private float maxLightDamage = 50f;

    [Header("3. Configuraci�n de Comportamiento")]
    [SerializeField] private float followDistance = 10f;
    [SerializeField] private float approachDistance = 5f;
    [SerializeField] private float lateralAngle = 45f;
    [SerializeField] private float chaseThreshold = 15f;

    [Header("4. Configuraci�n de Huida (Retreat)")]
    [Tooltip("Tiempo de huida cuando el estr�s es bajo (f�cil)")]
    [SerializeField] private float maxRetreatDuration = 5.0f;
    [Tooltip("Tiempo de huida cuando el estr�s es alto (dif�cil)")]
    [SerializeField] private float minRetreatDuration = 2.0f;


    [Header("Curva de Recursos")]
    [Tooltip("Si el jugador tiene muchas pilas, �cu�nto sube la dificultad?")]
    [SerializeField] private AnimationCurve resourceTensionCurve;

    private float _missionProgress;
    private float _playerResourcePower;

    // Estado interno
    private float _levelStartTime;
    private DynamicEnemyConfig _dynamicConfig;

    // --- INTERFAZ IEnemyStatsProvider ---
    public IEnemyMovementConfig CurrentMovementConfig => _dynamicConfig;
    public IFollowSettings CurrentFollowSettings => _dynamicConfig;
    public float CurrentLightDamage => Mathf.Lerp(minLightDamage, maxLightDamage, GetCurrentStress());

    // --- NUEVA PROPIEDAD PARA ARREGLAR TU ERROR ---
    // Cuanto m�s estr�s, MENOS tiempo huye (vuelve m�s r�pido a por ti)
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
        _missionProgress = Mathf.Clamp01(progress01);
    }

    public float GetCurrentStress()
    {
        // 1. Tiempo
        float timeMin = (Time.time - _levelStartTime) / 60f;
        float stressTime = timeDifficultyCurve.Evaluate(timeMin);

        // 2. Objetivos (Lineal)
        float stressObj = progressDifficultyCurve.Evaluate(_missionProgress);

        // 3. Recursos (Nuevo)
        // Si resourcePower es alto (tienes muchas pilas), la curva deber�a devolver un valor alto
        // para que el enemigo sea m�s r�pido y te obligue a gastarlas.
        float stressResources = resourceTensionCurve.Evaluate(_playerResourcePower);

        // F�RMULA MAESTRA:
        // El estr�s es el m�ximo entre el Tiempo y (Progreso + Penalizaci�n por tener muchas pilas)
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
        _missionProgress = Mathf.Clamp01(objectiveProg);
        _playerResourcePower = resourcePow;
    }
}
