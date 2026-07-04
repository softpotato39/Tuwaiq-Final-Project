using UnityEngine;

/// <summary>
/// يشغّل أصوات رعب عشوائية (تقطير ماء، صرير باب، همسة، طرق...) في أوقات
/// وأماكن عشوائية حوالين اللاعب — فيحسّ إن فيه شي معه في المكان.
///
/// حطه على أي أوبجكت (مدير الأصوات في الغرفة). اربط مجموعة مقاطع صوت.
/// مستقل تماماً — آمن للميرج.
/// </summary>
public class AmbientHorror : MonoBehaviour
{
    [Header("الأصوات")]
    [Tooltip("مقاطع الرعب — يختار وحد عشوائي كل مرة.")]
    public AudioClip[] clips;

    [Header("التوقيت")]
    [Tooltip("المدة العشوائية بين صوت وآخر (ثواني).")]
    public Vector2 intervalRange = new Vector2(8f, 22f);
    public Vector2 volumeRange = new Vector2(0.5f, 1f);

    [Header("المكان (صوت مجسّم 3D)")]
    [Tooltip("اللاعب — عشان الأصوات تطلع من حواليه. فاضي = Camera.main.")]
    public Transform player;
    [Tooltip("يطلّع الصوت من نقطة عشوائية حوالين اللاعب (يخليه يلتفت).")]
    public bool positionAroundPlayer = true;
    public float minDistance = 3f;
    public float maxDistance = 9f;

    private float _timer;

    private void Start()
    {
        if (player == null && Camera.main != null) player = Camera.main.transform;
        ResetTimer();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            PlayRandom();
            ResetTimer();
        }
    }

    private void PlayRandom()
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        float vol = Random.Range(volumeRange.x, volumeRange.y);

        if (positionAroundPlayer && player != null)
        {
            // نقطة عشوائية على دائرة حوالين اللاعب (مع ارتفاع بسيط عشوائي).
            Vector2 dir = Random.insideUnitCircle.normalized;
            float d = Random.Range(minDistance, maxDistance);
            Vector3 pos = player.position + new Vector3(dir.x * d, Random.Range(-0.5f, 1.5f), dir.y * d);
            AudioSource.PlayClipAtPoint(clip, pos, vol);
        }
        else
        {
            Vector3 pos = player != null ? player.position : transform.position;
            AudioSource.PlayClipAtPoint(clip, pos, vol);
        }
    }

    private void ResetTimer()
    {
        _timer = Random.Range(intervalRange.x, intervalRange.y);
    }
}
