using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBatterySystem
{
    float CurrentCharge { get; }
    void Drain(float amount);
    void Recharge(float amount);
    bool HasCharge { get; }
}

public interface IFlashlightInput
{
    bool TogglePressed { get; }
    bool RechargePressed { get; }
}

public interface IFlashlightVisuals
{
    void SetIntensity(float intensity);
    void Toggle(bool state, bool playSound = true);
    void PlayFlickerEffect();
}
public interface ILightConfigurator
{
    void SetCookie(Texture cookieTexture); 
    void ConfigureLight(float intensity, float spotAngle);

}