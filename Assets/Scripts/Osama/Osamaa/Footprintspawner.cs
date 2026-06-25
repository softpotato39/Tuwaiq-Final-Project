using UnityEngine;

public class FootprintSpawner : MonoBehaviour
{
    public Transform leftFootBone;
    public Transform rightFootBone;
    public LayerMask surfaceMask = ~0;
    public float surfaceSearchDistance = 1f;

    public void SpawnFootprint(string foot)
    {
        Transform targetBone = foot == "left" ? leftFootBone : rightFootBone;
        if (targetBone == null)
        {
            Debug.LogWarning($"FootprintSpawner: عظمة القدم '{foot}' غير محددة.");
            return;
        }

        Vector3 surfaceDown = -transform.up;
        Vector3 rayOrigin = targetBone.position - surfaceDown * 0.2f;

        if (Physics.Raycast(rayOrigin, surfaceDown, out RaycastHit hit, surfaceSearchDistance, surfaceMask))
        {
            SurfaceTrailPainter painter = hit.collider.GetComponent<SurfaceTrailPainter>();
            if (painter != null)
            {
                painter.PaintAt(hit.textureCoord);
            }
        }
    }
}