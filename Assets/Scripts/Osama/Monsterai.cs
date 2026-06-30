using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    [Header("المراجع")]
    public Transform player;

    [Header("التجوال العشوائي")]
    public float wanderRadius = 20f;
    public float wanderIntervalMin = 3f;
    public float wanderIntervalMax = 7f;

    [Header("الهروب")]
    public float fleeRadius = 3f;
    public float fleeDistance = 15f;
    public float fleeSpeed = 6f;
    public float normalSpeed = 2.5f;

    [Header("الاختفاء وإعادة الظهور")]
    public float hideDelay = 2f;
    public float reappearDelay = 3f;
    public float teleportRadius = 30f;
    public float minTeleportDistance = 15f;

    [HideInInspector]
    public float distanceToPlayer = 999f;

    private NavMeshAgent agent;
    private enum State { Wandering, Fleeing, Hidden }
    private State currentState = State.Wandering;

    private float wanderTimer = 0f;
    private float nextWanderTime = 0f;
    private float fleeTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = false;
        foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>())
            r.enabled = false;

        agent.speed = normalSpeed;
        nextWanderTime = Random.Range(wanderIntervalMin, wanderIntervalMax);
    }

    void Update()
    {
        if (player == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Wandering: UpdateWandering(); break;
            case State.Fleeing: UpdateFleeing(); break;
            case State.Hidden: break;
        }
    }

    void UpdateWandering()
    {
        if (distanceToPlayer <= fleeRadius)
        {
            currentState = State.Fleeing;
            agent.speed = fleeSpeed;
            fleeTimer = 0f;
            SetFleeDestination();
            return;
        }

        wanderTimer += Time.deltaTime;
        if (wanderTimer >= nextWanderTime)
        {
            wanderTimer = 0f;
            nextWanderTime = Random.Range(wanderIntervalMin, wanderIntervalMax);
            SetRandomWanderDestination();
        }
    }

    void UpdateFleeing()
    {
        if (distanceToPlayer <= fleeRadius)
        {
            SetFleeDestination();
            fleeTimer = 0f;
            return;
        }

        fleeTimer += Time.deltaTime;
        if (fleeTimer >= hideDelay)
        {
            StartCoroutine(HideAndReappear());
        }
    }

    System.Collections.IEnumerator HideAndReappear()
    {
        currentState = State.Hidden;
        agent.isStopped = true;
        agent.enabled = false;

        yield return new WaitForSeconds(reappearDelay);

        Vector3 newPosition = FindSafeTeleportPoint();
        transform.position = newPosition;

        agent.enabled = true;
        agent.speed = normalSpeed;
        agent.isStopped = false;

        wanderTimer = 0f;
        nextWanderTime = Random.Range(wanderIntervalMin, wanderIntervalMax);
        currentState = State.Wandering;
        SetRandomWanderDestination();
    }

    void SetRandomWanderDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        randomPoint.y = transform.position.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void SetFleeDestination()
    {
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionAwayFromPlayer * fleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    Vector3 FindSafeTeleportPoint()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 candidate = player.position + Random.insideUnitSphere * teleportRadius;
            candidate.y = player.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, teleportRadius, NavMesh.AllAreas))
            {
                float distFromPlayer = Vector3.Distance(hit.position, player.position);
                if (distFromPlayer >= minTeleportDistance)
                    return hit.position;
            }
        }

        return transform.position + Random.insideUnitSphere * teleportRadius;
    }
}