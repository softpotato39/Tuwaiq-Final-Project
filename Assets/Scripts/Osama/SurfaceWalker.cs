using UnityEngine;

/// <summary>
/// يخلي الوحش يمشي على أي سطح (أرض/جدار/سقف) كأنه يمشي على الأرض.
/// يلتصق بالسطح ويلتف حول الحواف الداخلية (يتسلق الجدار) والخارجية (يلف حول الحافة).
///
/// إعداد سريع:
/// 1. عطّل (شيل علامة الصح) مكوّن NavMesh Agent على هذا الوحش — لا يصلح للجدران.
/// 2. عطّل مكوّن Monster AI (لأنه يعتمد على NavMesh).
/// 3. ضيف هذا السكربت على نفس أوبجكت الـ Animator.
/// 4. تأكد إن الأرض والجدران والسقف عندهم Collider.
/// </summary>
[DisallowMultipleComponent]
public class SurfaceWalker : MonoBehaviour
{
    [Header("الحركة")]
    public float moveSpeed = 2f;
    public float surfaceAlignSpeed = 12f;     // سرعة محاذاة الجسم مع السطح
    public float edgeTurnSpeed = 220f;        // سرعة الالتفاف حول الحواف (درجة/ثانية)

    [Header("التجوال العشوائي")]
    public Vector2 turnIntervalRange = new Vector2(2.5f, 5f);
    public float maxRandomTurn = 90f;         // أقصى زاوية انعطاف عشوائي
    public float randomTurnSpeed = 90f;       // سرعة الانعطاف العشوائي (درجة/ثانية)

    [Header("التوقّف (غموض)")]
    [Tooltip("لو مفعّل، الوحش يوقف كل فترة فالأثر يخبو ويضيع على اللاعب.")]
    public bool enablePauses = true;
    [Tooltip("كم يمشي قبل ما يقف (ثواني).")]
    public Vector2 walkDurationRange = new Vector2(4f, 8f);
    [Tooltip("كم يبقى واقف (ثواني).")]
    public Vector2 pauseDurationRange = new Vector2(1.5f, 4f);

    [Header("نمط الحركة")]
    [Tooltip("مفعّل = يتمشّى على الأرض فقط؛ لو صادف جدار أو حافة يلتف ويرجع (ما يتسلّق). " +
             "مطفي = يتسلّق الجدران والسقف.")]
    public bool floorOnly = true;

    [Header("كشف الأسطح")]
    public float stickDistance = 0.6f;        // مدى البحث عن السطح تحت القدم
    public float wallCheckDistance = 0.5f;    // مدى كشف الجدار الأمامي
    public float heightOffset = 0.05f;        // ارتفاع الجسم فوق السطح
    public LayerMask surfaceMask = ~0;

    [Header("الأنميتر (اختياري)")]
    public Animator animator;
    public string walkBoolParam = "IsWalking";

    private float _turnTimer;
    private float _randomTurnRemaining;
    private int _randomTurnDir = 1;

    private bool _paused;
    private float _stateTimer;

    private void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _paused = false;
        SetWalking(true);
        _stateTimer = Random.Range(walkDurationRange.x, walkDurationRange.y);
        PickNewRandomTurn();
        SnapToSurfaceImmediate();
    }

    private void Update()
    {
        // 0) تبديل بين المشي والتوقّف (يخلي الأثر يضيع وقت الوقوف).
        if (enablePauses)
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f) TogglePause();
        }

        // وهو واقف: يحافظ على التصاقه بالسطح بس بدون حركة/انعطاف/تسلّق.
        if (_paused)
        {
            StickToSurface();
            return;
        }

        // 1) انعطاف عشوائي حوالين محور "فوق" الحالي (يمين/يسار على نفس السطح)
        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0f) PickNewRandomTurn();

        if (_randomTurnRemaining > 0f)
        {
            float step = Mathf.Min(randomTurnSpeed * Time.deltaTime, _randomTurnRemaining);
            transform.Rotate(transform.up, step * _randomTurnDir, Space.World);
            _randomTurnRemaining -= step;
        }

        // 2) جدار أمامي؟
        Vector3 wallOrigin = transform.position + transform.up * heightOffset;
        bool wallAhead = Physics.Raycast(wallOrigin, transform.forward, wallCheckDistance, surfaceMask);

        if (floorOnly)
        {
            // أرض فقط: لو جدار قدّام أو ما فيه أرض قدّام (حافة) → التف حول محور "فوق" لتفاديه.
            Vector3 aheadProbe = transform.position + transform.forward * 0.4f + transform.up * heightOffset;
            bool floorAhead = Physics.Raycast(aheadProbe, -transform.up, stickDistance + heightOffset + 0.4f, surfaceMask);

            if (wallAhead || !floorAhead)
                transform.Rotate(transform.up, edgeTurnSpeed * Time.deltaTime, Space.World);

            StickToSurface(); // يحاذي الأرض بدون تسلّق
        }
        else
        {
            // النمط الكامل: يتسلّق الجدران والسقف ويلتف حول الحواف.
            if (wallAhead)
                transform.Rotate(transform.right, -edgeTurnSpeed * Time.deltaTime, Space.World);

            Vector3 origin = transform.position + transform.up * heightOffset;
            if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, stickDistance + heightOffset + 0.3f, surfaceMask))
            {
                transform.position = hit.point + hit.normal * heightOffset;
                Quaternion target = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, target, surfaceAlignSpeed * Time.deltaTime);
            }
            else
            {
                // ما فيه سطح تحت → حافة خارجية: يلف حولها للأسفل
                transform.Rotate(transform.right, edgeTurnSpeed * Time.deltaTime, Space.World);
            }
        }

        // 4) تحرّك للأمام على السطح
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void SnapToSurfaceImmediate()
    {
        if (Physics.Raycast(transform.position + transform.up * 0.3f, -transform.up, out RaycastHit hit, 5f, surfaceMask))
        {
            transform.position = hit.point + hit.normal * heightOffset;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
    }

    private void PickNewRandomTurn()
    {
        _turnTimer = Random.Range(turnIntervalRange.x, turnIntervalRange.y);
        _randomTurnRemaining = Random.Range(0f, maxRandomTurn);
        _randomTurnDir = Random.value < 0.5f ? -1 : 1;
    }

    // يبدّل بين المشي والوقوف ويحدّث الأنميتر (وقت الوقوف ما يطلع Animation Events فالأثر يخبو).
    private void TogglePause()
    {
        _paused = !_paused;
        SetWalking(!_paused);
        _stateTimer = _paused
            ? Random.Range(pauseDurationRange.x, pauseDurationRange.y)
            : Random.Range(walkDurationRange.x, walkDurationRange.y);
    }

    // محاذاة بسيطة مع السطح بدون حركة أمامية (تُستخدم وقت الوقوف عشان ما يطفو/يطيح).
    private void StickToSurface()
    {
        Vector3 origin = transform.position + transform.up * heightOffset;
        if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, stickDistance + heightOffset + 0.3f, surfaceMask))
        {
            transform.position = hit.point + hit.normal * heightOffset;
            Quaternion target = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, target, surfaceAlignSpeed * Time.deltaTime);
        }
    }

    private void SetWalking(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
            animator.SetBool(walkBoolParam, isWalking);
    }
}