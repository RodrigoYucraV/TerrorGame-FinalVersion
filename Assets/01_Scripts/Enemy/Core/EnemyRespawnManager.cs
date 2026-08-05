using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawnManager : MonoBehaviour, IRespawnHandler
{
    [Header("Configuración de Áreas")]
    [Tooltip("Arrastra aquí tus objetos con el script SpawnVolume")]
    [SerializeField] private List<SpawnVolume> spawnVolumes;

    [Header("Configuración de Visibilidad")]
    [SerializeField] private LayerMask obstacleMask; // Paredes/Obstáculos
    [SerializeField] private float playerViewRadius = 20f;
    [SerializeField] private float viewAngleThreshold = 70f;

    private IPlayerCameraPositionProvider cameraProvider;

    public event Action OnRespawnComplete;

    private void Start()
    {
        cameraProvider = PlayerManager.Instance; 

        if (cameraProvider == null)
        {
            // Error si el Singleton no está inicializado
            Debug.LogError("EnemyRespawnManager: IPlayerCameraPositionProvider no encontrado o no implementado correctamente. VERIFICAR: 1) Que PlayerManager tenga el Singleton. 2) Que el objeto PlayerManager esté activo en la escena.");
        }
    }

    public Vector3 GetRespawnPosition()
    {
        if (spawnVolumes == null || spawnVolumes.Count == 0)
        {
            Debug.LogError("No hay SpawnVolumes asignados.");
            return transform.position;
        }


        ShuffleList(spawnVolumes);

        // Intentamos encontrar un punto válido
        foreach (var volume in spawnVolumes)
        {
            // Intentamos 5 veces dentro de ESTE volumen específico
            for (int i = 0; i < 5; i++)
            {
                Vector3 candidatePos = volume.GetRandomPointInVolume();

                if (!IsPlayerLookingAtPosition(candidatePos))
                {
                    return candidatePos; // ¡Encontrado!
                }
            }
        }

        // (Mejor aparecer visible que romper el juego)
        Debug.LogWarning("No se encontró punto oculto. Usando fallback.");
        return spawnVolumes[0].GetRandomPointInVolume();
    }

    private bool IsPlayerLookingAtPosition(Vector3 targetPosition)
    {
        // NO usamos Transform, solo los Vector3 de la interfaz
        Vector3 camPos = cameraProvider.CameraPosition;
        Vector3 camFwd = cameraProvider.CameraForward;

        // El resto de la lógica matemática utiliza camPos y camFwd:
        Vector3 toSpawn = (targetPosition - camPos).normalized;
        float angle = Vector3.Angle(camFwd, toSpawn);
        float distance = Vector3.Distance(targetPosition, camPos);

        if (angle < 70f && distance < 20f)
        {
            // El Raycast usa camPos como punto de inicio:
            if (!Physics.Linecast(camPos, targetPosition, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    public void Respawn(Transform enemyTransform)
    {

        Vector3 newPos = GetRespawnPosition();
        enemyTransform.position = newPos;
        OnRespawnComplete?.Invoke();
    }

    // Utilidad simple para desordenar listas (Fisher-Yates shuffle)
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}