using UnityEngine;
using InteractionSystem; // واجهة IUsableTool من نظام Hanof

/// <summary>
/// أداة الفلاش البنفسجي — تربط الفلاش الموجود (PurpleFlashlight) بنظام الأدوات حق Hanof.
/// تنحط على بريفاب الفلاش (نفس الأوبجكت اللي عليه PurpleFlashlight أو أب له).
/// لما اللاعب يستخدم الأداة، النظام ينادي Use() → تشغّل/تطفّي الفلاش،
/// والكتابات/البصمات اللي عليها RevealUnderFlashlight تبان داخل ضوءه.
///
/// ما يلمس أي ملف حق Hanof — بس ينفّذ واجهتهم IUsableTool.
/// </summary>
public class PurpleFlashlightTool : MonoBehaviour, IUsableTool
{
    [Tooltip("مكوّن PurpleFlashlight. لو فاضي يلقاه على نفس الأوبجكت أو الأبناء.")]
    [SerializeField] private PurpleFlashlight flashlight;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponentInChildren<PurpleFlashlight>(true);
    }

    // يناديها نظام الأدوات لما اللاعب "يستخدم" الأداة.
    public void Use()
    {
        if (flashlight != null)
            flashlight.ToggleFlashlight();
    }
}
