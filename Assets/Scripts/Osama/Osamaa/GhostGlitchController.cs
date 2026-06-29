using UnityEngine;
using System.Collections;

/// <summary>
/// Ì Õﬂ„ »‘œ…  √ÀÌ— «·€·«Ì ‘ (_GlitchIntensity) ›Ì ‘Ìœ— GhostGlitch ⁄»— MaterialPropertyBlock
/// »œÊ‰ ≈‰‘«¡ ‰”Œ ÃœÌœ… „‰ «·„« Ì—Ì«·.
///
/// ÿ—Ìﬁ… «·«” Œœ«„:
/// 1. ÷Ì›Â ⁄·Ï ‰›” «·√Ê»Ãﬂ  «··Ì ⁄·ÌÂ «·‘Ìœ— = Custom/GhostGlitch.
/// 2. «·ﬁÌ„  ‰ÿ»ﬁ ⁄·Ï ﬂ· «·‹ Renderers  Õ  Â–« «·√Ê»Ãﬂ .
/// 3. ”ﬂ—» «  √Œ—Ï („À· GhostBehavior)  ﬁœ—  ” œ⁄Ì TriggerBurst() Êﬁ  —œ ›⁄· „›«Ã∆.
/// </summary>
public class GhostGlitchController : MonoBehaviour
{
    [Header("‘œ… «·€·«Ì ‘")]
    [Tooltip("‘œ… «·€·«Ì ‘ Êﬁ  „« «·ÊÕ‘ Â«œ∆")]
    [Range(0f, 1f)] public float idleIntensity = 0.15f;

    [Tooltip("‘œ… «·€·«Ì ‘ Êﬁ  —œ ›⁄· „›«Ã∆ ( Ì·Ì»Ê—  / ›·«‘ ·«Ì )")]
    [Range(0f, 1f)] public float burstIntensity = 1f;

    [Tooltip("ﬂ„ À«‰Ì…  ” „— ‘œ… «·‹ Burst ﬁ»· „«  —Ã⁄ ··Ê÷⁄ «·Â«œ∆")]
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