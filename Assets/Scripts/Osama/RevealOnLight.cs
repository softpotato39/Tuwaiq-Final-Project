using UnityEngine;

/// <summary>
/// ÓßÑÈÊ íÎİí ÇáÃæÈÌßÊ ÈÔßá ØÈíÚí¡ æíÙåÑå İŞØ áãÇ íŞÚ Úáíå ÖæÁ ÇáİáÇÔ áÇíÊ ÇáÈäİÓÌí.
/// ãËÇáí áÃÔíÇÁ ãÎİíÉ (ÃÔÈÇÍ¡ ÑÓÇÆá ÓÑíÉ¡ ÃÚÏÇÁ ãÎİííä...).
///
/// ØÑíŞÉ ÇáÅÚÏÇÏ:
/// 1. Öíİ åĞÇ ÇáÓßÑÈÊ Úáì ÇáÃæÈÌßÊ Çááí ÊÈí ÊÎİíå (áÇÒã íÍÊæí MeshRenderer).
/// 2. İí ÇáÍŞá "flashlight" ÇÓÍÈ Úáíå ÇáÜ GameObject Çááí İíå ÓßÑÈÊ PurpleFlashlight.
/// 3. ÇÖÈØ "revealAngle" æ"revealDistance" ÍÓÈ ÍÌã ÇáÃæÈÌßÊ.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class RevealOnLight : MonoBehaviour
{
    [Header("ÑÈØ ÇáİáÇÔ áÇíÊ")]
    [Tooltip("ÇÓÍÈ Úáíå ÇáÃæÈÌßÊ Çááí İíå ÓßÑÈÊ PurpleFlashlight")]
    public PurpleFlashlight flashlight;

    [Header("ÔÑæØ ÇáÅÙåÇÑ")]
    [Tooltip("ÃŞÕì ãÓÇİÉ Èíä ÇáİáÇÔ áÇíÊ æÇáÃæÈÌßÊ ÚÔÇä íäßÔİ")]
    public float revealDistance = 15f;

    [Tooltip("äÕ ÒÇæíÉ ãÎÑæØ ÇáßÔİ (áÇÒã íßæä ŞÑíÈ ãä spotAngle ÇáİáÇÔ áÇíÊ)")]
    public float revealAngle = 25f;

    [Tooltip("áæ İÚøáÊåÇ¡ íÊÍŞŞ ÇáÓßÑÈÊ ÅĞÇ İí ÍÇÌÒ Èíä ÇáÖæÁ æÇáÃæÈÌßÊ (Raycast)")]
    public bool requireLineOfSight = true;

    [Tooltip("ÇáØÈŞÇÊ Çááí ÊÚÊÈÑ ÍæÇÌÒ ÊãäÚ ÇáßÔİ")]
    public LayerMask obstructionMask = ~0;

    [Header("ÊÃËíÑ ÇáÙåæÑ")]
    [Tooltip("ÓÑÚÉ ÇáÊáÇÔí ÈÇáÙåæÑ/ÇáÇÎÊİÇÁ (0 = ÙåæÑ İæÑí)")]
    public float fadeSpeed = 4f;

    private MeshRenderer _renderer;
    private Material _material;
    private float _currentAlpha;
    private bool _isRevealed;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _material = _renderer.material; // äÓÎÉ ÎÇÕÉ ÈåĞÇ ÇáÃæÈÌßÊ

        // äÈÏÃ ãÎİí ÊãÇãÇğ
        _currentAlpha = 0f;
        SetAlpha(_currentAlpha);
    }

    private void Update()
    {
        if (flashlight == null)
        {
            return;
        }

        _isRevealed = flashlight.IsOn && IsInsideLightCone();

        float targetAlpha = _isRevealed ? 1f : 0f;

        if (fadeSpeed <= 0f)
        {
            _currentAlpha = targetAlpha;
        }
        else
        {
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }

        SetAlpha(_currentAlpha);
        _renderer.enabled = _currentAlpha > 0.01f;
    }

    private bool IsInsideLightCone()
    {
        Transform lightTransform = flashlight.transform;

        Vector3 toObject = transform.position - lightTransform.position;
        float distance = toObject.magnitude;

        if (distance > revealDistance)
        {
            return false;
        }

        float angle = Vector3.Angle(lightTransform.forward, toObject);
        if (angle > revealAngle)
        {
            return false;
        }

        if (requireLineOfSight)
        {
            if (Physics.Raycast(lightTransform.position, toObject.normalized, out RaycastHit hit, distance, obstructionMask))
            {
                // ÅĞÇ Ãæá Ôí íÕØÏã Èå ÇáÑÇí ãÔ äİÓ ÇáÃæÈÌßÊ¡ íÚäí İí ÍÇÌÒ íÍÌÈ ÇáÖæÁ
                if (hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SetAlpha(float alpha)
    {
        if (_material.HasProperty("_Color"))
        {
            Color c = _material.color;
            c.a = alpha;
            _material.color = c;
        }

        // áæ ÇáãÇÊíÑíÇá íÏÚã Transparent/Fade rendering mode İÚøáå ãä ÇáãÍÑÑ
        // Standard Shader: Rendering Mode = Fade or Transparent
    }
}
