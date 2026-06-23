using UnityEngine;
using UnityEngine.AI;

public class AnimeLookChase : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;
    private Camera cam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
    }

    void Update()
    {
        if (CanPlayerSeeMe())
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // 👈 هذا اللي يخليه يتجمد فورًا
        }
    }

    bool CanPlayerSeeMe()
    {
        Vector3 dirToEnemy = (transform.position - cam.transform.position);
        float angle = Vector3.Angle(cam.transform.forward, dirToEnemy);

        if (angle < 60f)
        {
            Ray ray = new Ray(cam.transform.position, dirToEnemy.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.transform == transform;
            }
        }

        return false;
    }
}