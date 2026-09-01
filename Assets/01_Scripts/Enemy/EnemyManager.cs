using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random; // Evitamos confusión con System.Random

public class EnemyManager : MonoBehaviour, IRespawnHandler, ISafeZoneProvider
{
 
    [Header("Spawn Inicial")]
    [Tooltip("El prefab del enemigo. Debe tener EnemyController y EnemyDeathNotifier.")]
    [SerializeField] private GameObject prefabEnemy;
    [SerializeField] private int initialEnemies = 5;
    [SerializeField] private float spawnRadius = 10f;

    [Header("Reaparición (Slenderman Logic)")]
    [Tooltip("Arrastra aquí los objetos que definen zonas seguras (Techos, patios, etc.)")]
    [SerializeField] private List<SpawnVolume> spawnVolumes;

    [Tooltip("Capas que bloquean la visión (Paredes, Suelo)")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("Distancia máxima a la que el jugador puede ver")]
    [SerializeField] private float playerViewRadius = 20f;

    [Tooltip("Ángulo de visión de la cámara (FOV aproximado)")]
    [SerializeField] private float viewAngleThreshold = 70f;

    [Header("Spawn Oculto (Inicial)")]
    [Tooltip("Distancia mínima al jugador para el spawn inicial.")]
    [SerializeField] private float minSpawnDistance = 15f;
    [Tooltip("Número máximo de intentos para encontrar una posición segura.")]
    [SerializeField] private int maxSpawnTries = 10;

    private IPlayerCameraPositionProvider cameraProvider;
    public event Action OnRespawnComplete; // Evento de la interfaz

    public EnemyPhase CurrentGlobalPhase { get; private set; } = EnemyPhase.Phase1_Stealth;
    public event Action<EnemyPhase> OnGlobalPhaseChanged;
    private void Start()
    {
        cameraProvider = PlayerManager.Instance;

        if (cameraProvider == null)
        {
            Debug.LogError("CRITICO: PlayerManager no encontrado. Asegúrate de que PlayerManager existe en la escena.");
        }
        EnemyDeathNotifier.OnEnemyDied += HandleEnemyDeath;
        SpawnInitialEnemies();
    }


    private Vector3 GetHiddenInitialSpawnPoint()
    {
        // Si no hay volúmenes, devolvemos la posición del Manager (o un error)
        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            Debug.LogWarning("No hay SpawnVolumes. Usando posición por defecto.");
            return transform.position;
        }

        // 1. Barajamos la lista para distribución uniforme de los intentos
        ShuffleList(spawnVolumes);

        // 2. Buscamos un punto oculto que cumpla ambas condiciones: Invisible y Lejos
        for (int i = 0; i < maxSpawnTries; i++)
        {
            // Elegimos un volumen al azar (usando el índice del bucle de Shuffle)
            SpawnVolume volume = spawnVolumes[i % spawnVolumes.Count];

            Vector3 candidatePos = volume.GetRandomPointInVolume();

            float distance = cameraProvider != null
                ? Vector3.Distance(candidatePos, cameraProvider.CameraPosition)
                : minSpawnDistance + 1f;

            // Condición 1: Debe estar lejos (Mínima distancia)
            bool isFarEnough = distance > minSpawnDistance;

            // Condición 2: Debe ser invisible para el jugador (Usamos la función ya existente)
            bool isHidden = !IsPlayerLookingAtPosition(candidatePos);

            if (isFarEnough && isHidden)
            {
                return candidatePos; // ¡Éxito!
            }
        }

        // 3. Fallback: Si no se encuentra un punto después de todos los intentos,
        // usamos la posición de spawn del primer volumen, confiando en que al menos está fuera del campo de juego inicial.
        Debug.LogWarning("No se pudo encontrar una posición de spawn inicial invisible. Usando Fallback.");
        return spawnVolumes[0].GetRandomPointInVolume();
    }
    public Vector3 GetRetreatPosition(Vector3 enemyPosition, Vector3 playerPosition)
    {

        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            Debug.LogWarning("No hay SpawnVolumes para huir. Regresando posición actual.");
            return enemyPosition;
        }
        var bestVolume = spawnVolumes
            .Where(v => Vector3.Distance(v.transform.position, playerPosition) > 10f)
            .OrderBy(v => Vector3.Distance(v.transform.position, enemyPosition))
            .FirstOrDefault();

        if (bestVolume != null)
        {
            return bestVolume.GetRandomPointInVolume();
        }
        var fallbackVolume = spawnVolumes
            .OrderByDescending(v => Vector3.Distance(v.transform.position, playerPosition))
            .FirstOrDefault();

        return fallbackVolume != null ? fallbackVolume.GetRandomPointInVolume() : enemyPosition;
    }
    public void ChangePhaseGlobal(EnemyPhase newPhase)
    {
        CurrentGlobalPhase = newPhase;
        Debug.Log($"[EnemyManager] CAMBIO DE FASE GLOBAL: {newPhase}");

        // Notificar a todos los enemigos vivos
        OnGlobalPhaseChanged?.Invoke(newPhase);
    }
    private Vector3 GetRandomVolumeSpawnPoint()
    {
        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            // Fallback: usar la posición del Manager si no hay volúmenes.
            return transform.position;
        }

        // Elegir un volumen aleatorio
        int randomIndex = Random.Range(0, spawnVolumes.Count);

        // Devolver un punto aleatorio dentro de ese volumen
        return spawnVolumes[randomIndex].GetRandomPointInVolume();
    }

    private void SpawnInitialEnemies()
    {
        if (prefabEnemy == null) return;

        for (int i = 0; i < initialEnemies; i++)
        {
            // ✅ CAMBIO CLAVE: Usamos la función que garantiza que el punto esté oculto
            Vector3 pos = GetHiddenInitialSpawnPoint();

            // La inyección de dependencia es correcta
            GameObject newEnemyObj = Instantiate(prefabEnemy, pos, Quaternion.identity);
            EnemyController controller = newEnemyObj.GetComponent<EnemyController>();

            if (controller != null)
            {
                controller.Initialize(this, this);
            }
        }
    }

    private void HandleEnemyDeath(GameObject enemy)
    {
        // Aquí puedes poner lógica: si quedan 0 enemigos, iniciar siguiente nivel
        Debug.Log("Un enemigo ha muerto. Manager notificado.");
    }

    public void Respawn(Transform enemyTransform)
    {
        if (enemyTransform == null) return;

        Vector3 safePosition = GetHiddenPosition();
        enemyTransform.position = safePosition;

        Debug.Log($"[Respawn] Enemigo movido a zona segura: {safePosition}");

        OnRespawnComplete?.Invoke();
    }

    public Vector3 GetRespawnPosition()
    {
  
        return GetHiddenPosition();
    }

    private Vector3 GetHiddenPosition()
    {
        // Si no hay volúmenes configurados, devolver la posición del manager como fallback
        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            Debug.LogWarning("No hay SpawnVolumes asignados en EnemyManager. Usando posición por defecto.");
            return transform.position;
        }

        // 1. Barajar la lista para que no siempre pruebe el mismo volumen primero
        ShuffleList(spawnVolumes);

        // 2. Buscar un punto oculto
        foreach (var volume in spawnVolumes)
        {
            // Intentamos 5 veces por volumen para encontrar un punto ciego
            for (int i = 0; i < 5; i++)
            {
                Vector3 candidatePos = volume.GetRandomPointInVolume();

                if (!IsPlayerLookingAtPosition(candidatePos))
                {
                    return candidatePos; // ¡Éxito! Nadie lo ve aquí.
                }
            }
        }
        return spawnVolumes[0].GetRandomPointInVolume();
    }

    // ---------------------------------------------------------
    // CÁLCULOS MATEMÁTICOS (Visibilidad)
    // ---------------------------------------------------------
    private bool IsPlayerLookingAtPosition(Vector3 targetPosition)
    {
        if (cameraProvider == null) return false; // Si no hay player, asumimos que no lo ve

        Vector3 camPos = cameraProvider.CameraPosition;
        Vector3 camFwd = cameraProvider.CameraForward;

        Vector3 toTarget = targetPosition - camPos;
        float distance = toTarget.magnitude;

        // 1. Chequeo de Distancia: Si está muy lejos, se considera "oculto/seguro"
        if (distance > playerViewRadius) return false;

        // 2. Chequeo de Ángulo: ¿Está frente a la cámara?
        float angle = Vector3.Angle(camFwd, toTarget);
        if (angle < viewAngleThreshold)
        {
            if (Physics.Linecast(camPos, targetPosition, obstacleMask))
            {
                return false; // Hay pared, por lo tanto NO lo ve.
            }
            return true; // Está cerca, en ángulo y sin paredes: LO VE.
        }
        return false;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    private void OnDestroy()
    {
        EnemyDeathNotifier.OnEnemyDied -= HandleEnemyDeath;
    }

}
