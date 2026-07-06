using UnityEngine;

/// <summary>مجموعة أصوات خطوة لسطح معيّن.</summary>
[System.Serializable]
public class SurfaceFootstep
{
    [Tooltip("اسم السطح: Metal, Tile, Wood... (يطابق SurfaceTag أو Tag الأرض).")]
    public string surface = "Default";
    [Tooltip("أصوات خطوة لهذا السطح — يختار وحد عشوائي كل خطوة.")]
    public AudioClip[] clips;
}

/// <summary>
/// أصوات خطوات اللاعب — **تتغيّر حسب السطح** اللي يمشي عليه.
/// حطه على اللاعب. كل ما يمشي مسافة Step Distance، يطلق راي لتحت،
/// يعرف نوع السطح (من مكوّن SurfaceTag، أو Tag الأرض)، ويشغّل صوت عشوائي
/// من مجموعة ذاك السطح — مع تنويع بسيط في الطبقة/الصوت عشان ما يتكرر ممل.
///
/// مستقل تماماً — آمن للميرج.
/// </summary>
public class FootstepSounds : MonoBehaviour
{
    [Header("الصوت")]
    [Tooltip("مصدر الصوت. فاضي = يلقاه على اللاعب أو يضيف واحد.")]
    public AudioSource audioSource;
    [Tooltip("مجموعات الأصوات لكل سطح.")]
    public SurfaceFootstep[] surfaces;
    [Tooltip("أصوات افتراضية لو السطح مو معرّف.")]
    public SurfaceFootstep defaultSurface;

    [Header("التنويع")]
    public Vector2 volumeRange = new Vector2(0.7f, 1f);
    public Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Header("الخطو")]
    [Tooltip("كم متر يمشي اللاعب بين خطوة وخطوة (أصغر = خطوات أسرع).")]
    public float stepDistance = 2.2f;
    [Tooltip("أقل سرعة عشان يعتبره ماشي (يمنع الصوت وهو واقف).")]
    public float minSpeed = 0.3f;

    [Header("كشف السطح")]
    [Tooltip("مدى الراي لتحت لكشف الأرض.")]
    public float rayDistance = 1.5f;
    [Tooltip("ارتفاع بداية الراي فوق موقع اللاعب.")]
    public float rayStartHeight = 0.3f;
    public LayerMask groundMask = ~0;

    private Vector3 _lastPos;
    private float _accumulated;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        _lastPos = transform.position;
    }

    private void Update()
    {
        // مسافة الحركة الأفقية هذا الفريم.
        Vector3 delta = transform.position - _lastPos;
        delta.y = 0f;
        _lastPos = transform.position;

        float dist = delta.magnitude;
        float speed = dist / Mathf.Max(Time.deltaTime, 0.0001f);

        if (speed < minSpeed)
        {
            _accumulated = 0f; // واقف → صفّر العدّاد
            return;
        }

        _accumulated += dist;
        if (_accumulated >= stepDistance)
        {
            _accumulated = 0f;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        // راي لتحت لكشف الأرض ونوعها.
        Vector3 origin = transform.position + Vector3.up * rayStartHeight;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                            rayDistance + rayStartHeight, groundMask, QueryTriggerInteraction.Ignore))
            return; // في الهواء → ما فيه خطوة

        AudioClip[] clips = GetClipsFor(hit.collider);
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
    }

    private AudioClip[] GetClipsFor(Collider col)
    {
        // 1) نوع السطح: من مكوّن SurfaceTag أول، وإلا من Tag الأرض.
        string surf = null;
        SurfaceTag tag = col.GetComponentInParent<SurfaceTag>();
        if (tag != null) surf = tag.surface;
        else surf = col.tag;

        // 2) لقّى المجموعة المطابقة (بدون حساسية لحالة الأحرف).
        if (surfaces != null && !string.IsNullOrEmpty(surf))
            foreach (SurfaceFootstep s in surfaces)
                if (s != null && string.Equals(s.surface, surf, System.StringComparison.OrdinalIgnoreCase))
                    return s.clips;

        // 3) الافتراضي.
        return defaultSurface != null ? defaultSurface.clips : null;
    }
}
