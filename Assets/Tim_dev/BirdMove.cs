using UnityEngine;
using UnityEngine.AI;

public class BirdMove : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveRange = 10f;           // 移动范围
    [SerializeField] private float sampleRadius = 2f;         // 采样半径
    [SerializeField] private int maxSampleAttempts = 30;      // 最大采样尝试次数
    [SerializeField] private float minDistanceToTarget = 0.5f; // 到达目标的最小距离
    [SerializeField] private float idleTime = 2f;             // 到达目标后的等待时间
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = false;      // 是否显示调试信息
    
    private NavMeshAgent agent;
    private Vector3 currentTarget;
    private bool isMoving = false;
    private float idleTimer = 0f;
    
    void Start()
    {
        InitializeAgent();
    }
    
    void Update()
    {
        if (agent == null) return;
        
        // 检查是否到达目标
        if (isMoving && HasReachedTarget())
        {
            OnReachedTarget();
        }
        
        // 空闲时间处理
        if (!isMoving)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                MoveToRandomPosition();
            }
        }
    }
    
    private void InitializeAgent()
    {
        // 获取NavMeshAgent组件
        agent = GetComponent<NavMeshAgent>();
        
        // 检查组件是否存在
        if (agent == null)
        {
            Debug.LogError($"[BirdMove] {gameObject.name} 缺少 NavMeshAgent 组件！");
            enabled = false;
            return;
        }
        
        // 验证NavMesh是否存在
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"[BirdMove] {gameObject.name} 当前位置不在NavMesh上！");
        }
        
        // 设置初始目标
        MoveToRandomPosition();
    }
    
    private void MoveToRandomPosition()
    {
        if (agent == null) return;
        
        Vector3 targetPos = GetRandomNavMeshPoint(transform.position, moveRange);
        
        // 检查是否找到了有效目标
        if (targetPos != transform.position)
        {
            currentTarget = targetPos;
            agent.SetDestination(targetPos);
            isMoving = true;
            idleTimer = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BirdMove] {gameObject.name} 移动到: {targetPos}");
            }
        }
        else
        {
            Debug.LogWarning($"[BirdMove] {gameObject.name} 无法找到有效的移动目标");
        }
    }
    
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
    {
        // 参数验证
        if (range <= 0f)
        {
            Debug.LogWarning("[BirdMove] 移动范围必须大于0");
            return center;
        }
        
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            // 生成随机位置
            Vector3 randomPos = center + Random.insideUnitSphere * range;
            randomPos.y = center.y; // 保持Y轴不变，适合地面移动
            
            // 尝试在NavMesh上采样位置
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                // 检查目标是否可达
                if (NavMesh.CalculatePath(center, hit.position, NavMesh.AllAreas, new NavMeshPath()))
                {
                    return hit.position;
                }
            }
        }
        
        // 如果找不到有效位置，返回当前位置
        Debug.LogWarning($"[BirdMove] {gameObject.name} 在 {maxSampleAttempts} 次尝试后未找到有效目标");
        return center;
    }
    
    private bool HasReachedTarget()
    {
        if (agent == null) return false;
        
        // 检查是否到达目标
        return !agent.pathPending && 
               agent.remainingDistance <= minDistanceToTarget;
    }
    
    private void OnReachedTarget()
    {
        isMoving = false;
        idleTimer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[BirdMove] {gameObject.name} 到达目标: {currentTarget}");
        }
    }
    
    // 公共方法：强制移动到指定位置
    public void MoveToPosition(Vector3 position)
    {
        if (agent == null) return;
        
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            currentTarget = hit.position;
            agent.SetDestination(hit.position);
            isMoving = true;
            idleTimer = 0f;
        }
        else
        {
            Debug.LogWarning($"[BirdMove] {gameObject.name} 无法移动到指定位置: {position}");
        }
    }
    
    // 公共方法：停止移动
    public void StopMoving()
    {
        if (agent == null) return;
        
        agent.ResetPath();
        isMoving = false;
        idleTimer = 0f;
    }
    
    // 在Scene视图中绘制调试信息
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        // 绘制移动范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRange);
        
        // 绘制当前目标
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }
}
