using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(FlashlightVisualController))]
public class FlashlightSystem : MonoBehaviour
{
    [SerializeField] private FlashlightConfigurator _configurator;
    [SerializeField] private FlashLigthBattery _battery;
    [SerializeField] private FlashlightVisualController _visuals;

    private IFlashlightInput _input = new FlashlightKeyboardInput();
    private bool _isActive;

    private void Awake()
    {
        _battery.Initialize();
        _battery.OnCriticalLevel += HandleCriticalBattery;
        _battery.OnBatteryDepleted += HandleBatteryDepletion;
    }

    private void HandleCriticalBattery()
    {
        _visuals.EnableFlickerEffects(true);
        _visuals.ModifySoundPitch(1.5f); // Sonido más agudo de advertencia
    }

    private void HandleBatteryDepletion()
    {
        StartCoroutine(LowBatteryFlicker());
    }

    private IEnumerator LowBatteryFlicker()
    {
        while (_battery.IsCritical && _battery.HasCharge)
        {
            _visuals.PlayFlickerEffect();
            yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        }
        _visuals.EnableFlickerEffects(false);
    }

    private void OnDestroy()
    {
        _battery.OnCriticalLevel -= HandleCriticalBattery;
        _battery.OnBatteryDepleted -= HandleBatteryDepletion;
    }

    private void Update()
    {
        HandleInput();
        UpdateBattery();
    }

    private void HandleInput()
    {
        if (_input.TogglePressed) Toggle(!_isActive);
        if (_input.RechargePressed) _battery.Recharge(30);
    }

    private void UpdateBattery()
    {
        if (_isActive)
        {
            _battery.Drain(_battery.HasCharge ? 1f : 0f);
            _visuals.SetIntensity(_battery.CurrentCharge / 100f);

            if (!_battery.HasCharge) Toggle(false);
        }
    }
    public void Toggle(bool state)
    {
        if (_isActive == state) return; // Importante para evitar retrigger

        _isActive = state;
        _visuals.Toggle(state, playSound: true); // Ahora con control explícito
    }
    private void SetInitialState(bool state)
    {
        _isActive = state;                    
    }
}
