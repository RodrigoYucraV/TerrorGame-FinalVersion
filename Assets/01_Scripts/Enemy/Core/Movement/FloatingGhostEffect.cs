using UnityEngine;

[DisallowMultipleComponent]
public class FloatingGhostEffect : MonoBehaviour
{
    [Header("Flotación vertical")]
    [SerializeField, Min(0f)] private float verticalAmplitude = 0.08f;
    [SerializeField, Min(0.01f)] private float verticalFrequency = 0.8f;

    [Header("Balanceo")]
    [SerializeField, Min(0f)] private float pitchAmplitude = 1.5f;
    [SerializeField, Min(0f)] private float yawAmplitude = 2f;
    [SerializeField, Min(0f)] private float rollAmplitude = 1.2f;
    [SerializeField, Min(0.01f)] private float rotationFrequency = 0.5f;

    [Header("Variación")]
    [SerializeField] private bool randomizePhase = true;
    [SerializeField] private float phaseOffset = 0f;

    [Header("Control")]
    [SerializeField] private bool effectEnabled = true;
    [SerializeField, Min(0f)] private float intensity = 1f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (randomizePhase)
        {
            phaseOffset = Random.Range(0f, 100f);
        }
    }

    private void LateUpdate()
    {
        if (!effectEnabled)
            return;

        float time = Time.time + phaseOffset;

        ApplyFloating(time);
        ApplySway(time);
    }

    private void ApplyFloating(float time)
    {
        float verticalOffset =
            Mathf.Sin(time * verticalFrequency * Mathf.PI * 2f)
            * verticalAmplitude
            * intensity;

        transform.localPosition =
            initialLocalPosition + Vector3.up * verticalOffset;
    }

    private void ApplySway(float time)
    {
        float pitch =
            Mathf.Sin(time * rotationFrequency * Mathf.PI * 2f)
            * pitchAmplitude
            * intensity;

        float yaw =
            Mathf.Sin((time * rotationFrequency * 0.73f) + 1.7f)
            * yawAmplitude
            * intensity;

        float roll =
            Mathf.Sin((time * rotationFrequency * 1.21f) + 3.2f)
            * rollAmplitude
            * intensity;

        Quaternion floatingRotation =
            Quaternion.Euler(pitch, yaw, roll);

        transform.localRotation =
            initialLocalRotation * floatingRotation;
    }

    public void SetEffectEnabled(bool enabled)
    {
        effectEnabled = enabled;

        if (!enabled)
            ResetVisualTransform();
    }

    public void SetIntensity(float newIntensity)
    {
        intensity = Mathf.Max(0f, newIntensity);
    }

    public float GetIntensity()
    {
        return intensity;
    }

    private void ResetVisualTransform()
    {
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
    }
}