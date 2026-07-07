using UnityEngine;

/// <summary>
/// يشغّل مشهد الجريمة لما اللاعب **يقرب من مكان الجريمة** (بمسافة، مو بالبصّ).
/// حطه على أوبجكت عند مكان الجريمة (القاتل/الضحية) — يفضّل نفس أوبجكت CrimeSceneReplay
/// بعد ما تنقله لمكان الجريمة.
///
/// وضعان:
///  - Auto Play On Approach = ✔ : يشتغل تلقائياً أول ما تقرب.
///  - أو = ❌ : تقرب ويطلع تلميح، وتضغط زر التفعيل لتشغيله.
/// مستقل تماماً — آمن للميرج.
/// </summary>
public class CrimeSceneActivator : MonoBehaviour
{
    [Header("المشهد")]
    [Tooltip("مشهد الجريمة. لو فاضي يلقى أقرب CrimeSceneReplay.")]
    public CrimeSceneReplay scene;
    public float autoFindRange = 12f;

    [Header("التفعيل بالقرب")]
    [Tooltip("اللاعب. فاضي = Camera.main.")]
    public Transform player;
    [Tooltip("مكان الجريمة اللي نحسب المسافة منه (مثلاً الضحية). فاضي = موقع هذا الأوبجكت.")]
    public Transform crimeLocation;
    [Tooltip("قد إيش لازم تقرب من مكان الجريمة عشان يتفعّل (متر).")]
    public float activateRange = 5f;
    [Tooltip("✔ = يشتغل تلقائياً أول ما تقرب. ❌ = تضغط زر التفعيل.")]
    public bool autoPlayOnApproach = false;
    [Tooltip("زر التفعيل (لو مو تلقائي).")]
    public KeyCode activateKey = KeyCode.E;
    [Tooltip("تلميح UI يظهر لما تكون قريب (اختياري).")]
    public GameObject promptIcon;
    [Tooltip("يشتغل مرة وحدة بس.")]
    public bool oneShot = true;

    private bool _used;

    private void Awake()
    {
        if (scene == null) scene = FindClosestScene();
        if (player == null && Camera.main != null) player = Camera.main.transform;
        if (promptIcon != null) promptIcon.SetActive(false);
    }

    private void Update()
    {
        if (scene == null || player == null) return;
        if (oneShot && _used) return;

        Vector3 crimePos = crimeLocation != null ? crimeLocation.position : transform.position;
        bool inRange = Vector3.Distance(player.position, crimePos) <= activateRange;

        if (promptIcon != null)
            promptIcon.SetActive(inRange && !autoPlayOnApproach);

        if (!inRange) return;

        if (autoPlayOnApproach || Input.GetKeyDown(activateKey))
        {
            scene.Play();
            _used = true;
            if (promptIcon != null) promptIcon.SetActive(false);
        }
    }

    private CrimeSceneReplay FindClosestScene()
    {
        CrimeSceneReplay[] all = FindObjectsByType<CrimeSceneReplay>(FindObjectsSortMode.None);
        CrimeSceneReplay best = null;
        float bestDist = autoFindRange;
        foreach (CrimeSceneReplay s in all)
        {
            if (s == null) continue;
            float d = Vector3.Distance(transform.position, s.transform.position);
            if (d <= bestDist) { bestDist = d; best = s; }
        }
        return best;
    }
}
