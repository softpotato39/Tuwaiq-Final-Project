using UnityEngine;

/// <summary>
/// يحدّد نوع السطح لنظام صوت الخطوات. حطه على أي أرضية/سطح
/// وغيّر Surface لاسم النوع (Metal, Tile, Wood, Concrete...).
/// FootstepSounds يقرأه لما يمشي اللاعب فوقه ويشغّل الصوت المناسب.
/// </summary>
public class SurfaceTag : MonoBehaviour
{
    [Tooltip("اسم نوع السطح: Metal, Tile, Wood... (يطابق اللي في FootstepSounds).")]
    public string surface = "Default";
}
