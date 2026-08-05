using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image healthFillImage; // Arrastra aquí la imagen de relleno (Foreground)
    [SerializeField] private EnemyLightSensitivity healthSystem; // El script de vida del enemigo
    [SerializeField] private Canvas canvas;

    private float maxHealthCache;

    private void Start()
    {
        // Buscar el sistema de salud en el padre si no está asignado
        if (healthSystem == null)
            healthSystem = GetComponentInParent<EnemyLightSensitivity>();

        if (healthSystem != null)
        {
            // Suscribirse al evento de daño
            healthSystem.OnDamageTaken += UpdateHealthBar;
            // Inicializar barra
            maxHealthCache = healthSystem.CurrentHealth > 0 ? healthSystem.CurrentHealth : 100f;
            UpdateHealthBar(0); // Actualizar visualmente al inicio
        }

        // Asegurar que la cámara renderice este canvas (importante para WorldSpace)
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void UpdateHealthBar(float damageAmount)
    {
        if (healthSystem != null && healthFillImage != null)
        {
            // Calculamos porcentaje (0 a 1)
            float fillAmount = healthSystem.CurrentHealth / maxHealthCache;
            healthFillImage.fillAmount = fillAmount;
        }
    }

    // Hacer que la barra mire siempre al jugador (Billboard)
    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null) healthSystem.OnDamageTaken -= UpdateHealthBar;
    }
}