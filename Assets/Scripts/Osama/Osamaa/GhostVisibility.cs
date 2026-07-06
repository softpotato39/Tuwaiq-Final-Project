using UnityEngine;

/// <summary>
/// Hides the monster's renderers and shows them ONLY while it's lit by the
/// purple flashlight (inside the cone + range) AND the flashlight is ON.
///
/// Put this on the monster root (the parent of the mesh, e.g. Ch45_nonPBR).
///
/// The flashlight is spawned at runtime by the tool system (into the player's
/// hand), so it does NOT exist in the scene at edit time -> you can't assign it
/// in the Inspector. This script auto-finds it at runtime and keeps retrying
/// until the player equips it.
/// </summary>
public class GhostVisibility : MonoBehaviour
{
    [Tooltip("Leave empty: it's auto-found at runtime once the flashlight is equipped.")]
    public PurpleFlashlight flashlight;
    public float detectionDistance = 18f;
    public float detectionAngle = 25f;

    private Renderer[] _renderers;
    private bool _isVisible;
    private float _findTimer;

    private void Awake()
    {
        // Keep the animator running even while the monster is hidden, so
        // animation-driven logic keeps working off-screen: footstep Animation
        // Events fire and the foot bones keep moving for FootprintDecalSpawner.
        // Without this, a "Cull Update Transforms" animator freezes the bones
        // while hidden and footprints spawn at the wrong spot (or stop).
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        _renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);
    }

    private void Update()
    {
        // The flashlight is instantiated when the player equips the tool, so it
        // may not exist yet at Start. Keep looking until we find it.
        if (flashlight == null)
        {
            _findTimer -= Time.deltaTime;
            if (_findTimer <= 0f)
            {
                flashlight = FindFirstObjectByType<PurpleFlashlight>();
                _findTimer = 0.3f;
            }
        }

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
