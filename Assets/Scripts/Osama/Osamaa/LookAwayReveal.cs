using UnityEngine;

/// <summary>
/// رعب "تلتفت يتغيّر شي": لما اللاعب يبصّ على هذا الغرض (مرآة مثلاً) ثم يلتفت عنه،
/// يظهر شي مخفي (كتابة دم، وجه، ظل...) — فيلقاه موجود لما يرجع يبصّ. رعب نفسي كلاسيكي.
///
/// حطه على الغرض اللي اللاعب يبصّ عليه (المرآة/اللوحة). اربط Reveal Object
/// بالشي المخفي (يبدأ مطفي). يشتغل مستقل — آمن للميرج.
/// </summary>
public class LookAwayReveal : MonoBehaviour
{
    [Header("الكشف")]
    [Tooltip("كاميرا اللاعب. فاضي = Camera.main.")]
    public Camera playerCamera;
    [Tooltip("قد إيش لازم الغرض يكون في وسط النظر عشان نعتبره 'يبصّ عليه' (درجات).")]
    public float viewAngle = 22f;
    [Tooltip("مدى يبان فيه الغرض (متر).")]
    public float maxDistance = 8f;
    [Tooltip("يتطلب خط رؤية (ما فيه جدار يحجب).")]
    public bool requireLineOfSight = true;
    public LayerMask obstructionMask = ~0;

    [Header("الرعب")]
    [Tooltip("الشي المخفي اللي يظهر (كتابة/وجه/ظل). يبدأ مطفي.")]
    public GameObject revealObject;
    [Tooltip("صوت مفاجئ لحظة الظهور (اختياري).")]
    public AudioSource stinger;
    [Tooltip("يشتغل مرة وحدة بس.")]
    public bool oneShot = true;
    [Tooltip("لازم اللاعب يبصّ عليه أول قبل ما يتفعّل (عشان يلاحظ التغيير).")]
    public bool requireSeenFirst = true;

    private bool _hasSeen;
    private bool _triggered;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (revealObject != null) revealObject.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null || revealObject == null) return;
        if (_triggered && oneShot) return;

        bool looking = IsLookingAtMe();

        if (requireSeenFirst && !_hasSeen)
        {
            if (looking) _hasSeen = true;
            return;
        }

        // شافه (أو ما نحتاج) + التفت عنه → فعّل الرعب وهو مش شايف، فيلقاه لما يرجع.
        if (!looking)
        {
            if (!revealObject.activeSelf)
            {
                revealObject.SetActive(true);
                if (stinger != null) stinger.Play();
                _triggered = true;
            }
        }
        else if (!oneShot && _triggered)
        {
            // وضع متكرر: يختفي وهو يبصّ عشان يعيد المفاجأة المرة الجاية.
            revealObject.SetActive(false);
            _triggered = false;
            _hasSeen = true;
        }
    }

    private bool IsLookingAtMe()
    {
        Vector3 toObj = transform.position - playerCamera.transform.position;
        float dist = toObj.magnitude;
        if (dist > maxDistance) return false;
        if (Vector3.Angle(playerCamera.transform.forward, toObj) > viewAngle) return false;

        if (requireLineOfSight && dist > 0.01f)
        {
            if (Physics.Raycast(playerCamera.transform.position, toObj / dist, out RaycastHit hit, dist, obstructionMask))
            {
                if (hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
                    return false;
            }
        }
        return true;
    }
}
