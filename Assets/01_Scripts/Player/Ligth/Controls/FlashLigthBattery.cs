using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLigthBattery : MonoBehaviour, IBatterySystem
{
    // Eventos para el sistema de fallos
    public event System.Action OnCriticalLevel;
    public event System.Action OnBatteryDepleted;

    [Header("Configuración")]
    [SerializeField] private float _maxCharge = 100f;
    [SerializeField] private float _drainRate = 2f;
    [SerializeField][Range(0, 1)] private float _criticalThreshold = 0.2f;

    [Header("Fallos")]
    [SerializeField] private float _failureProbability = 0.3f;
    [SerializeField] private float _minFailureDelay = 1f;
    [SerializeField] private float _maxFailureDelay = 3f;

    [SerializeField] private float _currentCharge;
    private bool _isCritical;

    public float CurrentCharge => _currentCharge;
    public bool HasCharge => _currentCharge > 0;
    public bool IsCritical => _isCritical;

    public void Initialize() => _currentCharge = _maxCharge;

    public void Drain(float amount)
    {
        amount = _drainRate;
        float previousCharge = _currentCharge;
        _currentCharge = Mathf.Max(_currentCharge - amount * Time.deltaTime, 0);

        CheckCriticalState(previousCharge);
        CheckForFailures(previousCharge);
    }

    private void CheckCriticalState(float previousCharge)
    {
        bool wasCritical = _isCritical;
        _isCritical = (_currentCharge / _maxCharge) <= _criticalThreshold;

        if (!wasCritical && _isCritical) OnCriticalLevel?.Invoke();
    }

    private void CheckForFailures(float previousCharge)
    {
        if (!_isCritical) return;
        if (Random.value < _failureProbability * Time.deltaTime)
        {
            StartCoroutine(FailureRoutine());
        }
    }

    private IEnumerator FailureRoutine()
    {
        OnBatteryDepleted?.Invoke();
        yield return new WaitForSeconds(Random.Range(_minFailureDelay, _maxFailureDelay));

        if (_isCritical && HasCharge)
        {
            StartCoroutine(FailureRoutine());
        }
    }

    public void Recharge(float amount)
    {
        _currentCharge = Mathf.Min(_currentCharge + amount, _maxCharge);
        _isCritical = false;
        StopAllCoroutines();
    }
}
