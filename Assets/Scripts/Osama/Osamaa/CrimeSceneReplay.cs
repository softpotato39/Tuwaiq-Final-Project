using System.Collections;
using UnityEngine;

/// <summary>
/// إعادة تمثيل الجريمة (رؤية بالأداة / زر G للتجربة):
///  - عند التشغيل: القاتل (وأي أشباح ثابتة) يظهرون فوراً بجلتش، والقاتل يبدأ حركة الطلق.
///  - بعد Victim Appear Time: الضحية **تتجسّد** (تظهر من العدم) بجلتش وتقف واقفة.
///  - عند Time To Shot: صوت الطلقة + الوميض، والضحية تسقط.
///  - بعد Scene Duration: الكل يختفي.
///
/// كل شخصية عندها أنميشن واحد بس (Entry → الحالة مباشرة، بدون Parameters).
/// السكربت ينادي Animator.Play(stateName) لإعادة تشغيل الحركة من البداية.
/// خلّ Loop Time مطفي على الكليبات + Culling Mode = Always Animate.
/// مستقل تماماً — آمن للميرج.
/// </summary>
public class CrimeSceneReplay : MonoBehaviour
{
    [Header("يظهرون فوراً (عند G)")]
    [Tooltip("أوبجكتات تظهر فوراً عند التشغيل (القاتل + أي أشباح ثابتة). لا تحط الضحية هنا.")]
    public GameObject[] characters;
    [Tooltip("GhostGlitchController على هؤلاء — burst لحظة ظهورهم (اختياري).")]
    public GhostGlitchController[] glitchOnReveal;

    [Header("القاتل")]
    public Animator killer;
    [Tooltip("اسم حالة أنيميتر القاتل (نفس اسم العقدة في نافذة Animator).")]
    public string killerShootState = "Shoot";

    [Header("الضحية (تتجسّد متأخرة)")]
    public Animator victim;
    [Tooltip("اسم حالة أنيميتر الضحية (مثل 'Victim Reaction').")]
    public string victimReactionState = "Victim Reaction";
    [Tooltip("بعد كم ثانية من التشغيل تظهر الضحية (مختفية تماماً قبلها).")]
    public float victimAppearTime = 1.2f;
    [Tooltip("GhostGlitchController حق الضحية — burst لحظة ظهورها (اختياري).")]
    public GhostGlitchController victimGlitch;
    [Tooltip("تظهر الضحية واقفة وتسقط وقت الطلقة. لو مطفي: تسقط أول ما تظهر.")]
    public bool victimFallsOnShot = true;

    [Header("الطلقة")]
    public AudioSource gunAudio;
    public AudioClip gunClip;
    [Tooltip("وميض الكاتم (اختياري) — يشتغل لحظة الطلق.")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime = 0.07f;
    [Tooltip("وقت صوت الطلقة + الوميض + سقوط الضحية (من بداية المشهد).")]
    public float timeToShot = 1.6f;

    [Header("التوقيت")]
    [Tooltip("مدة المشهد الكاملة قبل ما يختفي الكل.")]
    public float sceneDuration = 4.5f;
    public bool hideAfter = true;
    public bool startHidden = true;
    public bool oneShot = false;

    [Header("اختبار")]
    [Tooltip("زر يشغّل المشهد للتجربة (بدون الأداة). None = معطّل.")]
    public KeyCode testKey = KeyCode.G;

    private bool _playing;
    private bool _played;

    private void Start()
    {
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
        if (startHidden)
        {
            SetHidden(characters, true);
            SetKillerHidden(true);
            SetVictimHidden(true);
        }
    }

    private void Update()
    {
        if (testKey != KeyCode.None && Input.GetKeyDown(testKey))
            Play();
    }

    /// <summary>يشغّل مشهد الجريمة. تقدر تناديها من أداة أو Trigger.</summary>
    public void Play()
    {
        if (_playing || (oneShot && _played)) return;
        _played = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        _playing = true;

        // 1) القاتل (والأشباح الثابتة) يظهرون فوراً بجلتش، والقاتل يبدأ الطلق.
        SetHidden(characters, false);
        SetKillerHidden(false);
        if (glitchOnReveal != null)
            foreach (var gc in glitchOnReveal)
                if (gc != null) gc.TriggerBurst();
        if (killer != null && !string.IsNullOrEmpty(killerShootState))
            killer.Play(killerShootState, 0, 0f);

        // 2) استنى لين تتجسّد الضحية.
        yield return new WaitForSeconds(victimAppearTime);

        // 3) الضحية تظهر من العدم بجلتش، وتقف واقفة (فريم 0).
        SetVictimHidden(false);
        if (victimGlitch != null) victimGlitch.TriggerBurst();
        if (victim != null && !string.IsNullOrEmpty(victimReactionState))
        {
            victim.Play(victimReactionState, 0, 0f);
            victim.speed = victimFallsOnShot ? 0f : 1f; // 0 = واقفة تنتظر الطلقة
        }

        // 4) استنى لحظة الطلقة → صوت + وميض + الضحية تسقط.
        yield return new WaitForSeconds(Mathf.Max(0f, timeToShot - victimAppearTime));
        if (gunAudio != null && gunClip != null) gunAudio.PlayOneShot(gunClip);
        if (muzzleFlash != null) StartCoroutine(FlashRoutine());
        if (victim != null && victimFallsOnShot && !string.IsNullOrEmpty(victimReactionState))
        {
            victim.speed = 1f;
            victim.Play(victimReactionState, 0, 0f);
        }

        // 5) استنى نهاية المشهد ثم اخفِ الكل.
        yield return new WaitForSeconds(Mathf.Max(0f, sceneDuration - timeToShot));
        if (hideAfter)
        {
            SetHidden(characters, true);
            SetKillerHidden(true);
            SetVictimHidden(true);
        }
        _playing = false;
    }

    private IEnumerator FlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleFlash.SetActive(false);
    }

    private void SetHidden(GameObject[] arr, bool hidden)
    {
        if (arr == null) return;
        foreach (GameObject c in arr) SetRenderers(c, !hidden);
    }

    private void SetKillerHidden(bool hidden)
    {
        if (killer != null) SetRenderers(killer.gameObject, !hidden);
    }

    private void SetVictimHidden(bool hidden)
    {
        if (victim != null) SetRenderers(victim.gameObject, !hidden);
    }

    private void SetRenderers(GameObject go, bool enabled)
    {
        if (go == null) return;
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            r.enabled = enabled;
    }
}
