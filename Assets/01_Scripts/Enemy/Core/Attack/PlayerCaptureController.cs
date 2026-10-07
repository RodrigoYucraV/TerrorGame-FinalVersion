using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCaptureController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerManager playerManager;

    public bool IsCaptured { get; private set; }

    public event Action CaptureStarted;
    public event Action CaptureEnded;

    private void Awake()
    {
        if (playerManager == null)
            playerManager = GetComponent<PlayerManager>();

        if (playerManager == null)
        {
            Debug.LogError(
                "[PlayerCaptureController] No se encontró PlayerManager.",
                this
            );
        }
    }

    public bool StartCapture()
    {
        if (IsCaptured)
            return false;

        if (playerManager == null)
        {
            Debug.LogError(
                "[PlayerCaptureController] No se puede iniciar la captura.",
                this
            );
            return false;
        }

        IsCaptured = true;

        // Bloquea el movimiento normal y el salto,
        // pero conserva la lectura de inputs y la cámara.
        playerManager.SetMovementLocked(true);

        Debug.Log("[Player] Captura iniciada.");

        CaptureStarted?.Invoke();

        return true;
    }

    public void ReleaseCapture()
    {
        if (!IsCaptured)
            return;

        IsCaptured = false;

        if (playerManager != null)
            playerManager.SetMovementLocked(false);

        Debug.Log("[Player] Captura finalizada.");

        CaptureEnded?.Invoke();
    }

    private void OnDestroy()
    {
        // Evita dejar bloqueado al jugador si se destruye
        // este componente mientras la captura está activa.
        if (IsCaptured && playerManager != null)
            playerManager.SetMovementLocked(false);
    }
}