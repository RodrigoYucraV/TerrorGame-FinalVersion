using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light _spotLight;
    [SerializeField] private Material cookieMaterial;
    private void Start()
    {
        _spotLight.type = LightType.Spot;
        _spotLight.cookie = cookieMaterial.mainTexture;
        _spotLight.cookieSize = 1024; // Resolución acorde a tu textura
    }
    public void SetDamagedCookie(Texture2D damagedCookie)
    {
        _spotLight.cookie = damagedCookie;
        _spotLight.intensity *= 0.7f; // Reducir intensidad
    }
}
