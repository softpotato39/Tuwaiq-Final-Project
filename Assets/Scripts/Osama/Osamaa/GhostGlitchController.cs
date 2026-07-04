using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the intensity of the glitch effect (_GlitchIntensity) on the GhostGlitch shader
/// via MaterialPropertyBlock, without creating new material instances.
///
/// Usage:
/// 1. Attach this to the same object that has the Custom/GhostGlitch shader.
/// 2. The values apply to all Renderers under this object.
/// 3. Other scripts (e.g. GhostBehavior) can call TriggerBurst() for a sudden reaction.
/// </summary>
public class GhostGlitchController : MonoBehaviour
{
    [Header("Glitch Intensity")]
    [Tooltip("Glitch intensity while the ghost is idle/calm")]
    [Range(0f, 1f)] public float idleIntensity = 0.15f;

    [Tooltip("Glitch intensity during a sudden reaction (teleport / flashlight)")]
    [Range(0f, 1f)] public float burstIntensity = 1f;

    [Tooltip("How many seconds the burst intensity lasts before returning to idle")]
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