using UnityEngine;

/// <summary>
/// 鸟类滑翔控制器 - 提供平滑的飞行控制体验
/// </summary>
public class BirdGlideController : MonoBehaviour
{
    [Header("飞行参数")]
    [SerializeField, Range(1f, 50f)] private float glideSpeed = 10f;
    [SerializeField, Range(10f, 200f)] private float turnSpeed = 50f;
    [SerializeField, Range(0.1f, 2f)] private float verticalInfluence = 0.2f;
    
    [Header("边界限制")]
    [SerializeField] private bool enableBoundaries = true;
    [SerializeField] private float maxHeight = 100f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float boundaryRadius = 500f;
    
    [Header("平滑控制")]
    [SerializeField, Range(0.1f, 10f)] private float rotationSmoothing = 5f;
    [SerializeField, Range(0.1f, 10f)] private float movementSmoothing = 3f;
    
    // 私有变量
    private Vector3 targetRotation;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private bool isInitialized = false;
    
    // 属性访问器
    public float GlideSpeed 
    { 
        get => glideSpeed; 
        set => glideSpeed = Mathf.Clamp(value, 1f, 50f); 
    }
    
    public float TurnSpeed 
    { 
        get => turnSpeed; 
        set => turnSpeed = Mathf.Clamp(value, 10f, 200f); 
    }

    private void Start()
    {
        InitializeController();
    }

    private void InitializeController()
    {
        if (isInitialized)
        {
            Debug.LogWarning("BirdGlideController 已经初始化过了");
            return;
        }
        
        // 验证必要组件
        if (transform == null)
        {
            Debug.LogError("BirdGlideController: Transform 组件缺失!");
            enabled = false;
            return;
        }
        
        // 设置初始状态
        targetRotation = transform.rotation.eulerAngles;
        targetPosition = transform.position;
        
        // 确保初始高度在合理范围内
        if (enableBoundaries)
        {
            float currentHeight = transform.position.y;
            if (currentHeight < minHeight)
            {
                Vector3 newPos = transform.position;
                newPos.y = minHeight;
                transform.position = newPos;
                Debug.Log($"BirdGlideController: 调整初始高度到最小值 {minHeight}");
            }
        }
        
        isInitialized = true;
        Debug.Log("BirdGlideController 初始化完成");
    }

    private void Update()
    {
        if (!isInitialized || !enabled)
            return;
            
        HandleInput();
        ApplyMovement();
        ApplyBoundaries();
    }
    
    private void HandleInput()
    {
        try
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            // 应用输入死区
            horizontalInput = ApplyDeadzone(horizontalInput, 0.1f);
            verticalInput = ApplyDeadzone(verticalInput, 0.1f);
            
            // 计算目标旋转
            targetRotation.y += horizontalInput * turnSpeed * Time.deltaTime;
            
            // 计算目标位置
            Vector3 forwardDirection = transform.forward;
            Vector3 verticalDirection = Vector3.up * verticalInput * verticalInfluence;
            Vector3 targetDirection = (forwardDirection + verticalDirection).normalized;
            
            targetPosition = transform.position + targetDirection * glideSpeed * Time.deltaTime;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BirdGlideController 输入处理错误: {e.Message}");
        }
    }
    
    private void ApplyMovement()
    {
        try
        {
            // 平滑旋转
            Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetQuaternion, 
                rotationSmoothing * Time.deltaTime
            );
            
            // 平滑移动
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref currentVelocity, 
                1f / movementSmoothing
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BirdGlideController 移动应用错误: {e.Message}");
        }
    }
    
    private void ApplyBoundaries()
    {
        if (!enableBoundaries)
            return;
            
        try
        {
            Vector3 currentPos = transform.position;
            bool positionChanged = false;
            
            // 高度边界检查
            if (currentPos.y > maxHeight)
            {
                currentPos.y = maxHeight;
                positionChanged = true;
            }
            else if (currentPos.y < minHeight)
            {
                currentPos.y = minHeight;
                positionChanged = true;
            }
            
            // 水平边界检查
            Vector2 horizontalPos = new Vector2(currentPos.x, currentPos.z);
            if (horizontalPos.magnitude > boundaryRadius)
            {
                horizontalPos = horizontalPos.normalized * boundaryRadius;
                currentPos.x = horizontalPos.x;
                currentPos.z = horizontalPos.y;
                positionChanged = true;
            }
            
            if (positionChanged)
            {
                transform.position = currentPos;
                targetPosition = currentPos;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BirdGlideController 边界检查错误: {e.Message}");
        }
    }
    
    private float ApplyDeadzone(float input, float deadzone)
    {
        return Mathf.Abs(input) < deadzone ? 0f : input;
    }
    
    /// <summary>
    /// 重置控制器到初始状态
    /// </summary>
    public void ResetController()
    {
        isInitialized = false;
        currentVelocity = Vector3.zero;
        InitializeController();
    }
    
    /// <summary>
    /// 设置飞行速度
    /// </summary>
    public void SetGlideSpeed(float newSpeed)
    {
        GlideSpeed = newSpeed;
    }
    
    /// <summary>
    /// 设置转向速度
    /// </summary>
    public void SetTurnSpeed(float newSpeed)
    {
        TurnSpeed = newSpeed;
    }
    
    /// <summary>
    /// 获取当前飞行状态信息
    /// </summary>
    public string GetFlightInfo()
    {
        return $"位置: {transform.position}, 速度: {currentVelocity.magnitude:F1}, 高度: {transform.position.y:F1}";
    }
    
    private void OnValidate()
    {
        // 在编辑器中验证参数
        glideSpeed = Mathf.Clamp(glideSpeed, 1f, 50f);
        turnSpeed = Mathf.Clamp(turnSpeed, 10f, 200f);
        verticalInfluence = Mathf.Clamp(verticalInfluence, 0.1f, 2f);
        rotationSmoothing = Mathf.Clamp(rotationSmoothing, 0.1f, 10f);
        movementSmoothing = Mathf.Clamp(movementSmoothing, 0.1f, 10f);
        
        if (maxHeight <= minHeight)
        {
            maxHeight = minHeight + 10f;
        }
        
        if (boundaryRadius <= 0f)
        {
            boundaryRadius = 100f;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!enableBoundaries)
            return;
            
        // 绘制边界可视化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, boundaryRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(-boundaryRadius, minHeight, 0), 
            new Vector3(boundaryRadius, minHeight, 0)
        );
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-boundaryRadius, maxHeight, 0), 
            new Vector3(boundaryRadius, maxHeight, 0)
        );
    }
}
