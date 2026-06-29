using UnityEngine;
using UnityEngine.Rendering.Universal; // DecalProjector

/// <summary>
/// البديل البسيط لـ FootprintSpawner: بدل الرسم في RenderTexture،
/// ينشئ نسخة من بريفاب الخطوة عند كل خطوة، يوجّهها مع المشي ويقلبها يمين/يسار.
/// ينحط على الوحش، والـ Animation Events تنادي SpawnFootprint("left"/"right").
///
/// مزايا: ما يحتاج Mesh Collider ولا أوفرلي مكرّر؛ يشتغل على الأرض والجدران تلقائياً
/// (الخطوة تتسطّح على السطح حسب hit.normal).
/// </summary>
public class FootprintDecalSpawner : MonoBehaviour
{
    [Header("عظام القدم")]
    public Transform leftFootBone;
    public Transform rightFootBone;

    [Header("الأثر")]
    [Tooltip("بريفاب الخطوة (Quad + FootprintDecal + مادة Custom/FootprintDecal).")]
    public GameObject footprintPrefab;
    [Tooltip("حجم الخطوة.")]
    public float footScale = 0.3f;
    [Tooltip("رفع بسيط فوق السطح لتجنّب التداخل البصري (z-fighting).")]
    public float surfaceOffset = 0.01f;
    [Tooltip("تصحيح دوران شكل القدم لو طلع منحرف (بالدرجات). جرّب 0 أو 180.")]
    public float angleOffset = 0f;
    [Tooltip("لو اليمين/اليسار مقلوبين.")]
    public bool swapLeftRight = false;

    [Header("كشف السطح")]
    public LayerMask surfaceMask = ~0;
    public float surfaceSearchDistance = 1f;

    [Header("الكشف بالفلاش (يُمرّر لكل خطوة)")]
    [Tooltip("لو مربوط: الخطوات تبان فقط داخل مخروط الفلاش. لو فاضي: تبان دايم.")]
    public PurpleFlashlight flashlight;

    [Header("تشخيص")]
    [Tooltip("يطبع رسائل في الـ Console تبيّن وش يصير عند كل خطوة.")]
    public bool debugLog = true;

    // زر اختبار: كليك يمين على المكوّن في الـ Inspector → Test Spawn (Left)
    // يطلع خطوة فوراً بدون الاعتماد على Animation Events — يعزل المشكلة.
    [ContextMenu("Test Spawn (Left)")]
    private void TestSpawnLeft() => SpawnFootprint("left");
    [ContextMenu("Test Spawn (Right)")]
    private void TestSpawnRight() => SpawnFootprint("right");

    /// <summary>تُستدعى من Animation Event بقيمة "left" أو "right".</summary>
    public void SpawnFootprint(string foot)
    {
        if (debugLog) Debug.Log($"[Footprint] SpawnFootprint('{foot}') اتنادى", this);

        Transform bone = (foot == "left") ? leftFootBone : rightFootBone;
        if (bone == null)
        {
            if (debugLog) Debug.LogWarning($"[Footprint] عظمة القدم '{foot}' مو مربوطة!", this);
            return;
        }
        if (footprintPrefab == null)
        {
            if (debugLog) Debug.LogWarning("[Footprint] Footprint Prefab مو مربوط!", this);
            return;
        }

        Vector3 down = -transform.up;
        Vector3 origin = bone.position - down * 0.2f;

        if (!Physics.Raycast(origin, down, out RaycastHit hit,
                            surfaceSearchDistance, surfaceMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLog) Debug.LogWarning($"[Footprint] الراي ما ضرب سطح. origin={origin}, مسافة={surfaceSearchDistance}. " +
                                           "كبّر Surface Search Distance أو تأكد الأرض داخل Surface Mask.", this);
            return;
        }

        if (debugLog) Debug.Log($"[Footprint] الراي ضرب '{hit.collider.name}' عند {hit.point}", this);

        // اتجاه المشي مسقط على السطح.
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        if (fwd.sqrMagnitude < 1e-6f) fwd = transform.forward;

        bool isProjector = footprintPrefab.GetComponent<DecalProjector>() != null;
        bool isLeft = (foot == "left") ^ swapLeftRight;

        // الدوران يختلف حسب النوع:
        //  - Decal Projector: يسقط على طول محور Z المحلي → نخلي Z داخل السطح، وY اتجاه المشي.
        //  - Quad: وجهه (Z) يطلع عمودي على السطح، وY اتجاه المشي.
        Quaternion rot = isProjector
            ? Quaternion.LookRotation(-hit.normal, fwd)
            : Quaternion.LookRotation(hit.normal, fwd);
        if (Mathf.Abs(angleOffset) > 0.01f)
            rot *= Quaternion.AngleAxis(angleOffset, Vector3.forward);

        Vector3 pos = hit.point + hit.normal * surfaceOffset;
        GameObject fp = Instantiate(footprintPrefab, pos, rot);
        fp.transform.localScale = Vector3.one * footScale;

        if (isProjector)
        {
            // قلب يمين/يسار عبر UV (السكيل السالب ما يضبط مع الديكال).
            DecalProjector dp = fp.GetComponent<DecalProjector>();
            if (isLeft)
            {
                dp.uvScale = new Vector2(-1f, 1f);
                dp.uvBias = new Vector2(1f, 0f);
            }
        }
        else if (isLeft)
        {
            // Quad: قلب عبر سكيل أفقي سالب.
            Vector3 s = fp.transform.localScale;
            s.x = -s.x;
            fp.transform.localScale = s;
        }

        // مرّر مرجع الفلاش للخطوة (لو فيه كشف بالفلاش).
        FootprintDecal decal = fp.GetComponent<FootprintDecal>();
        if (decal != null) decal.flashlight = flashlight;

        if (debugLog) Debug.Log($"[Footprint] طلعت خطوة عند {pos} (scale={footScale})", fp);
    }
}
