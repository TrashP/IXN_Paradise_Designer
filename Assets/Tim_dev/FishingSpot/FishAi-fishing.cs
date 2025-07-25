using UnityEngine;
using UnityEngine.AI;

public class FishAI : MonoBehaviour
{
    public Transform centerPoint; // Activity center
    public float roamRadius = 5f;
    public float roamDelay = 3f;

    private float timer = 0f;
    private NavMeshAgent agent;

    void Start()
    {
        // Get NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("FishAI: NavMeshAgent component not found!", this);
            enabled = false; // Disable this script
            return;
        }

        // Set center point
        if (centerPoint == null)
        {
            centerPoint = this.transform;
            Debug.LogWarning("FishAI: centerPoint not set, using self position as center point", this);
        }

        // Validate parameters
        if (roamRadius <= 0f)
        {
            roamRadius = 5f;
            Debug.LogWarning("FishAI: roamRadius must be greater than 0, reset to default value 5", this);
        }

        if (roamDelay <= 0f)
        {
            roamDelay = 3f;
            Debug.LogWarning("FishAI: roamDelay must be greater than 0, reset to default value 3", this);
        }
    }

    void Update()
    {
        // Basic safety check
        if (agent == null || centerPoint == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= roamDelay && agent.remainingDistance < 0.5f)
        {
            Vector3 newDestination = GetRandomNavmeshPosition(centerPoint.position, roamRadius);
            if (newDestination != centerPoint.position) // Ensure a valid position is found
            {
                agent.SetDestination(newDestination);
                timer = 0f;
            }
        }
    }

    Vector3 GetRandomNavmeshPosition(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find a feasible point
        {
            Vector3 rand = Random.insideUnitSphere * radius;
            rand.y = 0;
            Vector3 tryPos = center + rand;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(tryPos, out hit, 1f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // If no random position is found, return the center point
        Debug.LogWarning("FishAI: Unable to find a valid navigation mesh position, returning center point", this);
        return center;
    }
}
