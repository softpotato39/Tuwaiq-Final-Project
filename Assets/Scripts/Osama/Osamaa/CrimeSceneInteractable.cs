using UnityEngine;
using InteractionSystem; // واجهة IInteractable من نظام Hanof

/// <summary>
/// غرض تفاعلي (راديو، جهاز...) يشغّل مشهد الجريمة لما اللاعب **يقرب منه ويتفاعل**.
/// يعتمد على نظام تفاعل Hanof (PlayerInteractor) — فيشتغل فقط لما تكون قريب
/// وتبصّ عليه وتضغط زر Interact.
///
/// أبسط من نظام الأدوات المحمولة: ما فيه التقاط ولا حمل — تقرب وتتفاعل وخلاص.
/// حطه على غرض فيه Collider و Layer = Interactable.
/// </summary>
public class CrimeSceneInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("مشهد الجريمة اللي يشتغل. لو فاضي، يلقى أقرب CrimeSceneReplay حوله.")]
    public CrimeSceneReplay scene;
    [Tooltip("مدى البحث عن أقرب مشهد لو ما ربطت واحد يدوياً.")]
    public float autoFindRange = 8f;

    [Tooltip("أيقونة/تلميح UI يظهر لما تبصّ عليه (اختياري).")]
    public GameObject promptIcon;

    [Tooltip("يشتغل مرة وحدة بس.")]
    public bool oneShot = false;

    private bool _used;

    private void Awake()
    {
        if (scene == null) scene = FindClosestScene();
        if (promptIcon != null) promptIcon.SetActive(false);
    }

    public void ShowPrompt()
    {
        if (promptIcon != null) promptIcon.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptIcon != null) promptIcon.SetActive(false);
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        return scene != null && !(oneShot && _used);
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (scene == null) return;
        scene.Play();
        _used = true;
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
