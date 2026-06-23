using UnityEngine;
using UnityEngine.AI;

public class AnimeLookChase : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera targetCamera;

    [Header("Optional")]
    public bool requireLineOfSight = false; // إذا تبيه يتوقف فقط لما يكون ظاهر بدون جدار بينه وبين اللاعب

    private NavMeshAgent agent;
    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        if (player == null || targetCamera == null || agent == null)
            return;

        bool visible = IsAnyPartVisible();

        if (visible)
        {
            StopAgentImmediately();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void StopAgentImmediately()
    {
        if (!agent.isStopped)
            agent.isStopped = true;

        agent.ResetPath();
        agent.velocity = Vector3.zero;

        // هذا يساعد أكثر في منع الانزلاق الخفيف
        if (agent.hasPath)
            agent.ResetPath();
    }

    bool IsAnyPartVisible()
    {
        Bounds totalBounds;
        if (!TryGetTotalBounds(out totalBounds))
            return false;

        if (!IsBoundsInCameraView(targetCamera, totalBounds))
            return false;

        if (requireLineOfSight && !HasLineOfSightToBounds(targetCamera, totalBounds))
            return false;

        return true;
    }

    bool TryGetTotalBounds(out Bounds totalBounds)
    {
        bool hasBounds = false;
        totalBounds = new Bounds(transform.position, Vector3.zero);

        // نفضل الـ Renderers لأنها تمثل الشيء المرئي فعليًا
        if (renderers != null && renderers.Length > 0)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null || !r.enabled)
                    continue;

                if (!hasBounds)
                {
                    totalBounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    totalBounds.Encapsulate(r.bounds);
                }
            }
        }

        // إذا ما فيه Renderers نستخدم Colliders
        if (!hasBounds && colliders != null && colliders.Length > 0)
        {
            foreach (Collider c in colliders)
            {
                if (c == null || !c.enabled)
                    continue;

                if (!hasBounds)
                {
                    totalBounds = c.bounds;
                    hasBounds = true;
                }
                else
                {
                    totalBounds.Encapsulate(c.bounds);
                }
            }
        }

        return hasBounds;
    }

    bool IsBoundsInCameraView(Camera cam, Bounds bounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    bool HasLineOfSightToBounds(Camera cam, Bounds bounds)
    {
        Vector3 camPos = cam.transform.position;

        Vector3[] points = new Vector3[]
        {
            bounds.center,
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)
        };

        foreach (Vector3 point in points)
        {
            Vector3 dir = point - camPos;
            float dist = dir.magnitude;

            if (!Physics.Raycast(camPos, dir.normalized, out RaycastHit hit, dist))
                return true;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;
        }

        return false;
    }
}
