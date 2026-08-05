using System.Collections;
using UnityEngine;

public class EnemyVisuals : MonoBehaviour, IVisibilityController
{
    [Header("Referencias Visuales")]
    [SerializeField] private Renderer enemyRenderer; // Asigna el MeshRenderer de la cápsula
    //[SerializeField] private ParticleSystem burnParticles; // Tu sistema de partículas

    [Header("Audio")]
    //[SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip burnSound;

    private Material materialInstance;
    public bool IsVisible { get; private set; } = true;

    private void Awake()
    {
        // Obtenemos una instancia del material para no modificar el original del proyecto
        if (enemyRenderer != null)
        {
            materialInstance = enemyRenderer.material;
        }

        // Configuración inicial de partículas
        //if (burnParticles != null) burnParticles.Stop();
    }

    public void SetVisibility(bool visible)
    {
        IsVisible = visible;
        // Opcional: Activar/Desactivar el renderer de golpe si no quieres fade
        if (enemyRenderer != null) enemyRenderer.enabled = visible;
        gameObject.SetActive(visible);
    }

    public void FadeOut(float duration)
    {
        // Activar partículas y sonido al empezar a quemarse
        //if (burnParticles != null) burnParticles.Play();
        //if (audioSource != null && burnSound != null) audioSource.PlayOneShot(burnSound);
        StartCoroutine(DoFade(1f, 0f, duration));
    }

    public void FadeIn(float duration)
    {
        //if (burnParticles != null) burnParticles.Stop();
        StartCoroutine(DoFade(0f, 1f, duration));
    }

    private IEnumerator DoFade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color initialColor = materialInstance.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);

            // Cambiamos el Alpha del color
            Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, currentAlpha);
            materialInstance.color = newColor;

            yield return null; // Esperar al siguiente frame
        }

        materialInstance.color = new Color(initialColor.r, initialColor.g, initialColor.b, endAlpha);

        if (endAlpha == 0)
        {
            IsVisible = false;
            if (enemyRenderer != null) enemyRenderer.enabled = false;
            gameObject.SetActive(false);
        }
        else
        {
            IsVisible = true;
            if (enemyRenderer != null) enemyRenderer.enabled = true;
        }
    }
}
