using System;
using UnityEngine;

public class PlayerVisibilityDetector : MonoBehaviour, IPlayerDetector, IPlayerDetectionEvents
{
    [Header("Configuración de Visibilidad")]
    public Transform playerCamera;
    public LayerMask obstacleMask;
    public float viewThresholdAngle = 45f;
    public float maxViewDistance = 15f;

    [Header("Linterna")]
    public Light flashlight;
    public float flashlightDetectionAngle = 15f;

    public bool IsPlayerLookingAtEnemy { get; private set; }
    public bool IsHitByFlashlight { get; private set; }
    
    private bool wasPlayerLookingLastFrame;
    private bool wasHitByFlashlightLastFrame;
    
    public event Action OnHideDarkness;
    public event Action OnEnemyAttack;
    public event Action OnPlayerDetected;
    public event Action OnPlayerLost;
    public event Action OnHitByFlashlight;

    private IPlayerPositionProvider playerPositionProvider;

    private void Start()
    {
        playerPositionProvider = PlayerManager.Instance;
        if (playerPositionProvider == null)
        {
            Debug.LogError("PlayerVisibilityDetector: PlayerManager.Instance no encontrado!");
        }
        if (playerCamera == null)
        {
            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        // 3. Si la linterna no está asignada, buscarla en el jugador
        if (flashlight == null && playerCamera != null)
        {
            // Asumimos que la linterna es hija de la cámara o del jugador
            flashlight = playerCamera.GetComponentInChildren<Light>();
        }
    }
    public void ManualDetect()
    {
        DetectPlayerView();
        DetectFlashlight();
        CheckDetectionChanges();
    }

    private void DetectPlayerView()
    {
        if (playerPositionProvider == null) return;
        Vector3 cameraPosition = playerPositionProvider.CameraPosition;
        Vector3 cameraForward = playerPositionProvider.CameraForward;

        Vector3 toEnemy = (transform.position - cameraPosition).normalized;
        float angle = Vector3.Angle(cameraForward, toEnemy);

        bool inViewAngle = angle < viewThresholdAngle;
        bool notBlocked = !Physics.Linecast(cameraPosition, transform.position, obstacleMask);
        float distance = Vector3.Distance(cameraPosition, transform.position);

        IsPlayerLookingAtEnemy = inViewAngle && notBlocked && distance <= maxViewDistance;
    }
    private void CheckDetectionChanges()
    {
        
        if (IsPlayerLookingAtEnemy && !wasPlayerLookingLastFrame)
        {
            OnPlayerDetected?.Invoke();
            Debug.Log("<color=green>Evento: Jugador comenzó a mirar al enemigo</color>");
        }
        else if (!IsPlayerLookingAtEnemy && wasPlayerLookingLastFrame)
        {
            OnPlayerLost?.Invoke();
            Debug.Log("<color=red>Evento: Jugador dejó de mirar al enemigo</color>");
        }

        // Detección de cambios en la linterna (opcional)
        if (IsHitByFlashlight && !wasHitByFlashlightLastFrame)
        {
            OnHitByFlashlight?.Invoke(); 
            Debug.Log("<color=#FFA500>🔦 EVENTO: ¡El enemigo FUE GOLPEADO por la linterna! 🔦</color>");
        }


        wasPlayerLookingLastFrame = IsPlayerLookingAtEnemy;
        wasHitByFlashlightLastFrame = IsHitByFlashlight;
    }

    private void DetectFlashlight()
        //APLICAR LOGICA DEL ACTION OnHidaDarkness
    {
        if (flashlight == null ||
        !flashlight.gameObject.activeInHierarchy ||
        !flashlight.enabled ||
        flashlight.intensity <= 0.1f)
        {
            IsHitByFlashlight = false;
            return;
        }

        Vector3 toEnemy = (transform.position - flashlight.transform.position).normalized;
        float angle = Vector3.Angle(flashlight.transform.forward, toEnemy);
        bool inFlashlightCone = angle < flashlightDetectionAngle;

        float distance = Vector3.Distance(flashlight.transform.position, transform.position);
        bool notBlocked = !Physics.Linecast(flashlight.transform.position, transform.position, obstacleMask);

        IsHitByFlashlight = inFlashlightCone && notBlocked && distance <= flashlight.range;
    }
}
