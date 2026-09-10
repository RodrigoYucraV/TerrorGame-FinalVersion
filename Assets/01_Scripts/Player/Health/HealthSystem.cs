using System;
using UnityEngine;

[System.Serializable]
public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPct => maxHealth > 0f ? currentHealth / maxHealth : 0f;

    public event Action OnDeath;
    public event Action OnHealthDepleted;
    public event Action<float> OnDamageTaken;
    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(HealthPct);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(HealthPct);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(HealthPct);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        OnHealthDepleted?.Invoke();
        Debug.Log("Jugador muerto (HealthSystem)");
    }
}
