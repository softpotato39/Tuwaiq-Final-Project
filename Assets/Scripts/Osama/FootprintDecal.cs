using UnityEngine;
using UnityEngine.Rendering.Universal; // DecalProjector

/// <summary>
/// ينحط على بريفاب الخطوة الواحدة. يدعم نوعين:
///   - URP Decal Projector (واقعي بـ Normal Map) → يتحكم في fadeFactor.
///   - Quad عادي بمادة Custom/FootprintDecal → يتحكم في _Alpha.
/// يخبو مع العمر ثم يدمّر نفسه، و(اختياري) يبان فقط داخل مخروط الفلاش البنفسجي.
/// </summary>
public class FootprintDecal : MonoBehaviour
{
    [Header("العمر")]
    [Tooltip("كم ثانية تعيش الخطوة قبل ما تختفي تماماً.")]
    public float lifetime = 6f;
    [Tooltip("مدة الظهور التدريجي أول ما تطلع.")]
    public float fadeInTime = 0.15f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    [Header("الكشف بالفلاش (اختياري)")]
    [Tooltip("لو مربوط: الخطوة تبان فقط داخل مخروط الفلاش. لو فاضي: تبان دايم.")]
    public PurpleFlashlight flashlight;
    [Tooltip("كم تبان الخطوة خارج الضوء: 0 = مخفية تماماً.")]
    [Range(0f, 1f)] public float ambientReveal = 0f;
    [Tooltip("سرعة ظهور/اختفاء الخطوة مع حركة الفلاش.")]
    public float revealFadeSpeed = 6f;
    [Tooltip("يتطلب خط رؤية مباشر من الفلاش (ما فيه جدار يحجب).")]
    public bool requireLineOfSight = false;
    public LayerMask obstructionMask = ~0;

    private DecalProjector _projector;   // مسار الديكال الواقعي (URP)
    private Material _material;           // مسار الـ Quad
    private float _age;
    private float _reveal;

    private void Awake()
    {
        _projector = GetComponent<DecalProjector>();
        if (_projector != null)
        {
            _projector.fadeFactor = 0f; // يبدأ مخفي ويظهر تدريجياً
        }
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                _material = r.material;          // نسخة مستقلة لكل خطوة
                _material.SetFloat("_Alpha", 0f); // يبدأ مخفي ويظهر تدريجياً
            }
        }
    }

    private void Update()
    {
        _age += Time.deltaTime;
        if (_age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // خبو العمر: ظهور سريع، ثم اختفاء في آخر 40% من العمر.
        float lifeFade = Mathf.Clamp01(_age / Mathf.Max(fadeInTime, 0.0001f));
        float tail = Mathf.InverseLerp(lifetime, lifetime * 0.6f, _age); // 1 ثم ينزل لـ0
        lifeFade *= Mathf.Clamp01(tail);

        // هدف الكشف: لو ما فيه فلاش → دايم ظاهر؛ غير كذا حسب المخروط.
        float target;
        if (flashlight == null) target = 1f;
        else target = (flashlight.IsOn && InsideLightCone()) ? 1f : ambientReveal;

        _reveal = Mathf.MoveTowards(_reveal, target, revealFadeSpeed * Time.deltaTime);

        float alpha = maxAlpha * lifeFade * _reveal;
        if (_projector != null) _projector.fadeFactor = alpha;
        else if (_material != null) _material.SetFloat("_Alpha", alpha);
    }

    private bool InsideLightCone()
    {
        Transform lt = flashlight.transform;
        Vector3 to = transform.position - lt.position;
        float dist = to.magnitude;

        Light light = flashlight.GetLight();
        if (light != null && dist > light.range) return false;

        float halfAngle = (light != null ? light.spotAngle : 45f) * 0.5f;
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
