using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour
{
    [Header("游动设置")]
    [SerializeField] public float swimSpeed = 2f;           // 游动速度
    [SerializeField] public float turnSpeed = 3f;           // 转向速度
    [SerializeField] public float changeDirectionInterval = 3f; // 改变方向的时间间隔
    [SerializeField] public float boundaryBuffer = 1f;      // 边界缓冲区
    
    [Header("停留设置")]
    [SerializeField] private float stayProbability = 0.3f;  // 停留概率 (0-1)
    [SerializeField] private float minStayTime = 1f;        // 最小停留时间
    [SerializeField] private float maxStayTime = 3f;        // 最大停留时间
    [SerializeField] private float stayCheckInterval = 2f;  // 检查是否停留的时间间隔
    
    [Header("动画设置")]
    [SerializeField] private Animator fishAnimator;          // 鱼的动画控制器
    [SerializeField] private string isStayingParameterName = "IsStaying"; // 停留状态参数名称
    
    [Header("水体边界")]
    [SerializeField] private Transform waterVolume;          // 水体体积的Transform
    [SerializeField] private Vector3 waterBounds = new Vector3(10f, 5f, 10f); // 水体边界大小
    
    [Header("游动行为")]
    [SerializeField] private float minSwimHeight = 0.5f;    // 最小游动高度
    [SerializeField] private float maxSwimHeight = 4.5f;    // 最大游动高度
    [SerializeField] private float heightChangeSpeed = 1f;   // 高度变化速度
    
    private Vector3 targetDirection;                         // 目标方向
    private Vector3 currentVelocity;                         // 当前速度
    private float nextDirectionChange;                       // 下次改变方向的时间
    private float targetHeight;                             // 目标高度
    
    // 停留相关变量
    private bool isStaying = false;                         // 是否正在停留
    private float stayEndTime;                              // 停留结束时间
    private float nextStayCheck;                            // 下次检查停留的时间
    
    // 水体边界
    private Vector3 waterCenter;                            // 水体中心
    private Vector3 waterMinBounds;                         // 水体最小边界
    private Vector3 waterMaxBounds;                         // 水体最大边界
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeWaterBounds();
        SetNewTargetDirection();
        SetNewTargetHeight();
        nextStayCheck = Time.time + stayCheckInterval;
        
        // 如果没有指定Animator，尝试自动获取
        if (fishAnimator == null)
        {
            fishAnimator = GetComponent<Animator>();
        }
        
        // 初始化动画状态
        UpdateAnimatorState();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStayBehavior();
        
        if (!isStaying)
        {
            UpdateSwimming();
            CheckBoundaries();
            UpdateDirection();
        }
        
        // 同步状态到Animator
        UpdateAnimatorState();
    }
    
    /// <summary>
    /// 更新停留行为
    /// </summary>
    private void UpdateStayBehavior()
    {
        // 如果正在停留，检查是否应该结束停留
        if (isStaying)
        {
            if (Time.time >= stayEndTime)
            {
                isStaying = false;
                SetNewTargetDirection();
                SetNewTargetHeight();
                nextStayCheck = Time.time + stayCheckInterval;
            }
        }
        // 如果正在游动，检查是否应该开始停留
        else if (Time.time >= nextStayCheck)
        {
            if (Random.Range(0f, 1f) < stayProbability)
            {
                StartStaying();
            }
            else
            {
                nextStayCheck = Time.time + stayCheckInterval;
            }
        }
    }
    
    /// <summary>
    /// 开始停留
    /// </summary>
    private void StartStaying()
    {
        isStaying = true;
        float stayDuration = Random.Range(minStayTime, maxStayTime);
        stayEndTime = Time.time + stayDuration;
    }
    
    /// <summary>
    /// 更新Animator状态
    /// </summary>
    private void UpdateAnimatorState()
    {
        if (fishAnimator != null)
        {
            fishAnimator.SetBool(isStayingParameterName, isStaying);
        }
    }
    
    /// <summary>
    /// 初始化水体边界
    /// </summary>
    private void InitializeWaterBounds()
    {
        if (waterVolume != null)
        {
            waterCenter = waterVolume.position;
            waterMinBounds = waterCenter - waterBounds * 0.5f;
            waterMaxBounds = waterCenter + waterBounds * 0.5f;
        }
        else
        {
            // 如果没有指定水体，使用默认边界
            waterCenter = Vector3.zero;
            waterMinBounds = -waterBounds * 0.5f;
            waterMaxBounds = waterBounds * 0.5f;
        }
    }
    
    /// <summary>
    /// 更新游动逻辑
    /// </summary>
    private void UpdateSwimming()
    {
        // 平滑转向目标方向
        Vector3 currentDirection = transform.forward;
        Vector3 newDirection = Vector3.Slerp(currentDirection, targetDirection, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
        
        // 向前游动
        transform.position += transform.forward * swimSpeed * Time.deltaTime;
        
        // 平滑调整高度
        float currentHeight = transform.position.y;
        float heightDifference = targetHeight - currentHeight;
        if (Mathf.Abs(heightDifference) > 0.1f)
        {
            float newHeight = Mathf.MoveTowards(currentHeight, targetHeight, heightChangeSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
        }
    }
    
    /// <summary>
    /// 检查边界并调整方向
    /// </summary>
    private void CheckBoundaries()
    {
        Vector3 position = transform.position;
        bool needsDirectionChange = false;
        
        // 检查X轴边界
        if (position.x <= waterMinBounds.x + boundaryBuffer || position.x >= waterMaxBounds.x - boundaryBuffer)
        {
            targetDirection.x = -targetDirection.x;
            needsDirectionChange = true;
        }
        
        // 检查Z轴边界
        if (position.z <= waterMinBounds.z + boundaryBuffer || position.z >= waterMaxBounds.z - boundaryBuffer)
        {
            targetDirection.z = -targetDirection.z;
            needsDirectionChange = true;
        }
        
        // 检查Y轴边界
        if (position.y <= waterMinBounds.y + boundaryBuffer || position.y >= waterMaxBounds.y - boundaryBuffer)
        {
            targetDirection.y = -targetDirection.y;
            needsDirectionChange = true;
        }
        
        // 确保鱼不会游出边界
        if (needsDirectionChange)
        {
            // 将鱼拉回边界内
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(position.x, waterMinBounds.x + boundaryBuffer, waterMaxBounds.x - boundaryBuffer),
                Mathf.Clamp(position.y, waterMinBounds.y + boundaryBuffer, waterMaxBounds.y - boundaryBuffer),
                Mathf.Clamp(position.z, waterMinBounds.z + boundaryBuffer, waterMaxBounds.z - boundaryBuffer)
            );
            transform.position = clampedPosition;
        }
    }
    
    /// <summary>
    /// 更新方向
    /// </summary>
    private void UpdateDirection()
    {
        if (Time.time >= nextDirectionChange)
        {
            SetNewTargetDirection();
            SetNewTargetHeight();
            nextDirectionChange = Time.time + changeDirectionInterval;
        }
    }
    
    /// <summary>
    /// 设置新的目标方向
    /// </summary>
    private void SetNewTargetDirection()
    {
        // 生成随机方向，但保持水平方向为主
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 0.3f), // 垂直方向变化较小
            Random.Range(-1f, 1f)
        ).normalized;
    }
    
    /// <summary>
    /// 设置新的目标高度
    /// </summary>
    private void SetNewTargetHeight()
    {
        targetHeight = Random.Range(minSwimHeight, maxSwimHeight);
    }
    
    /// <summary>
    /// 设置水体边界（可在Inspector中调用）
    /// </summary>
    public void SetWaterBounds(Vector3 bounds)
    {
        waterBounds = bounds;
        InitializeWaterBounds();
    }
    
    /// <summary>
    /// 设置水体中心（可在Inspector中调用）
    /// </summary>
    public void SetWaterCenter(Vector3 center)
    {
        waterCenter = center;
        InitializeWaterBounds();
    }
    
    /// <summary>
    /// 手动设置动画控制器（可在Inspector中调用）
    /// </summary>
    public void SetAnimator(Animator animator)
    {
        fishAnimator = animator;
    }
    
    /// <summary>
    /// 手动设置停留状态参数名称（可在Inspector中调用）
    /// </summary>
    public void SetIsStayingParameterName(string parameterName)
    {
        isStayingParameterName = parameterName;
    }
    
    /// <summary>
    /// 获取当前停留状态（可在Inspector中调用）
    /// </summary>
    public bool GetIsStaying()
    {
        return isStaying;
    }
    
    /// <summary>
    /// 在Scene视图中绘制边界（仅用于调试）
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (waterVolume != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(waterCenter, waterBounds);
            
            // 绘制缓冲区
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(waterCenter, waterBounds - Vector3.one * boundaryBuffer * 2);
        }
    }
}
