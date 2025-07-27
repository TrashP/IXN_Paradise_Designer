using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 鸟类移动控制器 - 处理鸟类的自动移动和飞行行为
/// Bird Movement Controller - Handles bird's automatic movement and flying behavior
/// </summary>
public class BirdMove : MonoBehaviour
{
    [Header("移动设置")]
    [Header("Movement Settings")]
    [SerializeField] private float moveRange = 10f;           // 移动范围 Movement Range
    [SerializeField] private float sampleRadius = 2f;         // 采样半径 Sample Radius
    [SerializeField] private int maxSampleAttempts = 30;      // 最大采样尝试次数 Max Sample Attempts
    [SerializeField] private float minDistanceToTarget = 0.5f; // 到达目标的最小距离 Min Distance to Target
    [SerializeField] private float idleTime = 2f;             // 到达目标后的等待时间 Idle Time After Reaching Target
    
    [Header("飞行设置")]
    [Header("Flight Settings")]
    [SerializeField] private bool enableFlying = true;        // 是否启用飞行 Enable Flying
    [SerializeField] private float minFlightHeight = 5f;      // 最小飞行高度 Min Flight Height
    [SerializeField] private float maxFlightHeight = 10f;     // 最大飞行高度 Max Flight Height
    [SerializeField] private float flightSpeed = 3f;          // 飞行速度 Flight Speed
    [SerializeField] private float heightChangeSpeed = 2f;    // 高度变化速度 Height Change Speed
    
    [Header("调试设置")]
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = false;      // 是否显示调试信息 Show Debug Info
    
    // 私有变量
    // Private Variables
    private NavMeshAgent agent;
    private Vector3 currentTarget;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private float targetHeight;                               // 目标飞行高度 Target Flight Height
    
    void Start()
    {
        InitializeAgent();
    }
    
    void Update()
    {
        if (agent == null) return;
        
        if (enableFlying)
        {
            HandleFlyingMovement();
        }
        else
        {
            HandleGroundMovement();
        }
    }
    
    private void HandleFlyingMovement()
    {
        // 检查是否到达目标
        // Check if reached target
        if (isMoving && HasReachedTarget())
        {
            OnReachedTarget();
        }
        
        // 空闲时间处理
        // Handle idle time
        if (!isMoving)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                MoveToRandomPosition();
            }
        }
        
        // 平滑移动到目标位置
        // Smoothly move to target position
        if (isMoving)
        {
            Vector3 direction = (currentTarget - transform.position).normalized;
            transform.position += direction * flightSpeed * Time.deltaTime;
            
            // 平滑高度变化
            // Smooth height change
            if (Mathf.Abs(transform.position.y - targetHeight) > 0.1f)
            {
                float newY = Mathf.MoveTowards(transform.position.y, targetHeight, heightChangeSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            // 平滑朝向目标
            // Smoothly face target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
            }
        }
    }
    
    private void HandleGroundMovement()
    {
        // 检查是否到达目标
        // Check if reached target
        if (isMoving && HasReachedTarget())
        {
            OnReachedTarget();
        }
        
        // 空闲时间处理
        // Handle idle time
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
        // Get NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        
        // 检查组件是否存在
        // Check if component exists
        if (agent == null)
        {
            Debug.LogError($"[BirdMove] {gameObject.name} 缺少 NavMeshAgent 组件！");
            Debug.LogError($"[BirdMove] {gameObject.name} Missing NavMeshAgent component!");
            enabled = false;
            return;
        }
        
        // 如果启用飞行，设置NavMeshAgent为飞行模式
        // If flying is enabled, set NavMeshAgent to flight mode
        if (enableFlying)
        {
            agent.enabled = false; // 禁用NavMeshAgent，使用自定义飞行逻辑
            // Disable NavMeshAgent, use custom flight logic
            targetHeight = transform.position.y;
        }
        else
        {
            // 验证NavMesh是否存在
            // Validate if NavMesh exists
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"[BirdMove] {gameObject.name} 当前位置不在NavMesh上！");
                Debug.LogWarning($"[BirdMove] {gameObject.name} Current position not on NavMesh!");
            }
        }
        
        // 设置初始目标
        // Set initial target
        MoveToRandomPosition();
    }
    
    private void MoveToRandomPosition()
    {
        if (enableFlying)
        {
            MoveToRandomFlyingPosition();
        }
        else
        {
            MoveToRandomGroundPosition();
        }
    }
    
    private void MoveToRandomFlyingPosition()
    {
        Vector3 targetPos = GetRandomFlyingPosition(transform.position, moveRange);
        
        // 检查是否找到了有效目标
        // Check if valid target was found
        if (targetPos != transform.position)
        {
            currentTarget = targetPos;
            targetHeight = targetPos.y;
            isMoving = true;
            idleTimer = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BirdMove] {gameObject.name} 飞行到: {targetPos}");
                Debug.Log($"[BirdMove] {gameObject.name} Flying to: {targetPos}");
            }
        }
        else
        {
            Debug.LogWarning($"[BirdMove] {gameObject.name} 无法找到有效的飞行目标");
            Debug.LogWarning($"[BirdMove] {gameObject.name} Cannot find valid flight target");
        }
    }
    
    private void MoveToRandomGroundPosition()
    {
        Vector3 targetPos = GetRandomNavMeshPoint(transform.position, moveRange);
        
        // 检查是否找到了有效目标
        // Check if valid target was found
        if (targetPos != transform.position)
        {
            currentTarget = targetPos;
            agent.SetDestination(targetPos);
            isMoving = true;
            idleTimer = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log($"[BirdMove] {gameObject.name} 移动到: {targetPos}");
                Debug.Log($"[BirdMove] {gameObject.name} Moving to: {targetPos}");
            }
        }
        else
        {
            Debug.LogWarning($"[BirdMove] {gameObject.name} 无法找到有效的移动目标");
            Debug.LogWarning($"[BirdMove] {gameObject.name} Cannot find valid movement target");
        }
    }
    
    private Vector3 GetRandomFlyingPosition(Vector3 center, float range)
    {
        // 参数验证
        // Parameter validation
        if (range <= 0f)
        {
            Debug.LogWarning("[BirdMove] 移动范围必须大于0");
            Debug.LogWarning("[BirdMove] Movement range must be greater than 0");
            return center;
        }
        
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            // 生成随机位置（包括高度）
            // Generate random position (including height)
            Vector3 randomPos = center + Random.insideUnitSphere * range;
            
            // 限制高度范围
            // Limit height range
            randomPos.y = Mathf.Clamp(randomPos.y, minFlightHeight, maxFlightHeight);
            
            // 检查位置是否在合理范围内
            // Check if position is within reasonable range
            float distance = Vector3.Distance(center, randomPos);
            if (distance <= range && distance > 1f) // 确保不是原地
            // Ensure not staying in place
            {
                return randomPos;
            }
        }
        
        // 如果找不到有效位置，返回当前位置
        // If no valid position found, return current position
        Debug.LogWarning($"[BirdMove] {gameObject.name} 在 {maxSampleAttempts} 次尝试后未找到有效飞行目标");
        Debug.LogWarning($"[BirdMove] {gameObject.name} No valid flight target found after {maxSampleAttempts} attempts");
        return center;
    }
    
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float range)
    {
        // 参数验证
        // Parameter validation
        if (range <= 0f)
        {
            Debug.LogWarning("[BirdMove] 移动范围必须大于0");
            Debug.LogWarning("[BirdMove] Movement range must be greater than 0");
            return center;
        }
        
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            // 生成随机位置
            // Generate random position
            Vector3 randomPos = center + Random.insideUnitSphere * range;
            randomPos.y = center.y; // 保持Y轴不变，适合地面移动
            // Keep Y-axis unchanged, suitable for ground movement
            
            // 尝试在NavMesh上采样位置
            // Try to sample position on NavMesh
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                // 检查目标是否可达
                // Check if target is reachable
                if (NavMesh.CalculatePath(center, hit.position, NavMesh.AllAreas, new NavMeshPath()))
                {
                    return hit.position;
                }
            }
        }
        
        // 如果找不到有效位置，返回当前位置
        // If no valid position found, return current position
        Debug.LogWarning($"[BirdMove] {gameObject.name} 在 {maxSampleAttempts} 次尝试后未找到有效目标");
        Debug.LogWarning($"[BirdMove] {gameObject.name} No valid target found after {maxSampleAttempts} attempts");
        return center;
    }
    
    private bool HasReachedTarget()
    {
        if (enableFlying)
        {
            // 飞行模式：检查距离和高度
            // Flight mode: Check distance and height
            float distance = Vector3.Distance(transform.position, currentTarget);
            float heightDifference = Mathf.Abs(transform.position.y - targetHeight);
            return distance <= minDistanceToTarget && heightDifference <= 0.5f;
        }
        else
        {
            // 地面模式：使用NavMeshAgent检查
            // Ground mode: Use NavMeshAgent check
            if (agent == null) return false;
            return !agent.pathPending && agent.remainingDistance <= minDistanceToTarget;
        }
    }
    
    private void OnReachedTarget()
    {
        isMoving = false;
        idleTimer = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[BirdMove] {gameObject.name} 到达目标: {currentTarget}");
            Debug.Log($"[BirdMove] {gameObject.name} Reached target: {currentTarget}");
        }
    }
    
    /// <summary>
    /// 公共方法：强制移动到指定位置
    /// Public method: Force move to specified position
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        if (enableFlying)
        {
            currentTarget = position;
            targetHeight = position.y;
            isMoving = true;
            idleTimer = 0f;
        }
        else
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
                Debug.LogWarning($"[BirdMove] {gameObject.name} Cannot move to specified position: {position}");
            }
        }
    }
    
    /// <summary>
    /// 公共方法：停止移动
    /// Public method: Stop movement
    /// </summary>
    public void StopMoving()
    {
        if (agent != null && !enableFlying)
        {
            agent.ResetPath();
        }
        isMoving = false;
        idleTimer = 0f;
    }
    
    /// <summary>
    /// 公共方法：切换飞行模式
    /// Public method: Toggle flight mode
    /// </summary>
    public void SetFlyingMode(bool flying)
    {
        enableFlying = flying;
        
        if (agent != null)
        {
            agent.enabled = !flying;
        }
        
        if (flying)
        {
            targetHeight = transform.position.y;
        }
    }
    
    // 在Scene视图中绘制调试信息
    // Draw debug info in Scene view
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;
        
        // 绘制移动范围
        // Draw movement range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRange);
        
        // 绘制当前目标
        // Draw current target
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
        
        // 绘制飞行高度范围
        // Draw flight height range
        if (enableFlying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(moveRange * 2, maxFlightHeight - minFlightHeight, moveRange * 2));
            
            // 绘制当前目标高度
            // Draw current target height
            if (isMoving)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(
                    new Vector3(currentTarget.x, minFlightHeight, currentTarget.z),
                    new Vector3(currentTarget.x, maxFlightHeight, currentTarget.z)
                );
            }
        }
    }
}
