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

    public float CurrentSanityPct => throw new NotImplementedException();

    private void OnEnable()
    {
        GetComponent<SanitySystem>().OnSanityChanged += UpdateUI;
    }

    private void OnDisable()
    {
        GetComponent<SanitySystem>().OnSanityChanged -= UpdateUI;
    }

    private void UpdateUI(float sanityPct)
    {
        // Barra básica
        sanityBar.fillAmount = sanityPct;

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
        sanityOverlay.color = new Color(criticalColor.r, criticalColor.g, criticalColor.b, intensity * overlayIntensity);
        sanityOverlay.gameObject.SetActive(true);

        heartbeatSound.volume = intensity;
        if (!heartbeatSound.isPlaying) heartbeatSound.Play();
    }

    private void ResetEffects()
    {
        sanityOverlay.gameObject.SetActive(false);
        heartbeatSound.Stop();
    }
    public void Initialize(ISanityProvider provider)
    {
        _sanityProvider = provider;
        _sanityProvider.OnSanityChanged += UpdateUI;
        _sanityProvider.OnSanityDepleted += OnSanityDepleted;
    }
    private void OnDestroy()
    {
        if (_sanityProvider != null)
        {
            _sanityProvider.OnSanityChanged -= UpdateUI;
            _sanityProvider.OnSanityDepleted -= OnSanityDepleted;
        }
    }
}
