using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerWeightSystem : MonoBehaviour
{
    [Header("Configuración del peso")]
    [SerializeField, Min(0f)]
    private float startingWeight = 120f;

    [SerializeField, Min(0f)]
    private float captureWeightThreshold = 100f;

    [SerializeField, Min(0f)]
    private float minimumWeight = 0f;

    [Header("Estado actual")]
    [SerializeField]
    private float currentWeight;

    private bool thresholdEventRaised;

    public float CurrentWeight => currentWeight;
    public float StartingWeight => startingWeight;
    public float CaptureWeightThreshold => captureWeightThreshold;

    public bool CanBeCaptured =>
        currentWeight <= captureWeightThreshold;

    public float WeightPercentage =>
        startingWeight > 0f
            ? Mathf.Clamp01(currentWeight / startingWeight)
            : 0f;

    public event Action<float> OnWeightChanged;
    public event Action OnCaptureThresholdReached;

    private void Awake()
    {
        InitializeWeight();
    }

    private void InitializeWeight()
    {
        currentWeight = Mathf.Clamp(
            startingWeight,
            minimumWeight,
            startingWeight
        );

        thresholdEventRaised = CanBeCaptured;
    }

    /// <summary>
    /// Reduce el peso del jugador.
    /// La cantidad debe ser positiva.
    /// </summary>
    public void ReduceWeight(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning(
                "[PlayerWeightSystem] La reducción de peso debe ser positiva.",
                this
            );
            return;
        }

        float previousWeight = currentWeight;

        currentWeight = Mathf.Max(
            currentWeight - amount,
            minimumWeight
        );

        if (Mathf.Approximately(previousWeight, currentWeight))
            return;

        OnWeightChanged?.Invoke(currentWeight);

        CheckCaptureThreshold();
    }

    private void CheckCaptureThreshold()
    {
        if (thresholdEventRaised || !CanBeCaptured)
            return;

        thresholdEventRaised = true;

        Debug.Log(
            "[PlayerWeightSystem] El jugador ha alcanzado el umbral de captura.",
            this
        );

        OnCaptureThresholdReached?.Invoke();
    }

    /// <summary>
    /// Restaura el peso inicial.
    /// Útil al reiniciar una partida.
    /// </summary>
    public void ResetWeight()
    {
        currentWeight = Mathf.Clamp(
            startingWeight,
            minimumWeight,
            startingWeight
        );

        thresholdEventRaised = CanBeCaptured;

        OnWeightChanged?.Invoke(currentWeight);
    }

    private void OnValidate()
    {
        startingWeight = Mathf.Max(0f, startingWeight);
        minimumWeight = Mathf.Clamp(minimumWeight, 0f, startingWeight);
        captureWeightThreshold = Mathf.Clamp(
            captureWeightThreshold,
            minimumWeight,
            startingWeight
        );

        currentWeight = Mathf.Clamp(
            currentWeight,
            minimumWeight,
            startingWeight
        );
    }
}