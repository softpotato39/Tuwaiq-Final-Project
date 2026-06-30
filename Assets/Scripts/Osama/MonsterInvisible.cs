using UnityEngine;

/// <summary>
/// يخفي جسم الوحش بالكامل فيبقى أثر الأقدام فقط — مع إبقاء الأنميشن شغّال
/// عشان الـ Animation Events (الخطوات) تستمر وهو مخفي.
///
/// حطه على جذر الوحش (Ch45_nonPBR). يغنيك عن:
///   - شيل الصح يدوياً من Skinned Mesh Renderer.
///   - تغيير Animator Culling Mode يدوياً.
/// (يسويهم بالكود فينتقل معك لأي مشروع بدون إعداد).
/// </summary>
[DisallowMultipleComponent]
public class MonsterInvisible : MonoBehaviour
{
    [Tooltip("يخفي كل الـ Renderers تحت الوحش عند بداية اللعب.")]
    public bool hideOnStart = true;

    private void Awake()
    {
        // ضروري: بدون هذا، يوقف الأنميشن لما الميش يكون خارج كاميرا/مخفي
        // فتوقف الخطوات. AlwaysAnimate يخلي الخطوات تستمر وهو مخفي.
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (hideOnStart)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
                r.enabled = false;
        }
    }
}
