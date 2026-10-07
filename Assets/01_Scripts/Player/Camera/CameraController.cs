
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Configuración normal")]
    [SerializeField] private float verticalLookLimit = 85f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool hideCursor = true;

    [Header("Captura cinematográfica")]
    [SerializeField] private float captureRotationSpeed = 3f;
    [SerializeField] private float captureLookHeight = 1.6f;

    private float verticalRotation;

    private bool captureSoftLockActive;
    private Transform captureTarget;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError(
                "CameraController: falta asignar cameraTransform y no existe Camera.main.",
                this
            );
        }

        InitializeCursorState();
    }

    private void LateUpdate()
    {
        if (captureSoftLockActive)
            UpdateCaptureSoftLock();
    }

    private void OnDestroy()
    {
        RestoreCursorState();
    }

    public void RotateCamera(Vector2 lookInput)
    {
        if (cameraTransform == null || captureSoftLockActive)
            return;

        // Rotación horizontal del jugador.
        transform.Rotate(Vector3.up * lookInput.x);

        // Rotación vertical de la cámara.
        verticalRotation -= lookInput.y;
        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -verticalLookLimit,
            verticalLookLimit
        );

        cameraTransform.localEulerAngles =
            new Vector3(verticalRotation, 0f, 0f);
    }

    public void StartCaptureSoftLock(Transform target)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning(
                "CameraController: no se puede activar el soft-lock sin cámara.",
                this
            );
            return;
        }

        if (target == null)
        {
            Debug.LogWarning(
                "CameraController: no se puede activar el soft-lock sin objetivo.",
                this
            );
            return;
        }

        captureTarget = target;
        captureSoftLockActive = true;
    }

    public void StopCaptureSoftLock()
    {
        captureSoftLockActive = false;
        captureTarget = null;

        // Sincroniza el ángulo vertical para evitar saltos
        // cuando el jugador retome el control.
        verticalRotation = NormalizeAngle(cameraTransform.localEulerAngles.x);
    }

    private void UpdateCaptureSoftLock()
    {
        if (cameraTransform == null || captureTarget == null)
        {
            StopCaptureSoftLock();
            return;
        }

        Vector3 lookPosition =
            captureTarget.position + Vector3.up * captureLookHeight;

        Vector3 direction = lookPosition - cameraTransform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Gira suavemente la cámara hacia el enemigo.
        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation,
            targetRotation,
            captureRotationSpeed * Time.deltaTime
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void InitializeCursorState()
    {
        if (!hideCursor)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestoreCursorState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}