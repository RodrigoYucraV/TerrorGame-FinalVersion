using UnityEngine;

public interface IInteractable
{
    void OnGrabbed(Transform holder);
    void OnReleased();
}

public class ObjectGrabbler : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private float grabRadius = 0.2f; // ✅ NUEVO: El grosor del rayo
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;
    private RaycastHit currentHitInfo; // Guardamos la info del hit para usarla luego

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
            if (currentInteractable == null)
                TryGrab();
            else
                Release();
        }
    }

    // ✅ Refactorización para evitar código duplicado (DRY)
    // Este método devuelve bool y saca el hit y el componente si lo encuentra
    private bool DetectInteractable(out RaycastHit hitInfo, out IInteractable interactable)
    {
        hitInfo = new RaycastHit();
        interactable = null;
        if (cameraTransform == null) return false;

        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        // 1. SPHERECAST: Lanza una esfera en lugar de una línea
        // Funciona igual que Raycast pero con 'radius'
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
            // Si el objeto tiene el script en el padre (común en objetos complejos)
            if (interactable == null)
                interactable = hitInfo.collider.GetComponentInParent<IInteractable>();

            return interactable != null;
        }

        return false;
    }

    private void CheckInteractable()
    {
        // Usamos el método centralizado de detección
        bool found = DetectInteractable(out var hit, out var interactable);

        // Debug Visual
        Color debugColor = found ? Color.green : Color.red;

        // Dibujamos la línea central
        if (cameraTransform != null) Debug.DrawRay(cameraTransform.position, cameraTransform.forward * grabDistance, debugColor);

        // Opcional: Dibujar la esfera en el punto de impacto para ver el volumen
        if (found)
        {
            // Esto es solo visualización rápida, Unity tiene Gizmos para esto mejor
            // (Ver método OnDrawGizmos abajo)
        }
    }

    private void TryGrab()
    {
        if (DetectInteractable(out var hit, out var interactable))
        {
            currentInteractable = interactable;
            currentHitInfo = hit;

            // ✅ MEJORA: Pasamos el 'transform' pero ahora es mucho más fácil acertar
            currentInteractable.OnGrabbed(transform);
        }
    }

    private void Release()
    {
        currentInteractable?.OnReleased();
        currentInteractable = null;
    }

    // ✅ DEBUGGING PROFESIONAL:
    // Esto dibujará la esfera en el editor para que veas el tamaño real del agarre
    private void OnDrawGizmosSelected()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            // Dibuja el cable del SphereCast
            Gizmos.DrawWireSphere(cameraTransform.position + cameraTransform.forward * grabDistance, grabRadius);
            // Dibuja la línea que conecta
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * grabDistance);
        }
    }
}
