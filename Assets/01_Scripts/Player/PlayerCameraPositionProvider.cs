using UnityEngine;

public class PlayerCameraPositionProvider : MonoBehaviour, IPlayerCameraPositionProvider
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí la Main Camera de tu jugador")]
    [SerializeField] private Transform playerCameraTransform;

    public Vector3 CameraPosition => playerCameraTransform.position;
    public Vector3 CameraForward => playerCameraTransform.forward;

    // Para debug visual
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(playerCameraTransform.position, 0.2f);
        Gizmos.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * 1.5f);
    }
}