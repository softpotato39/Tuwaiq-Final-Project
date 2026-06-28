using UnityEngine;

public class FootprintSpawner : MonoBehaviour
{
    public Transform leftFootBone;
    public Transform rightFootBone;
    public LayerMask surfaceMask = ~0;
    public float surfaceSearchDistance = 1f;

    public void SpawnFootprint(string foot)
    {
        Debug.Log($"[Footprint] SpawnFootprint استُدعيت — القدم: {foot}");

        Transform targetBone = foot == "left" ? leftFootBone : rightFootBone;
        if (targetBone == null)
        {
            Debug.LogWarning($"FootprintSpawner: عظمة القدم '{foot}' غير محددة.");
            return;
        }

        Vector3 surfaceDown = -transform.up;
        Vector3 rayOrigin = targetBone.position - surfaceDown * 0.2f;

        Debug.DrawRay(rayOrigin, surfaceDown * surfaceSearchDistance, Color.red, 2f);

        if (Physics.Raycast(rayOrigin, surfaceDown, out RaycastHit hit, surfaceSearchDistance, surfaceMask))
        {
            Debug.Log($"[Footprint] أصاب السطح: {hit.collider.gameObject.name} عند UV: {hit.textureCoord}");

            SurfaceTrailPainter painter = hit.collider.GetComponent<SurfaceTrailPainter>();
            if (painter != null)
            {
                painter.PaintAt(hit.textureCoord);
                Debug.Log("[Footprint] PaintAt استُدعيت بنجاح");
            }
            else
            {
                Debug.LogWarning($"[Footprint] السطح {hit.collider.gameObject.name} ما عليه SurfaceTrailPainter!");
            }
        }
        else
        {
            Debug.LogWarning("[Footprint] الـRaycast ما أصاب أي سطح!");
        }
    }
}