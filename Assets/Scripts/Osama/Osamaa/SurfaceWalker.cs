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

    private void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        SetWalking(true);
        PickNewRandomTurn();
        SnapToSurfaceImmediate();
    }

    private void Update()
    {
        // 1) انعطاف عشوائي حوالين محور "فوق" الحالي (يمين/يسار على نفس السطح)
        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0f) PickNewRandomTurn();

        if (_randomTurnRemaining > 0f)
        {
            float step = Mathf.Min(randomTurnSpeed * Time.deltaTime, _randomTurnRemaining);
            transform.Rotate(transform.up, step * _randomTurnDir, Space.World);
            _randomTurnRemaining -= step;
        }

        // 2) جدار أمامي؟ → يتسلقه (حافة داخلية)
        Vector3 wallOrigin = transform.position + transform.up * heightOffset;
        if (Physics.Raycast(wallOrigin, transform.forward, wallCheckDistance, surfaceMask))
        {
            transform.Rotate(transform.right, -edgeTurnSpeed * Time.deltaTime, Space.World);
        }

        // 3) ابحث عن السطح تحت القدم
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

    private void SetWalking(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
            animator.SetBool(walkBoolParam, isWalking);
    }
}