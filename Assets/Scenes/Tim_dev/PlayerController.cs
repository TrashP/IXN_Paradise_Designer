using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("视角控制")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("动画控制")]
    [SerializeField] private Animator animator;
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask = 1;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private Vector3 lastPosition;
    private float currentSpeed;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 获取Animator组件
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        
        // 如果没有设置相机，尝试找到主相机
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
            }
        }
        
        // 初始化位置记录
        lastPosition = transform.position;
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }
    
    void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 水平旋转（左右看）
        transform.Rotate(Vector3.up * mouseX);
        
        // 垂直旋转（上下看）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        // 地面检测
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 轻微的下沉力，确保角色贴地
        }
        
        // 获取输入
        float x = Input.GetAxis("Horizontal"); // A/D 或 左右箭头
        float z = Input.GetAxis("Vertical");   // W/S 或 上下箭头
        
        // 计算移动方向
        Vector3 move = transform.right * x + transform.forward * z;
        
        // 应用移动
        controller.Move(move * moveSpeed * Time.deltaTime);
        
        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // 计算移动速度并更新动画
        UpdateAnimationSpeed();
    }
    
    void UpdateAnimationSpeed()
    {
        if (animator == null) return;
        
        // 计算当前速度（基于位置变化）
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(lastPosition, currentPosition);
        currentSpeed = distance / Time.deltaTime;
        
        // 更新动画状态机的Speed参数
        animator.SetFloat("Speed", currentSpeed);
        
        // 更新位置记录
        lastPosition = currentPosition;
    }
    
    // 在Scene视图中显示地面检测范围（仅用于调试）
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
} 