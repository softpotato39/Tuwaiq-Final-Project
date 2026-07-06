using System.Collections;
using UnityEngine;

/// <summary>
/// إعادة تمثيل الجريمة: تشغّل المشهد → الأشباح (القاتل + الضحية) تظهر بجلتش،
/// القاتل يطلق، الطلقة تتزامن مع الصوت + وميض الكاتم + سقوط الضحية.
///
/// الأشباح مخفية عادةً (renderers مطفية). عند التشغيل تظهر مع burst جلتش
/// من GhostGlitchController، ثم تشتغل أنميشنات القاتل/الضحية.
///
/// كل شخصية عندها أنميشن واحد بس، فالأنيميتور بسيط (Entry يوصل مباشرة
/// للحالة، بدون Idle ولا Parameters). السكربت ينادي Animator.Play(stateName)
/// مباشرة لإعادة تشغيل الحركة من البداية — بدون الحاجة لـ Trigger/Transition.
/// خلّ "Loop Time" مطفي على الكليبات عشان تتوقف على آخر فريم (وضعية الإطلاق/الموت).
///
/// جرّبها بزر الاختبار (G) أول. بعدها اربط Play() بأداة (IUsableTool) أو Trigger.
/// مستقل تماماً — آمن للميرج.
/// </summary>
public class CrimeSceneReplay : MonoBehaviour
{
    [Header("الشخصيات (تنخفي/تنكشف)")]
    [Tooltip("أوبجكتات القاتل والضحية — تنطفي renderers-هم حتى يجي المشهد.")]
    public GameObject[] characters;

    [Header("أنميشن")]
    public Animator killer;
    [Tooltip("اسم حالة الأنيميتور حق القاتل (مثل 'Shoot') — نفس اسم العقدة في نافذة Animator.")]
    public string killerShootState = "Shoot";
    public Animator victim;
    [Tooltip("اسم حالة الأنيميتور حق الضحية (مثل 'Victim Reaction').")]
    public string victimReactionState = "Victim Reaction";

    [Header("ظهور بجلتش (اختياري)")]
    [Tooltip("GhostGlitchController على الأشباح — يعمل burst لحظة الظهور.")]
    public GhostGlitchController[] glitchOnReveal;

    [Header("الطلقة")]
    public AudioSource gunAudio;
    public AudioClip gunClip;
    [Tooltip("وميض الكاتم (اختياري) — يشتغل لحظة الطلق.")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime = 0.07f;
    [Tooltip("الوقت من بداية المشهد إلى لحظة الطلق (زامنه مع أنميشن القاتل).")]
    public float timeToShot = 1.2f;

    [Header("التوقيت")]
    [Tooltip("مدة المشهد الكاملة قبل ما يختفي.")]
    public float sceneDuration = 4.5f;
    [Tooltip("يختفي الأشباح بعد نهاية المشهد.")]
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
        if (startHidden) SetHidden(true);
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

        // 1) كشف الأشباح + burst جلتش (يظهرون كأنهم يتشكّلون).
        SetHidden(false);
        if (glitchOnReveal != null)
            foreach (var gc in glitchOnReveal)
                if (gc != null) gc.TriggerBurst();

        // 2) القاتل يبدأ حركة التصويب/الطلق (يعيد تشغيلها من الفريم 0 دايماً).
        if (killer != null && !string.IsNullOrEmpty(killerShootState))
            killer.Play(killerShootState, 0, 0f);

        // 3) استنى للحظة الطلق.
        yield return new WaitForSeconds(timeToShot);

        // 4) الطلقة: صوت + وميض + الضحية تسقط.
        if (gunAudio != null && gunClip != null) gunAudio.PlayOneShot(gunClip);
        if (victim != null && !string.IsNullOrEmpty(victimReactionState))
            victim.Play(victimReactionState, 0, 0f);
        if (muzzleFlash != null) StartCoroutine(FlashRoutine());

        // 5) استنى نهاية المشهد.
        yield return new WaitForSeconds(Mathf.Max(0f, sceneDuration - timeToShot));

        if (hideAfter) SetHidden(true);
        _playing = false;
    }

    private IEnumerator FlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleFlash.SetActive(false);
    }

    private void SetHidden(bool hidden)
    {
        if (characters == null) return;
        foreach (GameObject c in characters)
        {
            if (c == null) continue;
            foreach (Renderer r in c.GetComponentsInChildren<Renderer>(true))
                r.enabled = !hidden;
        }
    }
}
