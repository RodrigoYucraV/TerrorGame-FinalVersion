using UnityEngine;
public class CameraController : MonoBehaviour
{
    [SerializeField] private float verticalLookLimit = 85f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool hideCursor = true;
    private float verticalRotation;
    private void Start()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (cameraTransform == null) Debug.LogError("CameraController: falta asignar cameraTransform y no existe Camera.main.");
        InitializeCursorState();
    }

    private void OnDestroy()
    {
        RestoreCursorState();
    }
    public void RotateCamera(Vector2 lookInput)
    {
        if (cameraTransform == null) return;

        // Rotaci�n horizontal (jugador)
        transform.Rotate(Vector3.up * lookInput.x);

        // Rotaci�n vertical (c�mara)
        verticalRotation -= lookInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
        cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0, 0);
    }
    private void InitializeCursorState()
    {
        if (!hideCursor) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestoreCursorState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
