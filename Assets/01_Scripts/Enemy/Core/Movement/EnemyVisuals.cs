using System.Collections;
using UnityEngine;

public class EnemyVisuals : MonoBehaviour, IVisibilityController
{
    [Header("Referencias Visuales")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private ParticleSystem burnParticles;

    [Header("Audio")]

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip burnSound;

    private Material materialInstance;
    private Coroutine fadeCoroutine;

    public bool IsVisible { get; private set; } = true;

    private void Awake()
    {
        if (enemyRenderer == null)
        {
            Debug.LogError("EnemyVisuals: Falta asignar Enemy Renderer.");
            return;
        }

        // Creamos una instancia del material para no modificar
        // el material original del proyecto.
        materialInstance = enemyRenderer.material;

        // Cuando tengas las partículas:
        if (burnParticles != null)
            burnParticles.Stop();
    }

    public void SetVisibility(bool visible)
    {
        IsVisible = visible;

        if (enemyRenderer != null)
            enemyRenderer.enabled = visible;
    }

    public void FadeOut(float duration)
    {
        //Efectos al comenzar a quemarse.
        // Cuando tengas las partículas y el sonido, descomenta:

        if (burnParticles != null)
            burnParticles.Play();

        if (audioSource != null && burnSound != null)
            audioSource.PlayOneShot(burnSound);

        StartFade(1f, 0f, duration);
    }

    public void FadeIn(float duration)
    {
        // Cuando tengas las partículas:
        if (burnParticles != null)
            burnParticles.Stop();

        StartFade(0f, 1f, duration);
    }

    private void StartFade(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            DoFade(startAlpha, endAlpha, duration)
        );
    }

    private IEnumerator DoFade(
        float startAlpha,
        float endAlpha,
        float duration)
    {
        if (materialInstance == null)
            yield break;

        if (enemyRenderer != null)
            enemyRenderer.enabled = true;

        float elapsed = 0f;

        Color baseColor = materialInstance.color;
        baseColor.a = startAlpha;
        materialInstance.color = baseColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;

            float currentAlpha =
                Mathf.Lerp(startAlpha, endAlpha, t);

            Color newColor = materialInstance.color;
            newColor.a = currentAlpha;
            materialInstance.color = newColor;

            yield return null;
        }

        Color finalColor = materialInstance.color;
        finalColor.a = endAlpha;
        materialInstance.color = finalColor;

        if (endAlpha <= 0f)
        {
            IsVisible = false;

            if (enemyRenderer != null)
                enemyRenderer.enabled = false;
        }
        else
        {
            IsVisible = true;

            if (enemyRenderer != null)
                enemyRenderer.enabled = true;
        }

        fadeCoroutine = null;
    }
}