using UnityEngine;

public class PlayerBodyPositionProvider : MonoBehaviour, IPlayerBodyPositionProvider
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] private Transform playerBody; // Asignar al GameObject del cuerpo del jugador
    //[SerializeField] private Transform playerCamera; // Asignar a la cámara del jugador

    public Vector3 PlayerBodyPosition => playerBody.position;
    public Vector3 PlayerForward => playerBody.forward;

    private void OnDrawGizmos()
    {
        // Debug para el cuerpo
        Gizmos.color = Color.blue;
      //  Gizmos.DrawSphere(playerBody.position, 0.3f);
        Gizmos.DrawRay(playerBody.position, playerBody.forward * 2f);

    }
}