using UnityEngine;

/// <summary>
/// يخلّي أثر الأقدام يبان فقط داخل مخروط الفلاش البنفسجي (كشف لكل بكسل).
/// ينحط على أوبجكت "الأوفرلي" نفسه (اللي عليه مادة Custom/TrailGlowOverlay).
/// كل فريم يغذّي الشيدر بموقع/اتجاه/مدى الفلاش وحالته (مضوّي أو لا)،
/// والشيدر يحسب لكل بكسل هل هو داخل المخروط فيكشف الأثر هناك فقط.
///
/// إعداد سريع:
/// 1. حط هذا السكربت على أوبجكت الأوفرلي (floor 2 / wall 2).
/// 2. اربط Flashlight بمكوّن PurpleFlashlight الموجود على فلاش اللاعب.
/// 3. خلّ Ambient Reveal = 0 عشان الأثر يختفي تماماً بدون ضوء (أو 0.05 لو تبيه خافت دايم).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class TrailRevealLight : MonoBehaviour
{
    [Header("الفلاش البنفسجي")]
    [Tooltip("مكوّن PurpleFlashlight على فلاش اللاعب.")]
    public PurpleFlashlight flashlight;

    [Header("الكشف")]
    [Tooltip("كم يبان الأثر خارج الضوء: 0 = مخفي تماماً، 0.05 = خافت بالكاد يبان.")]
    [Range(0f, 1f)] public float ambientReveal = 0f;

    [Tooltip("يوسّع/يضيّق مخروط الكشف نسبةً لزاوية الفلاش الفعلية (1 = نفسها).")]
    [Range(0.3f, 1.5f)] public float coneScale = 1f;

    private Renderer _renderer;
    private Material _material;
    private Light _light;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material; // نفس النسخة اللي يغذّيها SurfaceTrailPainter
    }

    private void Start()
    {
        if (flashlight != null) _light = flashlight.GetLight();

        if (flashlight == null)
        {
            Debug.LogWarning("[TrailRevealLight] ما فيه Flashlight مربوط — الكشف بالفلاش معطّل، " +
                             "الأثر بيبان عادي.", this);
            _material.SetFloat("_RevealEnabled", 0f);
            enabled = false;
            return;
        }

        _material.SetFloat("_RevealEnabled", 1f);
        _material.SetFloat("_AmbientReveal", ambientReveal);
    }

    private void Update()
    {
        if (flashlight == null) return;

        Transform lt = flashlight.transform;

        // نصف زاوية المخروط (Unity spotAngle = الزاوية الكاملة) → cos للمقارنة في الشيدر.
        float halfAngle = (_light != null ? _light.spotAngle : 45f) * 0.5f * coneScale;
        float cosHalf = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        float range = (_light != null ? _light.range : 12f);

        _material.SetVector("_LightPos", lt.position);
        _material.SetVector("_LightDir", lt.forward);
        _material.SetFloat("_LightRange", range);
        _material.SetFloat("_LightCosAngle", cosHalf);
        _material.SetFloat("_LightOn", flashlight.IsOn ? 1f : 0f);
        _material.SetFloat("_AmbientReveal", ambientReveal);
    }
}
