using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// íÊÍßã ÈÔÏÉ ÇáÛáÇíÊÔ (_GlitchIntensity) İí ÔíÏÑ GhostGlitch áßá ÇáÑäÏÑÑÇÊ
/// ÊÍÊ åĞÇ ÇáÃæÈÌßÊ (ãËá Ch45_nonPBR æßá ÃÌÒÇÁ ÇáÌÓã).
///
/// ØÑíŞÉ ÇáÅÚÏÇÏ:
/// 1. ÃäÔÆ ãÇÊíÑíÇá ÌÏíÏ æÇÎÊÇÑ áå Shader = Custom/GhostGlitch.
/// 2. ØÈøŞ ÇáãÇÊíÑíÇá Úáì ßá ŞØÚ ÇáãíÔ ÈÊÇÚÉ ÇáæÍÔ (Ch45_Body ãËáÇğ).
/// 3. Öíİ åĞÇ ÇáÓßÑÈÊ Úáì ÇáÃæÈÌßÊ ÇáÌĞÑ ááæÍÔ (äİÓ Çááí İíå Animator).
/// 4. ÓßÑÈÊÇÊ ÇáÓáæß (GhostBehavior) ÊŞÏÑ ÊäÇÏí TriggerBurst() ÚÔÇä áÍÙÉ ÛáÇíÊÔ ŞæíÉ.
/// </summary>
public class GhostGlitchController : MonoBehaviour
{
    [Header("ÇáÔÏÉ ÇáÇİÊÑÇÖíÉ")]
    [Tooltip("ÔÏÉ ÇáÛáÇíÊÔ ÇáØÈíÚíÉ æŞÊ ãÇ ÇáæÍÔ åÇÏÆ (ÊãÔíÉ ÚÇÏíÉ)")]
    [Range(0f, 1f)] public float idleIntensity = 0.15f;

    [Tooltip("ÔÏÉ ÇáÛáÇíÊÔ æŞÊ äæÈÉ ŞæíÉ (ÊíáíÈæÑÊ / ÑÏ İÚá Úáì ÇáÖæÁ)")]
    [Range(0f, 1f)] public float burstIntensity = 1f;

    [Tooltip("ãÏÉ äæÈÉ ÇáÛáÇíÊÔ ÇáŞæíÉ ÈÇáËæÇäí")]
    public float burstDuration = 0.6f;

    private static readonly int GlitchIntensityID = Shader.PropertyToID("_GlitchIntensity");

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;
    private float _currentIntensity;
    private Coroutine _burstRoutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();
        _currentIntensity = idleIntensity;
    }

    private void Start()
    {
        ApplyIntensity(_currentIntensity);
    }

    /// <summary>
    /// íØáŞ äæÈÉ ÛáÇíÊÔ ŞæíÉ áİÊÑÉ ŞÕíÑÉ Ëã íÑÌÚ ááæÖÚ ÇáØÈíÚí.
    /// </summary>
    public void TriggerBurst()
    {
        if (_burstRoutine != null)
        {
            StopCoroutine(_burstRoutine);
        }
        _burstRoutine = StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        ApplyIntensity(burstIntensity);
        yield return new WaitForSeconds(burstDuration);
        ApplyIntensity(idleIntensity);
        _burstRoutine = null;
    }

    /// <summary>
    /// ÊÍÏíÏ ÔÏÉ ÇáÛáÇíÊÔ íÏæíÇğ (ÊÓÊÎÏãåÇ ÓßÑÈÊÇÊ ÃÎÑì ÚäÏ ÇáÍÇÌÉ).
    /// </summary>
    public void SetIntensity(float value)
    {
        ApplyIntensity(Mathf.Clamp01(value));
    }

    private void ApplyIntensity(float value)
    {
        _currentIntensity = value;

        foreach (Renderer r in _renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(GlitchIntensityID, value);
            r.SetPropertyBlock(_propBlock);
        }
    }
}
