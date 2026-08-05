using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    [SerializeField] private Image sanityBar;
    [SerializeField] private float dangerThreshold = 0.3f;
    [SerializeField] private Color dangerColor = Color.red;

    private ISanityProvider _sanityProvider;
    private bool _isInDangerZone;

    // Método Initialize requerido
    public void Initialize(ISanityProvider provider)
    {
        _sanityProvider = provider;
        _sanityProvider.OnSanityChanged += UpdateSanityUI;
    }

    private void UpdateSanityUI(float sanityPct)
    {
        sanityBar.fillAmount = sanityPct;

        if (sanityPct <= dangerThreshold && !_isInDangerZone)
        {
            _isInDangerZone = true;
            sanityBar.color = dangerColor;
        }
        else if (sanityPct > dangerThreshold)
        {
            _isInDangerZone = false;
            sanityBar.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        if (_sanityProvider != null)
        {
            _sanityProvider.OnSanityChanged -= UpdateSanityUI;
        }
    }
}
