using UnityEngine;
using System;

public class EnemyLightSensitivity : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float baseLightDamage = 10f; // Daño base por defecto
    private float currentHealth;

    // Dependencia: Necesitamos los ojos para saber si nos queman
    private IPlayerDetector playerDetector;

    public float CurrentHealth => currentHealth;
    public event Action OnHealthDepleted;
    public event Action<float> OnDamageTaken;

    private void Awake()
    {
        playerDetector = GetComponent<IPlayerDetector>();
        ResetHealth();
    }

    private void OnEnable() => ResetHealth();

    // ✅ SOLID SRP: La lógica de "Sufrir daño continuo por luz" pertenece aquí.
    private void Update()
    {
        // Si no hay detector, o ya estamos muertos, no hacemos nada
        if (playerDetector == null || currentHealth <= 0) return;

        // Si la linterna nos golpea...
        if (playerDetector.IsHitByFlashlight)
        {
            // Calculamos el daño aquí (o lo pedimos al GameManager)
            float damageAmount = (GamePacingManager.Instance != null)
                ? GamePacingManager.Instance.CurrentLightDamage
                : baseLightDamage;

            // Nos auto-infligimos el daño
            TakeDamage(damageAmount * Time.deltaTime);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        // Notificamos al mundo (y al Controller) que recibimos daño
        OnDamageTaken?.Invoke(amount);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnHealthDepleted?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}