using System;
using UnityEngine;

public interface IInteractable
{
    string GetInteractionPrompt();
    void OnInteract();
    void OnGrabbed(Transform holder);
    void OnReleased();
}

public class ObjectGrabbler : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private float grabRadius = 0.2f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject interactionPromptUI;

    private IInteractable currentInteractable;
    private RaycastHit currentHitInfo;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) cameraTransform = mainCamera.transform;
            else Debug.LogError("ObjectGrabbler: falta cameraTransform y no existe Camera.main.");
        }
    }

    private void Update()
    {
        CheckInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnInteract();
            }
        }
    }

    private bool DetectInteractable(out RaycastHit hitInfo, out IInteractable interactable)
    {
        hitInfo = new RaycastHit();
        interactable = null;
        if (cameraTransform == null) return false;

        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        bool hitSomething = Physics.SphereCast(
            origin,
            grabRadius,
            direction,
            out hitInfo,
            grabDistance,
            interactableLayer
        );

        if (hitSomething)
        {
            interactable = hitInfo.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hitInfo.collider.GetComponentInParent<IInteractable>();

            return interactable != null;
        }

        return false;
    }

    private void CheckInteractable()
    {
        bool found = DetectInteractable(out var hit, out var interactable);

        if (found)
        {
            currentInteractable = interactable;
            ShowInteractionPrompt(interactable.GetInteractionPrompt());
        }
        else
        {
            currentInteractable = null;
            HideInteractionPrompt();
        }

        // Debug Visual
        Color debugColor = found ? Color.green : Color.red;
        if (cameraTransform != null) Debug.DrawRay(cameraTransform.position, cameraTransform.forward * grabDistance, debugColor);
    }

    private void ShowInteractionPrompt(string prompt)
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
            // Aquí se actualizaría el texto del prompt
        }
    }

    private void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void TryGrab()
    {
        if (DetectInteractable(out var hit, out var interactable))
        {
            currentInteractable = interactable;
            currentHitInfo = hit;
            currentInteractable.OnGrabbed(transform);
        }
    }

    private void Release()
    {
        currentInteractable?.OnReleased();
        currentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(cameraTransform.position + cameraTransform.forward * grabDistance, grabRadius);
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * grabDistance);
        }
    }
}
