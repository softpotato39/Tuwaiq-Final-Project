using UnityEngine;
using UnityEngine.Rendering.Universal;


public class FootprintSpawner : MonoBehaviour
{

    [Header("Prefab الأثر")]
    public GameObject footprintDecalPrefab;   // Prefab يحتوي على URP Decal Projector

    [Header("عظام الأقدام")]
    public Transform leftFootBone;            // اسحب عظمة القدم اليسرى من Hierarchy
    public Transform rightFootBone;           // اسحب عظمة القدم اليمنى من Hierarchy

    [Header("إعدادات الأثر")]
    public float footprintLifetime = 2f;      // كم ثانية قبل الحذف
    public float decalOffsetY = 0.02f;        // ارتفاع طفيف فوق الأرض لتجنب Z-fighting

    // ─────────────────────────────────────────
    // هذه الدالة تُستدعى من Animation Event
    // Parameter: "left" أو "right"
    // ─────────────────────────────────────────
    public void SpawnFootprint(string foot)
    {
        if (footprintDecalPrefab == null) return;

        // اختر العظمة الصحيحة
        Transform targetBone = foot == "left" ? leftFootBone : rightFootBone;
        if (targetBone == null)
        {
            Debug.LogWarning($"FootprintSpawner: عظمة القدم '{foot}' غير محددة في Inspector.");
            return;
        }

        // موضع الأثر: موضع العظمة + ارتفاع طفيف
        Vector3 spawnPos = targetBone.position;
        spawnPos.y += decalOffsetY;

        // الدوران: الأثر يواجه الأرض لأسفل (Decal Projector يحتاج محور Y لأسفل)
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.down, transform.forward);

        // إنشاء الأثر
        GameObject decal = Instantiate(footprintDecalPrefab, spawnPos, spawnRot);

        // احذفه بعد مدة محددة
        Destroy(decal, footprintLifetime);
    }
}
