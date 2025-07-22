using UnityEngine;
using UnityEngine.AI;

public class FishAI : MonoBehaviour
{
    public Transform centerPoint; // 活动中心
    public float roamRadius = 5f;
    public float roamDelay = 3f;

    private float timer = 0f;
    private NavMeshAgent agent;

    void Start()
    {
        // 获取NavMeshAgent组件
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("FishAI: 未找到NavMeshAgent组件！", this);
            enabled = false; // 禁用此脚本
            return;
        }

        // 设置中心点
        if (centerPoint == null)
        {
            centerPoint = this.transform;
            Debug.LogWarning("FishAI: 未设置centerPoint，使用自身位置作为中心点", this);
        }

        // 验证参数
        if (roamRadius <= 0f)
        {
            roamRadius = 5f;
            Debug.LogWarning("FishAI: roamRadius必须大于0，已重置为默认值5", this);
        }

        if (roamDelay <= 0f)
        {
            roamDelay = 3f;
            Debug.LogWarning("FishAI: roamDelay必须大于0，已重置为默认值3", this);
        }
    }

    void Update()
    {
        // 基本安全检查
        if (agent == null || centerPoint == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= roamDelay && agent.remainingDistance < 0.5f)
        {
            Vector3 newDestination = GetRandomNavmeshPosition(centerPoint.position, roamRadius);
            if (newDestination != centerPoint.position) // 确保找到了有效位置
            {
                agent.SetDestination(newDestination);
                timer = 0f;
            }
        }
    }

    Vector3 GetRandomNavmeshPosition(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++) // 尝试10次找可行点
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

        // 如果找不到随机位置，返回中心点
        Debug.LogWarning("FishAI: 无法找到有效的导航网格位置，返回中心点", this);
        return center;
    }
}
