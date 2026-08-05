using System;
using UnityEngine;

[System.Serializable]
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public event Action OnDeath;
    public event Action<float> OnHealthChanged; // Para UI

    private void Start() => currentHealth = maxHealth;

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Jugador muerto");
    }
}