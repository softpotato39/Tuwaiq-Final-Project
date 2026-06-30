using UnityEngine;

/// <summary>
/// íÎİí ÇáæÍÔ ÈÔßá ßÇãá¡ æíÙåÑå İŞØ æŞÊ ãÇ ÇáİáÇÔ áÇíÊ íÖæí Úáíå ãÈÇÔÑÉ.
/// Öíİå Úáì äİÓ ÇáÃæÈÌßÊ ÇáÌĞÑ ááæÍÔ ÇáËÇäí (Ch45_nonPBR (1)).
/// </summary>
public class GhostVisibility : MonoBehaviour
{
    public PurpleFlashlight flashlight;
    public float detectionDistance = 18f;
    public float detectionAngle = 25f;

    private Renderer[] _renderers;
    private bool _isVisible;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);
    }

    private void Update()
    {
        bool shouldBeVisible = IsLitByFlashlight();

        if (shouldBeVisible != _isVisible)
        {
            SetVisible(shouldBeVisible);
        }
    }

    private bool IsLitByFlashlight()
    {
        if (flashlight == null || !flashlight.IsOn)
        {
            return false;
        }

        Transform lightTransform = flashlight.transform;
        Vector3 toGhost = transform.position - lightTransform.position;
        float distance = toGhost.magnitude;

        if (distance > detectionDistance)
        {
            return false;
        }

        float angle = Vector3.Angle(lightTransform.forward, toGhost);
        return angle <= detectionAngle;
    }

    private void SetVisible(bool visible)
    {
        _isVisible = visible;
        foreach (Renderer r in _renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}