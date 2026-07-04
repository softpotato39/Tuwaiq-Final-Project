using UnityEngine;

/// <summary>
/// يخلّي أي ضوء **يرمش ويطقطق** بشكل مرعب (نيون معطّل / كهرباء ضعيفة).
/// حطه على أي Light (أو اربطه بحقل Target Light). مستقل تماماً — آمن للميرج.
///
/// طبقتين:
///  1. رمش مستمر ناعم (Perlin noise) حوالين الشدّة الأصلية.
///  2. انطفاء مفاجئ عشوائي (blackout) لثواني قصيرة — يخلي القلب يوقف.
/// (اختياري) صوت طنين/كهرباء يعلا وقت الرمش.
/// </summary>
public class FlickerLight : MonoBehaviour
{
    [Header("الضوء")]
    [Tooltip("اتركه فاضي ليستخدم الـ Light على نفس الأوبجكت.")]
    public Light targetLight;

    [Header("الرمش المستمر")]
    [Tooltip("سرعة الرمش (أعلى = أعصب).")]
    public float flickerSpeed = 14f;
    [Tooltip("قوة الرمش: 0 = ثابت، 1 = يوصل للإطفاء.")]
    [Range(0f, 1f)] public float flickerAmount = 0.5f;

    [Header("الانطفاء المفاجئ")]
    public bool enableBlackouts = true;
    [Tooltip("كل كم ثانية يصير انطفاء مفاجئ.")]
    public Vector2 blackoutIntervalRange = new Vector2(6f, 16f);
    [Tooltip("كم يدوم الانطفاء.")]
    public Vector2 blackoutDurationRange = new Vector2(0.08f, 0.45f);

    [Header("صوت (اختياري)")]
    [Tooltip("طنين/كهرباء — يعلا صوته وقت الرمش القوي.")]
    public AudioSource buzz;

    private float _baseIntensity;
    private float _noiseSeed;
    private float _blackoutTimer;
    private bool _inBlackout;
    private float _blackoutEnd;

    private void Awake()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        if (targetLight == null)
        {
            Debug.LogWarning("[FlickerLight] ما فيه Light مربوط.", this);
            enabled = false;
            return;
        }
        _baseIntensity = targetLight.intensity;
        _noiseSeed = Random.value * 100f;
        ResetBlackoutTimer();
    }

    private void Update()
    {
        // 1) انطفاء مفاجئ
        if (enableBlackouts)
        {
            if (_inBlackout)
            {
                if (Time.time >= _blackoutEnd)
                    _inBlackout = false;
            }
            else
            {
                _blackoutTimer -= Time.deltaTime;
                if (_blackoutTimer <= 0f)
                {
                    _inBlackout = true;
                    _blackoutEnd = Time.time + Random.Range(blackoutDurationRange.x, blackoutDurationRange.y);
                    ResetBlackoutTimer();
                }
            }
        }

        if (_inBlackout)
        {
            targetLight.intensity = 0f;
            if (buzz != null) buzz.volume = 0f;
            return;
        }

        // 2) رمش مستمر بضوضاء Perlin (غير منتظم = طبيعي ومرعب)
        float noise = Mathf.PerlinNoise(_noiseSeed, Time.time * flickerSpeed);
        float factor = Mathf.Lerp(1f, noise, flickerAmount);
        targetLight.intensity = _baseIntensity * factor;

        if (buzz != null)
            buzz.volume = Mathf.Lerp(0.15f, 0.6f, 1f - factor); // يعلا كل ما خفت الضوء
    }

    private void ResetBlackoutTimer()
    {
        _blackoutTimer = Random.Range(blackoutIntervalRange.x, blackoutIntervalRange.y);
    }

    private void OnDisable()
    {
        // رجّع الضوء لطبيعته لو طفّينا السكربت.
        if (targetLight != null) targetLight.intensity = _baseIntensity;
    }
}
