using UnityEngine;

/// <summary>
/// ينحط على الوحش. الـ Animation Events في كليب المشي تستدعي
/// SpawnFootprint("left") و SpawnFootprint("right") عند لمس كل قدم للسطح.
/// يطلق Raycast من عظمة القدم باتجاه السطح، وعند نقطة التلامس ياخذ إحداثيات الـ UV
/// ويعطيها لـ SurfaceTrailPainter اللي على نفس السطح المضروب.
///
/// شرط أساسي: السطح لازم يكون عنده Mesh Collider (عشان hit.textureCoord يرجّع UV صحيح)
/// + عليه مكوّن SurfaceTrailPainter.
/// </summary>
public class FootprintSpawner : MonoBehaviour
{
    [Header("عظام القدم")]
    public Transform leftFootBone;
    public Transform rightFootBone;

    [Header("كشف السطح")]
    [Tooltip("الطبقات اللي يُسمح للراي يضربها. يفضّل تحديدها على طبقة الأرضيات/الجدران فقط.")]
    public LayerMask surfaceMask = ~0;
    [Tooltip("أقصى مسافة بحث عن السطح تحت القدم.")]
    public float surfaceSearchDistance = 1f;

    [Header("اتجاه القدم")]
    [Tooltip("تصحيح زاوية شكل القدم لو طلعت مقلوبة/منحرفة (بالدرجات). جرّب 0 أو 180.")]
    public float angleOffset = 0f;
    [Tooltip("لو اليمين/اليسار مقلوبين، فعّل هذا.")]
    public bool swapLeftRight = false;

    /// <summary>تُستدعى من Animation Event بقيمة "left" أو "right".</summary>
    public void SpawnFootprint(string foot)
    {
        Transform targetBone = (foot == "left") ? leftFootBone : rightFootBone;
        if (targetBone == null) return;

        // اتجاه السطح = أسفل الوحش المحلي (يشتغل على الأرض والجدران والسقف).
        Vector3 surfaceDown = -transform.up;
        // نبدأ الراي فوق القدم شوي عشان ما يبدأ من داخل/تحت السطح.
        Vector3 rayOrigin = targetBone.position - surfaceDown * 0.2f;

        if (!Physics.Raycast(rayOrigin, surfaceDown, out RaycastHit hit,
                            surfaceSearchDistance, surfaceMask, QueryTriggerInteraction.Ignore))
            return;

        SurfaceTrailPainter painter = hit.collider.GetComponent<SurfaceTrailPainter>();
        if (painter == null) return;

        Vector2 uv = hit.textureCoord;

        // اتجاه المشي في فضاء الـ UV: راي ثاني للأمام شوي ونقارن فرق الـ UV.
        // هذي الطريقة تشتغل مهما كان توزيع UV الأرض (ما نفترض محاور).
        float angle = 0f;
        Vector3 fwdOrigin = rayOrigin + transform.forward * 0.15f;
        if (Physics.Raycast(fwdOrigin, surfaceDown, out RaycastHit hitFwd,
                            surfaceSearchDistance, surfaceMask, QueryTriggerInteraction.Ignore)
            && hitFwd.collider == hit.collider)
        {
            Vector2 d = hitFwd.textureCoord - uv;
            if (d.sqrMagnitude > 1e-8f)
                angle = Mathf.Atan2(d.y, d.x) - Mathf.PI * 0.5f;
        }
        angle += angleOffset * Mathf.Deg2Rad;

        // قلب أفقي للتفريق بين اليمين واليسار.
        bool isLeft = (foot == "left") ^ swapLeftRight;
        float flip = isLeft ? -1f : 1f;

        painter.PaintAt(uv, angle, flip);
    }
}
