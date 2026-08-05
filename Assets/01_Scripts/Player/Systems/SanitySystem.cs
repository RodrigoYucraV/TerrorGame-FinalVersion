using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanitySystem : MonoBehaviour, ISanityProvider
{
    public float CurrentSanityPct => CurrentSanity / maxSanity; [Header("Configuración")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float passiveDrainRate = 0.5f;
    [SerializeField] private float distanceEffectMultiplier = 0.1f;

    public float CurrentSanity { get; private set; }
    public event Action<float> OnSanityChanged;
    public event Action OnSanityDepleted;

    private List<ISanityAffector> _activeAffectors = new();

    private void Start() => CurrentSanity = maxSanity;

    private void Update()
    {
        ApplyPassiveDrain();
        ApplyAffectorEffects();
        UpdateSanity();
    }

    public void RegisterAffector(ISanityAffector affector) => _activeAffectors.Add(affector);
    public void UnregisterAffector(ISanityAffector affector) => _activeAffectors.Remove(affector);

    public void RestoreSanity(float amount)
    {
        CurrentSanity = Mathf.Clamp(CurrentSanity + amount, 0, maxSanity);
        OnSanityChanged?.Invoke(CurrentSanity / maxSanity);
    }

    private void ApplyPassiveDrain()
    {
        CurrentSanity -= passiveDrainRate * Time.deltaTime;
    }

    private void ApplyAffectorEffects()
    {
        foreach (var affector in _activeAffectors)
        {
            if (!affector.IsAffecting) continue;

            float distanceEffect = 1f - Mathf.Clamp01(
                Vector3.Distance(transform.position, affector.AffectOrigin) * distanceEffectMultiplier
            );

            CurrentSanity += affector.SanityEffectPerSecond * distanceEffect * Time.deltaTime;
        }
    }

    private void UpdateSanity()
    {
        CurrentSanity = Mathf.Clamp(CurrentSanity, 0, maxSanity);
        OnSanityChanged?.Invoke(CurrentSanity / maxSanity);

        if (CurrentSanity <= 0)
        {
            OnSanityDepleted?.Invoke();
        }
    }
}
