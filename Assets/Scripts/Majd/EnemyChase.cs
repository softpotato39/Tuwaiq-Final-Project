using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    [Header("Patrol")]
    public Transform[] points;
    private int currentPoint = 0;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

void Update()
{
    Debug.Log(CanSeePlayer());

    if (CanSeePlayer())
    {
        agent.SetDestination(player.position);
    }
    else
    {
        Patrol();
    }
}
    bool CanSeePlayer()
{
    if (player == null) return false;

    Vector3 dir = player.position - transform.position;
    float distance = dir.magnitude;

    if (distance > viewDistance)
        return false;

    Ray ray = new Ray(transform.position + Vector3.up, dir.normalized);

    if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
    {
        if (hit.transform != player)
            return false;
    }

    return true;
}

    void Patrol()
    {
        if (points.Length == 0) return;

        agent.SetDestination(points[currentPoint].position);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPoint++;
            if (currentPoint >= points.Length)
                currentPoint = 0;
        }
    }
}