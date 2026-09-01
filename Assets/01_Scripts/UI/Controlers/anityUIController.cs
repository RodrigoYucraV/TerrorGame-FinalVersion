using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityUIController : MonoBehaviour, ISanityProvider
{
    private ISanityProvider _sanityProvider;
    [Header("Referencias")]
    [SerializeField] private Image sanityBar;
    [SerializeField] private Image sanityOverlay;
    [SerializeField] private AudioSource heartbeatSound;

    [Header("Efectos Visuales")]
    [SerializeField] private float criticalThreshold = 0.3f;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float overlayIntensity = 0.8f;

    public event Action<float> OnSanityChanged;
    public event Action OnSanityDepleted;

    public float CurrentSanityPct => _sanityProvider != null ? _sanityProvider.CurrentSanityPct : 0f;

    private void OnEnable()
    {
        if (_sanityProvider == null)
        {
            SanitySystem sanitySystem = GetComponent<SanitySystem>();
            if (sanitySystem == null) sanitySystem = FindFirstObjectByType<SanitySystem>();
            if (sanitySystem != null) Initialize(sanitySystem);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void UpdateUI(float sanityPct)
    {
        OnSanityChanged?.Invoke(sanityPct);

        // Barra b�sica
        if (sanityBar != null) sanityBar.fillAmount = sanityPct;

        // Efectos de cordura baja
        if (sanityPct <= criticalThreshold)
        {
            float effectIntensity = 1f - (sanityPct / criticalThreshold);
            ApplyCriticalEffects(effectIntensity);
        }
        else
        {
            ResetEffects();
        }
    }

    private void ApplyCriticalEffects(float intensity)
    {
        if (sanityOverlay != null)
        {
            sanityOverlay.color = new Color(criticalColor.r, criticalColor.g, criticalColor.b, intensity * overlayIntensity);
            sanityOverlay.gameObject.SetActive(true);
        }

        if (heartbeatSound != null)
        {
            heartbeatSound.volume = intensity;
            if (!heartbeatSound.isPlaying) heartbeatSound.Play();
        }
    }

    private void ResetEffects()
    {
        if (sanityOverlay != null) sanityOverlay.gameObject.SetActive(false);
        if (heartbeatSound != null) heartbeatSound.Stop();
    }
    public void Initialize(ISanityProvider provider)
    {
        Unsubscribe();
        _sanityProvider = provider;
        if (_sanityProvider == null) return;

        _sanityProvider.OnSanityChanged += UpdateUI;
        _sanityProvider.OnSanityDepleted += HandleSanityDepleted;
        UpdateUI(_sanityProvider.CurrentSanityPct);
    }

    private void HandleSanityDepleted() => OnSanityDepleted?.Invoke();

    private void Unsubscribe()
    {
        if (_sanityProvider == null) return;

        _sanityProvider.OnSanityChanged -= UpdateUI;
        _sanityProvider.OnSanityDepleted -= HandleSanityDepleted;
        _sanityProvider = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
