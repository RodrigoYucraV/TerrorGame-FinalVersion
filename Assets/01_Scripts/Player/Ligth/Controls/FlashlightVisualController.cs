using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightVisualController : MonoBehaviour, IFlashlightVisuals
{
    [Header("Componentes")]
    [SerializeField] private Light _spotlight;
    [SerializeField] private AudioSource _audioSource;

    [Header("Config")]
    [SerializeField] private float _maxIntensity = 50f;
    [SerializeField] private AudioClip _toggleSound;

    [Header("Fallos")]
    [SerializeField] private AudioClip _flickerSound;
    [SerializeField] private Vector2 _flickerIntensityRange = new Vector2(15f, 35f);

    private Coroutine _flickerRoutine;

    public void EnableFlickerEffects(bool state)
    {
        if (state && _flickerRoutine == null)
        {
            _flickerRoutine = StartCoroutine(RandomFlickerRoutine());
        }
        else if (!state && _flickerRoutine != null)
        {
            StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
            _spotlight.intensity = _maxIntensity;
        }
    }

    public void ModifySoundPitch(float pitch)
    {
        _audioSource.pitch = pitch;
    }

    private IEnumerator RandomFlickerRoutine()
    {
        while (true)
        {
            float randomDelay = Random.Range(2f, 5f);
            yield return new WaitForSeconds(randomDelay);

            // Flicker complejo con variación de intensidad
            float originalIntensity = _spotlight.intensity;
            _spotlight.intensity = Random.Range(_flickerIntensityRange.x, _flickerIntensityRange.y);
            _audioSource.PlayOneShot(_flickerSound);

            yield return new WaitForSeconds(0.1f);
            _spotlight.intensity = originalIntensity;
        }
    }
    public void SetIntensity(float intensity)
    {
        _spotlight.intensity = Mathf.Lerp(0, _maxIntensity, intensity);
    }

    public void Toggle(bool state, bool playSound = true)
    {
        if (_spotlight.enabled == state) return; 

        _spotlight.enabled = state;

        if (playSound && _toggleSound != null)
        {
            _audioSource.PlayOneShot(_toggleSound);
        }
    }

    public void PlayFlickerEffect()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        // Implementar lógica de parpadeo complejo
        _spotlight.enabled = false;
        yield return new WaitForSeconds(0.1f);
        _spotlight.enabled = true;
    }
}
