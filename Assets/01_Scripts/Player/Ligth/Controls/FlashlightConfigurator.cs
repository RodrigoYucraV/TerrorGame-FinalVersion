
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlashlightConfigurator : MonoBehaviour, ILightConfigurator
{
    private Light _spotLight;

    private void Awake()
    {
        _spotLight = GetComponent<Light>();
        _spotLight.cookie = null; // Resetear al iniciar
    }

    public void SetCookie(Texture cookieTexture)
    {
        _spotLight.cookie = cookieTexture;
    }

    // Nuevo método para configuración avanzada
    public void ConfigureLight(float intensity, float spotAngle)
    {
        _spotLight.intensity = intensity;
        _spotLight.spotAngle = spotAngle;
    }
}
