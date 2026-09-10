using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHighlightController : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private CanvasGroup highlightCanvasGroup;

    [Header("Icono de animación")]
    [SerializeField] private RectTransform iconTransformToAnimate;
    [SerializeField] private CanvasGroup iconCanvasGroup;
    [SerializeField] private Image iconImage;

    [Header("Bolsillo")]
    [SerializeField] private Image pocketOverlayImage;
    [SerializeField] private bool animatePocketBulge = true;

    [Header("Animación Pickup")]
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private float startPosY = 100f;
    [SerializeField] private float endPosY = 0f;
    [SerializeField] private Ease slideEase = Ease.Linear;

    [Header("Visibilidad del Pickup")]
    [Tooltip("Tiempo que el item permanece completamente nítido y quieto.")]
    [SerializeField] private float visibleDuration = 2f;

    [Tooltip("Tiempo que tarda el item en desaparecer mientras baja.")]
    [SerializeField] private float pickupFadeDuration = 0.8f;

    [Header("Highlight")]
    [SerializeField] private float fadeDuration = 2.5f;
    [SerializeField] private float rotateAngle = 45f;
    [SerializeField] private float rotateDuration = 2f;

    private Tween _fadeTween;
    private Tween _rotateTween;
    private Sequence _pickupSequence;

    private Vector3 _pocketInitialScale = Vector3.one;

    // Identifica cuál es la animación actual.
    // Evita que una animación anterior interfiera con una nueva.
    private int _pickupVersion = 0;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        // --------------------------------------------------------
        // HIGHLIGHT
        // --------------------------------------------------------

        if (highlightCanvasGroup == null && highlight != null)
        {
            highlightCanvasGroup =
                highlight.GetComponent<CanvasGroup>();

            if (highlightCanvasGroup == null)
            {
                highlightCanvasGroup =
                    highlight.AddComponent<CanvasGroup>();
            }
        }

        // --------------------------------------------------------
        // ICONO DE PICKUP
        // --------------------------------------------------------

        if (iconTransformToAnimate != null)
        {
            if (iconCanvasGroup == null)
            {
                iconCanvasGroup =
                    iconTransformToAnimate.GetComponent<CanvasGroup>();

                if (iconCanvasGroup == null)
                {
                    iconCanvasGroup =
                        iconTransformToAnimate.gameObject
                        .AddComponent<CanvasGroup>();
                }
            }

            // ====================================================
            // MUY IMPORTANTE
            // ====================================================
            //
            // El icono de pickup NO debe heredar el alpha
            // de los CanvasGroup padres.
            //
            // Esto evita que InventoryVisibilityController
            // lo haga aparecer transparente.
            //
            iconCanvasGroup.ignoreParentGroups = true;
        }

        // --------------------------------------------------------
        // BOLSILLO
        // --------------------------------------------------------

        if (pocketOverlayImage != null)
        {
            _pocketInitialScale =
                pocketOverlayImage.transform.localScale;
        }

        ResetPickupVisualState();
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        if (highlight != null)
            highlight.SetActive(false);

        if (pocketOverlayImage != null)
            pocketOverlayImage.enabled = true;
    }


    // ============================================================
    // PICKUP
    // ============================================================

    public void PlayPickupAnimation(
        InventoryItemData pickedItemData)
    {
        // --------------------------------------------------------
        // VALIDAR ITEM
        // --------------------------------------------------------

        if (pickedItemData == null)
        {
            Debug.LogError(
                "[Pickup] pickedItemData es NULL."
            );

            return;
        }

        if (pickedItemData.icon == null)
        {
            Debug.LogError(
                $"[Pickup] '{pickedItemData.name}' no tiene icon asignado."
            );

            return;
        }

        // --------------------------------------------------------
        // VALIDAR REFERENCIAS
        // --------------------------------------------------------

        if (iconTransformToAnimate == null ||
            iconCanvasGroup == null ||
            iconImage == null)
        {
            Debug.LogError(
                $"[Pickup] Faltan referencias del icono de animación " +
                $"en '{gameObject.name}'."
            );

            return;
        }

        // --------------------------------------------------------
        // NUEVA VERSIÓN
        // --------------------------------------------------------

        int currentVersion = ++_pickupVersion;

        // Cancelar cualquier animación anterior.
        KillPickupTweens();

        // --------------------------------------------------------
        // POSICIÓN INICIAL
        // --------------------------------------------------------

        Vector2 position =
            iconTransformToAnimate.anchoredPosition;

        position.y = startPosY;

        iconTransformToAnimate.anchoredPosition =
            position;

        iconTransformToAnimate.localScale =
            Vector3.one;


        // --------------------------------------------------------
        // ALPHA INICIAL
        // --------------------------------------------------------
        //
        // EL ITEM COMIENZA 100% NÍTIDO.
        //
        // --------------------------------------------------------

        iconCanvasGroup.ignoreParentGroups = true;

        iconCanvasGroup.alpha = 1f;

        iconCanvasGroup.interactable = false;
        iconCanvasGroup.blocksRaycasts = false;


        // --------------------------------------------------------
        // ASIGNAR SPRITE
        // --------------------------------------------------------

        iconImage.sprite =
            pickedItemData.icon;

        iconImage.enabled = true;


        // --------------------------------------------------------
        // ASEGURAR GAMEOBJECT ACTIVO
        // --------------------------------------------------------

        if (!iconTransformToAnimate.gameObject.activeSelf)
        {
            iconTransformToAnimate.gameObject.SetActive(true);
        }


        // ========================================================
        // CREAR SECUENCIA
        // ========================================================

        _pickupSequence =
            DOTween.Sequence();


        // ========================================================
        // 1. PERMANECE NÍTIDO
        // ========================================================
        //
        // Durante este tiempo:
        //
        // Alpha = 1
        // Posición = startPosY
        //
        // NO BAJA
        // NO HACE FADE
        //
        // ========================================================

        _pickupSequence.AppendInterval(
            visibleDuration
        );


        // ========================================================
        // 2. EMPIEZA A BAJAR
        // ========================================================

        _pickupSequence.Append(
            iconTransformToAnimate
                .DOAnchorPosY(
                    endPosY,
                    slideDuration
                )
                .SetEase(slideEase)
        );


        // ========================================================
        // 3. FADE OUT MIENTRAS BAJA
        // ========================================================

        _pickupSequence.Join(
            iconCanvasGroup
                .DOFade(
                    0f,
                    pickupFadeDuration
                )
                .SetEase(Ease.InQuad)
        );


        // ========================================================
        // 4. ANIMACIÓN DEL BOLSILLO
        // ========================================================

        if (pocketOverlayImage != null &&
            animatePocketBulge)
        {
            _pickupSequence.Insert(
                visibleDuration + 0.1f,

                pocketOverlayImage.transform
                    .DOPunchScale(
                        new Vector3(
                            0.15f,
                            0f,
                            0f
                        ),
                        slideDuration * 0.8f,
                        5,
                        0.5f
                    )
            );
        }


        // ========================================================
        // 5. FINAL
        // ========================================================

        Sequence sequenceCreated =
            _pickupSequence;

        sequenceCreated.OnComplete(() =>
        {
            // Si ya existe un pickup más reciente,
            // esta animación no puede modificarlo.

            if (currentVersion != _pickupVersion)
                return;

            ResetPickupVisualState();

            if (_pickupSequence == sequenceCreated)
            {
                _pickupSequence = null;
            }
        });
    }


    // ============================================================
    // RESET
    // ============================================================

    private void ResetPickupVisualState()
    {
        if (iconTransformToAnimate != null)
        {
            Vector2 position =
                iconTransformToAnimate.anchoredPosition;

            position.y = startPosY;

            iconTransformToAnimate.anchoredPosition =
                position;

            iconTransformToAnimate.localScale =
                Vector3.one;
        }


        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.ignoreParentGroups = true;

            iconCanvasGroup.alpha = 0f;

            iconCanvasGroup.interactable = false;
            iconCanvasGroup.blocksRaycasts = false;
        }


        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }


        if (pocketOverlayImage != null)
        {
            pocketOverlayImage.transform.localScale =
                _pocketInitialScale;
        }
    }


    // ============================================================
    // CANCELAR TWEENS
    // ============================================================

    private void KillPickupTweens()
    {
        if (_pickupSequence != null)
        {
            _pickupSequence.Kill();
            _pickupSequence = null;
        }

        if (iconTransformToAnimate != null)
        {
            iconTransformToAnimate.DOKill();
        }

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.DOKill();
        }

        if (pocketOverlayImage != null)
        {
            pocketOverlayImage.transform.DOKill();
        }
    }


    // ============================================================
    // HIGHLIGHT
    // ============================================================

    public void SetHighlight(bool active)
    {
        _fadeTween?.Kill();
        _rotateTween?.Kill();

        if (active)
            ActivateHighlight();
        else
            DeactivateHighlight();
    }


    private void ActivateHighlight()
    {
        if (highlight == null ||
            highlightCanvasGroup == null)
            return;

        highlight.SetActive(true);

        _fadeTween =
            highlightCanvasGroup
                .DOFade(
                    0.5f,
                    fadeDuration
                )
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                );

        _rotateTween =
            highlight.transform
                .DOLocalRotate(
                    new Vector3(
                        0f,
                        0f,
                        rotateAngle
                    ),
                    rotateDuration,
                    RotateMode.LocalAxisAdd
                )
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                );
    }


    private void DeactivateHighlight()
    {
        if (highlight == null ||
            highlightCanvasGroup == null)
            return;

        _fadeTween =
            highlightCanvasGroup
                .DOFade(
                    0f,
                    fadeDuration
                )
                .OnComplete(() =>
                {
                    if (highlight != null)
                        highlight.SetActive(false);
                });

        _rotateTween =
            highlight.transform
                .DOLocalRotate(
                    Vector3.zero,
                    rotateDuration * 0.5f
                );
    }


    // ============================================================
    // DESTROY
    // ============================================================

    private void OnDestroy()
    {
        _pickupVersion++;

        KillPickupTweens();

        _fadeTween?.Kill();
        _rotateTween?.Kill();
    }
}