using UnityEngine;

public class GlitchManager : MonoBehaviour
{
    [Header("References")]
    public Material ghostMaterial;
    public Transform flashlightTransform;
    public Light flashlight;

    [Header("Detection")]
    public float detectionAngle = 25f;
    public float detectionRange = 10f;

    private static readonly int LitByFlashlightID = Shader.PropertyToID("_LitByFlashlight");

    void Update()
    {
        bool hit = IsFlashlightHitting();
        ghostMaterial.SetFloat(LitByFlashlightID, hit ? 1f : 0f);
    }

    bool IsFlashlightHitting()
    {
        if (flashlight == null || !flashlight.enabled) return false;

        Vector3 toGhost = transform.position - flashlightTransform.position;
        float distance = toGhost.magnitude;
        float angle = Vector3.Angle(flashlightTransform.forward, toGhost);

        if (distance > detectionRange) return false;
        if (angle > detectionAngle) return false;

        // Raycast to make sure nothing blocks the light
        if (Physics.Raycast(flashlightTransform.position, toGhost.normalized, out RaycastHit hit, distance))
        {
            if (hit.collider.gameObject != gameObject) return false;
        }

        return true;
    }
}