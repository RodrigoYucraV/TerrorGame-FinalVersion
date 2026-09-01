using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light _spotLight;
    [SerializeField] private Material cookieMaterial;
    private void Start()
    {
        if (_spotLight == null) _spotLight = GetComponentInChildren<Light>();
        if (_spotLight == null)
        {
            Debug.LogError("FlashlightController: falta una Light tipo Spot.");
            enabled = false;
            return;
        }

        _spotLight.type = LightType.Spot;
        if (cookieMaterial != null) _spotLight.cookie = cookieMaterial.mainTexture;
        _spotLight.cookieSize = 1024; // Resoluci�n acorde a tu textura
    }
    public void SetDamagedCookie(Texture2D damagedCookie)
    {
        if (_spotLight == null || damagedCookie == null) return;

        _spotLight.cookie = damagedCookie;
        _spotLight.intensity *= 0.7f; // Reducir intensidad
    }
}
