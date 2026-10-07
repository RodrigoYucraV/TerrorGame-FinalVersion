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

    [Header("Refugios / Sombras")]
    [Tooltip("Objetos que definen zonas de refugio para el enemigo (paredes, patios, techos, etc.).")]
    [SerializeField] private List<SpawnVolume> spawnVolumes;

    [Tooltip("Capas que bloquean la visión entre el jugador y el enemigo.")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("Distancia máxima a la que consideramos que el jugador puede ver directamente una posición.")]
    [SerializeField] private float playerViewRadius = 20f;

    [Tooltip("Ángulo máximo de visión usado para saber si el jugador mira una posición.")]
    [SerializeField] private float viewAngleThreshold = 70f;

    [Header("Selección de Refugio al Huir")]
    [Tooltip("Distancia mínima que debe mantener el punto de refugio respecto al jugador.")]
    [SerializeField] private float retreatMinPlayerDistance = 8f;

    [Tooltip("Cantidad de puntos aleatorios que se prueban por SpawnVolume al buscar refugio.")]
    [SerializeField] private int retreatSamplesPerVolume = 8;

    [Header("Respawn Frente al Jugador")]
    [Tooltip("Distancia mínima del punto de respawn respecto a la cámara del jugador.")]
    [SerializeField] private float frontRespawnMinDistance = 4f;

    [Tooltip("Distancia máxima del punto de respawn respecto a la cámara del jugador.")]
    [SerializeField] private float frontRespawnMaxDistance = 20f;

    [Tooltip("Ángulo máximo respecto al frente de la cámara para considerar un punto 'en frente del jugador'.")]
    [SerializeField] private float frontRespawnAngle = 75f;

    [Tooltip("Cantidad de puntos aleatorios que se prueban por SpawnVolume al buscar el respawn.")]
    [SerializeField] private int respawnSamplesPerVolume = 10;

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

        // Buscamos un punto concreto dentro de los refugios, no solo el centro del volumen.
        // El candidato ideal es:
        // 1) oculto para el jugador,
        // 2) a una distancia mínima del jugador,
        // 3) lo más cercano posible al enemigo para que la huida sea corta.
        bool found = false;
        Vector3 bestPosition = enemyPosition;
        float bestDistanceToEnemy = float.MaxValue;
        int samples = Mathf.Max(1, retreatSamplesPerVolume);

        foreach (SpawnVolume volume in spawnVolumes)
        {
            if (volume == null) continue;

            for (int i = 0; i < samples; i++)
            {
                Vector3 candidatePos = volume.GetRandomPointInVolume();
                float distanceToPlayer = Vector3.Distance(candidatePos, playerPosition);

                if (distanceToPlayer < retreatMinPlayerDistance)
                    continue;

                if (IsPlayerLookingAtPosition(candidatePos))
                    continue;

                float distanceToEnemy = Vector3.Distance(candidatePos, enemyPosition);
                if (distanceToEnemy >= bestDistanceToEnemy)
                    continue;

                bestDistanceToEnemy = distanceToEnemy;
                bestPosition = candidatePos;
                found = true;
            }
        }

        if (found)
            return bestPosition;

        // Fallback 1: buscamos al menos un punto oculto, aunque no cumpla la distancia ideal.
        Vector3 hiddenFallback = GetNearestHiddenPosition(enemyPosition);
        if (hiddenFallback != enemyPosition)
            return hiddenFallback;

        // Fallback 2: conservar la lógica anterior de alejarnos usando el volumen más lejano.
        SpawnVolume fallbackVolume = spawnVolumes
            .Where(v => v != null)
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

        // IMPORTANTE:
        // No se destruye ni se instancia otro enemigo.
        // Movemos el mismo Transform a la posición oculta más cercana posible
        // que quede frente al jugador.
        Vector3 safePosition = GetHiddenPosition();
        enemyTransform.position = safePosition;

        Debug.Log($"[Respawn] Mismo enemigo movido a sombra frente al jugador: {safePosition}");

        OnRespawnComplete?.Invoke();
    }

    public Vector3 GetRespawnPosition()
    {
        return GetHiddenPosition();
    }

    private Vector3 GetHiddenPosition()
    {
        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            Debug.LogWarning("No hay SpawnVolumes asignados en EnemyManager. Usando posición por defecto.");
            return transform.position;
        }

        // Primera opción: punto oculto y lo más cercano posible al jugador,
        // pero situado frente a su cámara.
        Vector3 frontPosition;
        if (TryGetNearestHiddenPointInFrontOfPlayer(out frontPosition, true))
            return frontPosition;

        // Segunda opción: seguimos priorizando que esté delante y oculto,
        // pero relajamos la ventana de distancia.
        if (TryGetNearestHiddenPointInFrontOfPlayer(out frontPosition, false))
            return frontPosition;

        // Último recurso: cualquier punto oculto.
        Vector3 hiddenFallback = GetNearestHiddenPosition(cameraProvider != null
            ? cameraProvider.CameraPosition
            : transform.position);

        if (hiddenFallback != (cameraProvider != null ? cameraProvider.CameraPosition : transform.position))
            return hiddenFallback;

        return spawnVolumes[0].GetRandomPointInVolume();
    }

    private bool TryGetNearestHiddenPointInFrontOfPlayer(out Vector3 bestPosition, bool respectDistanceWindow)
    {
        bestPosition = transform.position;

        if (cameraProvider == null)
            return false;

        Vector3 cameraPosition = cameraProvider.CameraPosition;
        Vector3 cameraForward = cameraProvider.CameraForward;
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude <= 0.001f)
            return false;

        cameraForward.Normalize();

        float bestDistance = float.MaxValue;
        float frontCos = Mathf.Cos(frontRespawnAngle * Mathf.Deg2Rad);
        int samples = Mathf.Max(1, respawnSamplesPerVolume);
        bool found = false;

        foreach (SpawnVolume volume in spawnVolumes)
        {
            if (volume == null) continue;

            for (int i = 0; i < samples; i++)
            {
                Vector3 candidate = volume.GetRandomPointInVolume();
                Vector3 toCandidate = candidate - cameraPosition;
                float distance = toCandidate.magnitude;

                if (respectDistanceWindow)
                {
                    if (distance < frontRespawnMinDistance || distance > frontRespawnMaxDistance)
                        continue;
                }

                Vector3 flatDirection = toCandidate;
                flatDirection.y = 0f;

                if (flatDirection.sqrMagnitude <= 0.001f)
                    continue;

                flatDirection.Normalize();

                // El punto debe estar dentro del cono frontal del jugador.
                if (Vector3.Dot(cameraForward, flatDirection) < frontCos)
                    continue;

                // Y, sobre todo, debe estar en sombra/oculto para el jugador.
                if (IsPlayerLookingAtPosition(candidate))
                    continue;

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestPosition = candidate;
                found = true;
            }
        }

        return found;
    }

    private Vector3 GetNearestHiddenPosition(Vector3 referencePosition)
    {
        bool found = false;
        Vector3 bestPosition = referencePosition;
        float bestDistance = float.MaxValue;
        int samples = Mathf.Max(1, respawnSamplesPerVolume);

        foreach (SpawnVolume volume in spawnVolumes)
        {
            if (volume == null) continue;

            for (int i = 0; i < samples; i++)
            {
                Vector3 candidate = volume.GetRandomPointInVolume();

                if (IsPlayerLookingAtPosition(candidate))
                    continue;

                float distance = Vector3.Distance(candidate, referencePosition);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestPosition = candidate;
                found = true;
            }
        }

        return found ? bestPosition : referencePosition;
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

        // 1. Chequeo de Distancia: Si está muy lejos, se considera oculto/seguro
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
