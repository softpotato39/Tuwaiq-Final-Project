using UnityEngine;
using InteractionSystem; // واجهة IUsableTool من نظام Hanof

/// <summary>
/// أداة "رؤية الجريمة" — تُلبس بيد اللاعب عبر نظام الأدوات حق Hanof.
/// عند الاستخدام (زر Use) تشغّل **أقرب مشهد جريمة** (CrimeSceneReplay) حوالين اللاعب.
///
/// تنحط على **بريفاب الأداة** (اللي يشير له ToolDefinition → Tool Prefab).
/// ما تلمس أي ملف حق Hanof — بس تنفّذ واجهتهم IUsableTool.
/// </summary>
public class CrimeVisionTool : MonoBehaviour, IUsableTool
{
    [Tooltip("أقصى مسافة لتشغيل مشهد جريمة قريب (متر).")]
    public float range = 6f;

    [Tooltip("صوت تشغيل الأداة (اختياري).")]
    public AudioSource activateSound;

    // يناديها ToolController لما اللاعب يضغط زر الاستخدام.
    public void Use()
    {
        CrimeSceneReplay scene = FindClosestScene();
        if (scene == null) return;

        if (activateSound != null) activateSound.Play();
        scene.Play();
    }

    private CrimeSceneReplay FindClosestScene()
    {
        CrimeSceneReplay[] scenes = FindObjectsByType<CrimeSceneReplay>(FindObjectsSortMode.None);
        CrimeSceneReplay best = null;
        float bestDist = range;

        foreach (CrimeSceneReplay s in scenes)
        {
            if (s == null) continue;
            float d = Vector3.Distance(transform.position, s.transform.position);
            if (d <= bestDist)
            {
                bestDist = d;
                best = s;
            }
        }
        return best;
    }
}
