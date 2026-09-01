using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class InventoryHighlightController : MonoBehaviour
{
    public Sprite testSprite;
    [Header("Componentes Propios")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private CanvasGroup highlightCanvasGroup;

    [Header("Referencias al Ítem")]
    // ESTE ES EL QUE MOVEREMOS (El contenedor o la imagen misma)
    [SerializeField] private RectTransform iconTransformToAnimate;
    [SerializeField] private CanvasGroup iconCanvasGroup;
    [SerializeField] private Image iconImage;

    [Header("Referencias al Bolsillo")]
    // ✅ NUEVO: La imagen del bolsillo que va POR DELANTE del ítem
    [SerializeField] private Image pocketOverlayImage;
    // ✅ NUEVO: Un pequeño "bulto" en el bolsillo al guardar algo
    [SerializeField] private bool animatePocketBulge = true;

    [Header("Configuración Animación ")]
    [SerializeField] private float slideDuration = 0.5f;
    [Tooltip("La posición Y desde donde empieza a bajar el ítem (arriba)")]
    [SerializeField] private float startPosY = 100f;
    [Tooltip("La posición Y final donde queda guardado (abajo, asomando un poco)")]
    [SerializeField] private float endPosY = -30f; // Ajusta este valor negativo para que asome más o menos
    [SerializeField] private Ease slideEase = Ease.OutQuart;


    [Header("Configuración Animación Highlight (Rotación)")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float rotateAngle = 45f;
    [SerializeField] private float rotateDuration = 0.8f;

    private Tween _fadeTween;
    private Tween _rotateTween;
    private Sequence _pickupSequence; // Usaremos una secuencia para ordenar la animación
    private Vector3 _pocketInitialScale = Vector3.one;

    private void Awake()
    {
        if (highlightCanvasGroup == null && highlight != null) highlightCanvasGroup = highlight.GetComponent<CanvasGroup>();
        if (highlightCanvasGroup == null && highlight != null) highlightCanvasGroup = highlight.AddComponent<CanvasGroup>();
        if (iconCanvasGroup == null && iconTransformToAnimate != null) iconCanvasGroup = iconTransformToAnimate.GetComponent<CanvasGroup>();
        if (iconCanvasGroup == null && iconTransformToAnimate != null) iconCanvasGroup = iconTransformToAnimate.gameObject.AddComponent<CanvasGroup>();

        if (pocketOverlayImage != null) _pocketInitialScale = pocketOverlayImage.transform.localScale;

        ResetPickupVisualState(false);
    }

    private void Start()
    {
        // --- Validaciones de seguridad ---
        if (highlight == null) Debug.LogError($"[UI CRITICAL] Falta 'highlight' en {gameObject.name}");
        if (iconTransformToAnimate == null) Debug.LogWarning($"[UI INFO] Falta 'iconTransformToAnimate' en {gameObject.name}");

        if (highlight != null) highlight.SetActive(false);

        // Asegurar que el bolsillo frontal sea visible si existe
        if (pocketOverlayImage != null) pocketOverlayImage.enabled = true;
    }

    // --- Lógica de selección (Se mantiene igual) ---
    public void SetHighlight(bool active)
    {
        // 1. Matamos cualquier animación (de encendido o de apagado) que estuviera ocurriendo
        _fadeTween?.Kill();
        _rotateTween?.Kill();

        //if (active) ActivateHighlight();
        //else DeactivateHighlight();
    }

    // ✅ NUEVA LÓGICA DE RECOGER ÍTEM (Bolsillo)
    public void PlayPickupAnimation(InventoryItemData pickedItemData)
    {
        if (pickedItemData == null)
        {
            Debug.LogError("🚨 ¡ALERTA! Me pidieron animar, pero 'pickedItemData' llegó VACÍO (Null). Revisa tu script de Inventario.");
        }
        else if (pickedItemData.icon == null)
        {
            Debug.LogError($"🚨 ¡ALERTA! El ítem '{pickedItemData.name}' llegó bien, pero NO TIENE FOTO asignada en su ScriptableObject.");
        }
        else
        {
            Debug.Log($"✅ Todo correcto: Animizando la llave: {pickedItemData.name}");
        }
        // -----------------------------------

        // 1. Limpieza previa
        KillPickupTweens();
        ResetPickupVisualState(true);

        // 2. Actualizar Datos
        if (iconImage != null && pickedItemData != null)
        {
            iconImage.sprite = pickedItemData.icon;
            iconImage.enabled = true;
        }

        // 3. Crear la secuencia de animación
        _pickupSequence = DOTween.Sequence();

        if (iconTransformToAnimate != null && iconCanvasGroup != null)
        {
            //Bajar hasta la posición final (endPosY)
            _pickupSequence.Append(iconTransformToAnimate.DOAnchorPosY(endPosY, slideDuration).SetEase(slideEase));

       
            _pickupSequence.Join(iconCanvasGroup.DOFade(1f, slideDuration * 0.03f)); // Aparece rápido al principio

            
            if (pocketOverlayImage != null && animatePocketBulge)
            {
                // Hacemos que el bolsillo se ensanche un poquito justo cuando el ítem entra
                // Usamos Insert para que ocurra un poquito después de empezar a bajar (ej. a los 0.1s)
                _pickupSequence.Insert(0.1f, pocketOverlayImage.transform.DOPunchScale(new Vector3(0.15f, 0f, 0f), slideDuration * 0.8f, 5, 0.5f));
            }

            // Al finalizar la animación, reseteamos la posición inicial Y y ocultamos el elemento
            _pickupSequence.OnComplete(() =>
            {
                ResetPickupVisualState(false);
            });
        }
    }

    private void KillPickupTweens()
    {
        _pickupSequence?.Kill(false);
        _pickupSequence = null;

        if (iconTransformToAnimate != null) iconTransformToAnimate.DOKill(false);
        if (iconCanvasGroup != null) iconCanvasGroup.DOKill(false);
        if (pocketOverlayImage != null) pocketOverlayImage.transform.DOKill(false);
    }

    private void ResetPickupVisualState(bool visibleForAnimation)
    {
        if (iconTransformToAnimate != null)
        {
            Vector2 startPosition = iconTransformToAnimate.anchoredPosition;
            startPosition.y = startPosY;
            iconTransformToAnimate.anchoredPosition = startPosition;
            iconTransformToAnimate.localScale = Vector3.one;
        }

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.alpha = 0f;
            iconCanvasGroup.interactable = false;
            iconCanvasGroup.blocksRaycasts = false;
        }

        if (iconImage != null) iconImage.enabled = visibleForAnimation;
        if (pocketOverlayImage != null) pocketOverlayImage.transform.localScale = _pocketInitialScale;
    }

    // --- Métodos privados de Highlight (Sin cambios) ---
    private void ActivateHighlight()
    {
        if (highlight == null) return;
        highlight.SetActive(true);

        // Guardamos las animaciones infinitas
        _fadeTween = highlightCanvasGroup.DOFade(0.5f, fadeDuration).SetLoops(-1, LoopType.Yoyo);
        _rotateTween = highlight.transform.DOLocalRotate(new Vector3(0, 0, rotateAngle), rotateDuration, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Yoyo);
    }

    private void DeactivateHighlight()
    {
        if (highlight == null) return;

        // ✅ LA MEJORA: También guardamos las animaciones de apagado en las variables.
        // Así, si el jugador se arrepiente y vuelve a seleccionar este slot rápido, 
        // el Kill() de arriba podrá cancelar este apagado a medias.
        _fadeTween = highlightCanvasGroup.DOFade(0, fadeDuration).OnComplete(() => highlight.SetActive(false)); // Solo se apaga cuando termina el fade
        _rotateTween = highlight.transform.DOLocalRotate(Vector3.zero, rotateDuration * 0.5f);
    }
}
