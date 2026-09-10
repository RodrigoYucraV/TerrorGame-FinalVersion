using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryVisibilityController : MonoBehaviour
{
    [Header("Configuración Global")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float visibleDuration = 3.0f;

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;

    private float _timer;
    private bool _isVisible;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        _canvasGroup =
            GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;

        _isVisible = false;
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (!_isVisible)
            return;

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            Hide();
        }
    }


    // ============================================================
    // MOSTRAR CON FADE
    // ============================================================
    //
    // Se utiliza cuando el jugador mueve el inventario
    // con la rueda del mouse.
    //
    // ============================================================

    public void ShowBriefly()
    {
        _timer = visibleDuration;

        _fadeTween?.Kill();

        _isVisible = true;

        _fadeTween =
            _canvasGroup
                .DOFade(
                    1f,
                    fadeDuration
                )
                .SetEase(Ease.OutQuad);
    }


    // ============================================================
    // MOSTRAR INMEDIATAMENTE
    // ============================================================
    //
    // Se utiliza cuando el jugador RECOGE un item.
    //
    // No queremos que el item aparezca transparente.
    //
    // ============================================================

    public void ShowImmediately()
    {
        _fadeTween?.Kill();

        _isVisible = true;

        _timer = visibleDuration;

        // Sin fade.
        // La UI aparece inmediatamente al 100%.

        _canvasGroup.alpha = 1f;
    }


    // ============================================================
    // OCULTAR
    // ============================================================

    private void Hide()
    {
        _isVisible = false;

        _fadeTween?.Kill();

        _fadeTween =
            _canvasGroup
                .DOFade(
                    0f,
                    fadeDuration
                )
                .SetEase(Ease.InQuad);
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }
}