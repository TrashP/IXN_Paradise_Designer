using UnityEngine;

/// <summary>
/// 鸟类滑翔控制器 - 提供平滑的飞行控制体验
/// Bird Glide Controller - Provides smooth flight control experience
/// </summary>
public class BirdGlideController : MonoBehaviour
{
    [Header("飞行参数")]
    [Header("Flight Parameters")]
    [SerializeField, Range(1f, 50f)] private float glideSpeed = 10f; // 滑翔速度 Glide Speed
    [SerializeField, Range(0.1f, 2f)] private float verticalInfluence = 0.2f; // 垂直影响 Vertical Influence
    
    [Header("视角控制")]
    [Header("Camera Control")]
    [SerializeField, Range(0.1f, 5f)] private float mouseSensitivity = 2f; // 鼠标灵敏度 Mouse Sensitivity
    [SerializeField] private bool invertY = false; // 反转Y轴 Invert Y Axis
    [SerializeField] private bool lockCursor = true; // 锁定光标 Lock Cursor
    
    [Header("边界限制")]
    [Header("Boundary Limits")]
    [SerializeField] private bool enableBoundaries = true; // 启用边界 Enable Boundaries
    [SerializeField] private float maxHeight = 100f; // 最大高度 Maximum Height
    [SerializeField] private float minHeight = 5f; // 最小高度 Minimum Height
    [SerializeField] private float boundaryRadius = 500f; // 边界半径 Boundary Radius
    
    [Header("平滑控制")]
    [Header("Smoothing Control")]
    [SerializeField, Range(0.1f, 10f)] private float rotationSmoothing = 5f; // 旋转平滑 Rotation Smoothing
    [SerializeField, Range(0.1f, 10f)] private float movementSmoothing = 3f; // 移动平滑 Movement Smoothing
    
    // 私有变量
    // Private Variables
    private Vector3 targetRotation;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private bool isInitialized = false;
    private float currentPitch = 0f;
    private float currentYaw = 0f;
    
    // 属性访问器
    // Property Accessors
    public float GlideSpeed 
    { 
        get => glideSpeed; 
        set => glideSpeed = Mathf.Clamp(value, 1f, 50f); 
    }
    
    public float MouseSensitivity 
    { 
        get => mouseSensitivity; 
        set => mouseSensitivity = Mathf.Clamp(value, 0.1f, 5f); 
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
            Debug.LogWarning("BirdGlideController has already been initialized");
            return;
        }
        
        // 验证必要组件
        // Validate required components
        if (transform == null)
        {
            Debug.LogError("BirdGlideController: Transform 组件缺失!");
            Debug.LogError("BirdGlideController: Transform component missing!");
            enabled = false;
            return;
        }
        
        // 设置初始状态
        // Set initial state
        targetRotation = transform.rotation.eulerAngles;
        currentPitch = targetRotation.x;
        currentYaw = targetRotation.y;
        targetPosition = transform.position;
        
        // 锁定鼠标光标
        // Lock mouse cursor
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // 确保初始高度在合理范围内
        // Ensure initial height is within reasonable range
        if (enableBoundaries)
        {
            float currentHeight = transform.position.y;
            if (currentHeight < minHeight)
            {
                Vector3 newPos = transform.position;
                newPos.y = minHeight;
                transform.position = newPos;
                Debug.Log($"BirdGlideController: 调整初始高度到最小值 {minHeight}");
                Debug.Log($"BirdGlideController: Adjusted initial height to minimum {minHeight}");
            }
        }
        
        isInitialized = true;
        Debug.Log("BirdGlideController 初始化完成");
        Debug.Log("BirdGlideController initialization completed");
    }

    private void Update()
    {
        if (!isInitialized || !enabled)
            return;
            
        HandleInput();
        ApplyMovement();
        ApplyBoundaries();
        
        // 处理鼠标解锁
        // Handle mouse unlock
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    private void HandleInput()
    {
        try
        {
            // 鼠标视角控制
            // Mouse camera control
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // 应用Y轴反转
            // Apply Y-axis inversion
            if (invertY)
                mouseY = -mouseY;
            
            // 更新旋转角度
            // Update rotation angles
            currentYaw += mouseX;
            currentPitch -= mouseY; // 负值是因为Unity的坐标系
            // Negative value due to Unity's coordinate system
            
            // 限制俯仰角度（防止过度旋转）
            // Limit pitch angle (prevent over-rotation)
            currentPitch = Mathf.Clamp(currentPitch, -80f, 80f);
            
            // 设置目标旋转
            // Set target rotation
            targetRotation = new Vector3(currentPitch, currentYaw, 0f);
            
            // 垂直移动控制（W/S键）
            // Vertical movement control (W/S keys)
            float verticalInput = Input.GetAxis("Vertical");
            verticalInput = ApplyDeadzone(verticalInput, 0.1f);
            
            // 计算目标位置 - 基于当前朝向
            // Calculate target position - based on current orientation
            Vector3 forwardDirection = transform.forward;
            Vector3 verticalDirection = Vector3.up * verticalInput * verticalInfluence;
            Vector3 targetDirection = (forwardDirection + verticalDirection).normalized;
            
            targetPosition = transform.position + targetDirection * glideSpeed * Time.deltaTime;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BirdGlideController 输入处理错误: {e.Message}");
            Debug.LogError($"BirdGlideController input handling error: {e.Message}");
        }
    }
    
    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void ApplyMovement()
    {
        try
        {
            // 平滑旋转
            // Smooth rotation
            Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetQuaternion, 
                rotationSmoothing * Time.deltaTime
            );
            
            // 平滑移动
            // Smooth movement
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
            Debug.LogError($"BirdGlideController movement application error: {e.Message}");
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
            // Height boundary check
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
            // Horizontal boundary check
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
            Debug.LogError($"BirdGlideController boundary check error: {e.Message}");
        }
    }
    
    private float ApplyDeadzone(float input, float deadzone)
    {
        return Mathf.Abs(input) < deadzone ? 0f : input;
    }
    
    /// <summary>
    /// 重置控制器到初始状态
    /// Reset controller to initial state
    /// </summary>
    public void ResetController()
    {
        isInitialized = false;
        currentVelocity = Vector3.zero;
        currentPitch = 0f;
        currentYaw = 0f;
        InitializeController();
    }
    
    /// <summary>
    /// 设置飞行速度
    /// Set flight speed
    /// </summary>
    public void SetGlideSpeed(float newSpeed)
    {
        GlideSpeed = newSpeed;
    }
    
    /// <summary>
    /// 设置鼠标灵敏度
    /// Set mouse sensitivity
    /// </summary>
    public void SetMouseSensitivity(float newSensitivity)
    {
        MouseSensitivity = newSensitivity;
    }
    
    /// <summary>
    /// 获取当前飞行状态信息
    /// Get current flight status information
    /// </summary>
    public string GetFlightInfo()
    {
        return $"位置: {transform.position}, 速度: {currentVelocity.magnitude:F1}, 高度: {transform.position.y:F1}, 视角: ({currentPitch:F1}, {currentYaw:F1})";
        // Position: {transform.position}, Speed: {currentVelocity.magnitude:F1}, Height: {transform.position.y:F1}, View: ({currentPitch:F1}, {currentYaw:F1})
    }
    
    private void OnValidate()
    {
        // 在编辑器中验证参数
        // Validate parameters in editor
        glideSpeed = Mathf.Clamp(glideSpeed, 1f, 50f);
        mouseSensitivity = Mathf.Clamp(mouseSensitivity, 0.1f, 5f);
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
        // Draw boundary visualization
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
    
    private void OnDestroy()
    {
        // 恢复鼠标光标状态
        // Restore mouse cursor state
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
