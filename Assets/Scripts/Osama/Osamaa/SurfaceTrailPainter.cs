using UnityEngine;

/// <summary>
/// اللوحة الحيّة لآثار الأقدام المتوهّجة.
/// تنحط على السطح الأصلي (مثلاً floor 1) اللي عليه Mesh Collider.
/// تستقبل مواقع الخطوات (UV) من FootprintSpawner، ترسمها في RenderTexture
/// بتقنية ping-pong (بفرين يتبادلان) وتخبو كل فريم حسب Decay Rate،
/// وتغذّي مادة أوبجكت "الأوفرلي" بالتكستشر الناتج.
///
/// مهم: الراي يرجّع UV صحيح فقط لو ضرب Mesh Collider — أي كولايدر بدائي
/// (Box/Sphere/Capsule) يرجّع دايماً (0,0). فالسطح هنا لازم يكون عنده Mesh Collider فقط.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class SurfaceTrailPainter : MonoBehaviour
{
    [Header("إعدادات اللوحة")]
    public int textureSize = 512;
    public Color trailColor = new Color(0.55f, 0.2f, 0.85f, 1f);
    [Range(0.001f, 0.3f)] public float stampRadius = 0.04f;
    [Range(0.9f, 0.999f)] public float decayRate = 0.992f;

    [Header("شكل الخطوة")]
    [Tooltip("تكستشر قدم (أبيض على أسود). لو فاضي → بقعة دائرية. مثال: Assets/Animations/Osama/textures/alpha")]
    public Texture2D footTexture;
    [Tooltip("عرض القدم ÷ طولها (القدم عادة أطول من عرضها).")]
    [Range(0.1f, 1f)] public float footAspect = 0.45f;

    [Header("العرض (مادة الأوفرلي)")]
    [Tooltip("أوبجكت الأوفرلي اللي يعرض الأثر المتوهّج (نسخة السطح المرفوعة قليلاً).")]
    public Renderer overlayRenderer;

    private RenderTexture _bufferA;
    private RenderTexture _bufferB;
    private RenderTexture _current;
    private Material _stampMat;
    private bool _active;   // يصير false لو الشيدر مفقود — يعطّل اللوحة بأمان بدل ما يطيّح اللعبة.

    private Vector2 _pendingUV;
    private float _pendingAngle;
    private float _pendingFlip = 1f;
    private bool _hasPendingStamp;

    private void Awake()
    {
        // تحصين ضد فقدان الشيدر عند نقل النظام لمشروع ثاني.
        Shader stampShader = Shader.Find("Hidden/TrailStamp");
        if (stampShader == null)
        {
            Debug.LogWarning("[SurfaceTrailPainter] ما لقيت الشيدر 'Hidden/TrailStamp' — " +
                             "تأكد إن TrailStamp.shader موجود وما فيه خطأ كمبايل. اللوحة معطّلة مؤقتاً.", this);
            enabled = false;
            return;
        }

        _bufferA = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        _bufferB = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        _bufferA.Create();
        _bufferB.Create();

        _stampMat = new Material(stampShader);

        _current = _bufferA;
        ClearRT(_bufferA);
        ClearRT(_bufferB);
        _active = true;

        if (overlayRenderer != null)
            overlayRenderer.material.SetTexture("_TrailTex", _current);
    }

    private void ClearRT(RenderTexture rt)
    {
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prevActive;
    }

    private void Update()
    {
        if (!_active) return;

        RenderTexture target = (_current == _bufferA) ? _bufferB : _bufferA;

        _stampMat.SetFloat("_DecayRate", decayRate);
        _stampMat.SetColor("_StampColor", trailColor);
        _stampMat.SetFloat("_StampRadius", stampRadius);

        bool useFoot = footTexture != null;
        _stampMat.SetFloat("_UseFootTex", useFoot ? 1f : 0f);
        if (useFoot)
        {
            _stampMat.SetTexture("_FootTex", footTexture);
            _stampMat.SetFloat("_StampAspect", footAspect);
        }

        if (_hasPendingStamp)
        {
            _stampMat.SetVector("_StampUV", new Vector4(_pendingUV.x, _pendingUV.y, 0, 0));
            _stampMat.SetFloat("_StampAngle", _pendingAngle);
            _stampMat.SetFloat("_StampFlip", _pendingFlip);
            _stampMat.SetFloat("_StampActive", 1f);
            _hasPendingStamp = false;
        }
        else
        {
            _stampMat.SetFloat("_StampActive", 0f);
        }

        // _current يُمرّر كـ _MainTex تلقائياً (مصدر الـBlit) داخل شيدر التراكم.
        Graphics.Blit(_current, target, _stampMat);
        _current = target;

        if (overlayRenderer != null)
            overlayRenderer.material.SetTexture("_TrailTex", _current);
    }

    /// <summary>يرسم بصمة دائرية عند إحداثيات UV (للتوافق مع النداء القديم).</summary>
    public void PaintAt(Vector2 uv)
    {
        PaintAt(uv, 0f, 1f);
    }

    /// <summary>
    /// يرسم بصمة قدم عند UV باتجاه معيّن.
    /// angleRad = اتجاه المشي (راديان) في فضاء الـ UV.
    /// flip = ‎+1 للقدم اليمنى، ‎-1 لليسرى (قلب أفقي). يناديها FootprintSpawner.
    /// </summary>
    public void PaintAt(Vector2 uv, float angleRad, float flip)
    {
        _pendingUV = uv;
        _pendingAngle = angleRad;
        _pendingFlip = flip;
        _hasPendingStamp = true;
    }

    private void OnDestroy()
    {
        if (_bufferA != null) _bufferA.Release();
        if (_bufferB != null) _bufferB.Release();
    }
}
