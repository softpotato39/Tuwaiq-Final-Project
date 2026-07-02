using UnityEngine;

/// <summary>
/// يخلّي أي غرض (كتابة على جدار، بصمة، رسمة...) **يبان فقط داخل ضوء الفلاش البنفسجي**.
/// حطه على الغرض اللي تبيه يكون مخفي ويظهر بالكشاف، وخلاص.
///
/// مزايا للفريق:
///  - كل غرض مستقل بذاته (ما فيه قائمة مركزية) → صفر تعارض عند الميرج.
///  - يلقى PurpleFlashlight تلقائياً (ما تحتاج تربطه يدوياً لكل غرض).
///  - يشتغل مع مادتنا Custom/RevealGlow (_Alpha) ومع URP Lit (_BaseColor)
///    ومع Standard (_Color) — يضبط الشفافية على أي منهم.
///
/// طريقة الاستخدام السريعة:
///  1. سوِّ Quad على الجدار، حط عليه مادة فيها صورة الكتابة/البصمة (يفضّل شيدر Custom/RevealGlow).
///  2. ضيف هذا المكوّن. خلاص — يصير مخفي ويبان بس لما يطيح عليه ضوء الفلاش.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class RevealUnderFlashlight : MonoBehaviour
{
    [Header("الفلاش")]
    [Tooltip("اتركه فاضي ليلقى PurpleFlashlight تلقائياً في المشهد.")]
    public PurpleFlashlight flashlight;

    [Header("الكشف")]
    [Tooltip("كم يبان الغرض خارج الضوء: 0 = مخفي تماماً، 0.05 = خافت بالكاد.")]
    [Range(0f, 1f)] public float ambientReveal = 0f;
    [Tooltip("سرعة الظهور/الاختفاء مع حركة الفلاش.")]
    public float revealFadeSpeed = 6f;
    [Tooltip("0 = يستخدم زاوية مخروط الفلاش نفسها. غير كذا = نصف زاوية مخصصة (درجات).")]
    public float overrideHalfAngle = 0f;
    [Tooltip("يتطلب خط رؤية مباشر (ما فيه جدار يحجب الضوء عن الغرض).")]
    public bool requireLineOfSight = false;
    public LayerMask obstructionMask = ~0;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private float _reveal;
    private float _findTimer;

    private int _idAlpha, _idBaseColor, _idColor;
    private bool _hasAlpha, _hasBaseColor, _hasColor;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        _idAlpha     = Shader.PropertyToID("_Alpha");
        _idBaseColor = Shader.PropertyToID("_BaseColor");
        _idColor     = Shader.PropertyToID("_Color");

        Material mat = _renderer.sharedMaterial;
        _hasAlpha     = mat != null && mat.HasProperty(_idAlpha);
        _hasBaseColor = mat != null && mat.HasProperty(_idBaseColor);
        _hasColor     = mat != null && mat.HasProperty(_idColor);

        Apply(ambientReveal); // يبدأ مخفي (أو خافت حسب ambientReveal)
    }

    private void Start()
    {
        if (flashlight == null)
            flashlight = FindFirstObjectByType<PurpleFlashlight>();
    }

    private void Update()
    {
        // الفلاش قد يتسبِّن وقت اللعب (لما يلتقطه اللاعب) → ندوّر عليه لو ما لقيناه بعد.
        if (flashlight == null)
        {
            _findTimer -= Time.deltaTime;
            if (_findTimer <= 0f)
            {
                flashlight = FindFirstObjectByType<PurpleFlashlight>();
                _findTimer = 0.3f;
            }
        }

        float target = (flashlight != null && flashlight.IsOn && InsideCone()) ? 1f : ambientReveal;
        _reveal = Mathf.MoveTowards(_reveal, target, revealFadeSpeed * Time.deltaTime);
        Apply(_reveal);
    }

    private void Apply(float a)
    {
        // إطفاء الرندرر تماماً وقت الاختفاء (أداء + يضمن إنه مخفي 100%).
        bool visible = a > 0.001f;
        if (_renderer.enabled != visible) _renderer.enabled = visible;
        if (!visible) return;

        _renderer.GetPropertyBlock(_mpb);
        if (_hasAlpha)
        {
            _mpb.SetFloat(_idAlpha, a);
        }
        else if (_hasBaseColor)
        {
            Color c = _renderer.sharedMaterial.GetColor(_idBaseColor); c.a = a;
            _mpb.SetColor(_idBaseColor, c);
        }
        else if (_hasColor)
        {
            Color c = _renderer.sharedMaterial.GetColor(_idColor); c.a = a;
            _mpb.SetColor(_idColor, c);
        }
        _renderer.SetPropertyBlock(_mpb);
    }

    private bool InsideCone()
    {
        Transform lt = flashlight.transform;
        Vector3 to = transform.position - lt.position;
        float dist = to.magnitude;

        Light light = flashlight.GetLight();
        if (light != null && dist > light.range) return false;

        float halfAngle = overrideHalfAngle > 0f
            ? overrideHalfAngle
            : (light != null ? light.spotAngle * 0.5f : 22.5f);
        if (Vector3.Angle(lt.forward, to) > halfAngle) return false;

        if (requireLineOfSight && dist > 0.001f)
        {
            if (Physics.Raycast(lt.position, to / dist, out RaycastHit hit, dist, obstructionMask))
            {
                if (hit.collider.gameObject != gameObject &&
                    !hit.collider.transform.IsChildOf(transform))
                    return false;
            }
        }
        return true;
    }
}
