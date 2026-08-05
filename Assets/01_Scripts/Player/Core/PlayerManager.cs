using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(JumpSystem))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerManager : MonoBehaviour, IPlayerPositionProvider
{
    public static PlayerManager Instance { get; private set; }
    private PlayerInput input;
    private PlayerMovement movement;
    private JumpSystem jump;
    private CameraController cameraController;
    [SerializeField] private Transform playerCameraTransform;

    public Vector3 PlayerBodyPosition => transform.position;
    public Vector3 PlayerForward => transform.forward;
    public Vector3 CameraPosition => cameraController != null ? cameraController.transform.position : Vector3.zero; 
    public Vector3 CameraForward => cameraController != null ? cameraController.transform.forward : Vector3.forward;



    private void Awake()
    {
        // Lógica Singleton (Asegura que solo haya una instancia)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Validación
        if (playerCameraTransform == null)
            Debug.LogError("¡Falta asignar la Player Camera en el PlayerManager!");

        input = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<JumpSystem>();
        cameraController = GetComponent<CameraController>();

        // Notificar que el player está listo
        if (PlayerEventSystem.Instance != null)
        {
            PlayerEventSystem.Instance.NotifyPlayerReady(this);
        }
    }


    private void OnDestroy()
    {
        // Notificar que el player se va a destruir
        if (PlayerEventSystem.Instance != null)
        {
            PlayerEventSystem.Instance.NotifyPlayerUnready();
        }
    }

    private void Update()
    {
        movement.SetMovementInput(new Vector3(input.MoveInput.x, 0, input.MoveInput.y));
        cameraController.RotateCamera(input.LookInput);

        if (input.JumpPressed) jump.Jump();
    }

    private void FixedUpdate() => jump.CheckGrounded();
}
