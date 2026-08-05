using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryVisibilityController : MonoBehaviour
{
    [Header("Configuración Global")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleDuration = 3.0f; // Tiempo que se queda visible

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;
    private float _timer;
    private bool _isVisible;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // Estado inicial: Invisible
        _canvasGroup.alpha = 0f;
        _isVisible = false;
    }

    private void Update()
    {
        // Lógica de temporizador (si está visible, contar hacia atrás)
        if (_isVisible)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                Hide();
            }
        }
    }

    // Este es el método público que llamarán otros scripts
    public void ShowBriefly()
    {
        _timer = visibleDuration; // Reiniciar contador

        if (!_isVisible)
        {
            _isVisible = true;
            _fadeTween?.Kill(); // Matar animaciones anteriores
            _fadeTween = _canvasGroup.DOFade(1f, fadeDuration);
        }
    }

    private void Hide()
    {
        _isVisible = false;
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(0f, fadeDuration);
    }
}
