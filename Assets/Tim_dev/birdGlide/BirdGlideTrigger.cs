using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 鸟类滑翔触发器 - 处理玩家与鸟类的滑翔交互
/// Bird Glide Trigger - Handles player-bird gliding interaction
/// </summary>
public class BirdGlideTrigger : MonoBehaviour
{
    [Header("滑翔设置")]
    [Header("Gliding Settings")]
    public Transform attachPoint; // 鸟背上的点 Attachment point on bird's back
    public float interactDistance = 3f; // 交互距离 Interaction Distance
    
    [Header("事件")]
    [Header("Events")]
    public UnityEvent onGlideStart; // 滑翔开始事件 Glide Start Event
    public UnityEvent onGlideEnd; // 滑翔结束事件 Glide End Event

    [Header("调试")]
    [Header("Debug")]
    public bool showDebugInfo = false; // 显示调试信息 Show Debug Info

    // 私有变量
    // Private Variables
    private bool isGliding = false;
    private GameObject player;
    private PlayerController playerController;
    private BirdGlideController glideController;
    private BirdMove birdMove; // 添加鸟类自动移动组件引用 Add bird auto-movement component reference
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    // 缓存组件引用
    // Cache component references
    private void Awake()
    {
        // 获取滑翔控制器组件
        // Get glide controller component
        glideController = GetComponent<BirdGlideController>();
        if (glideController == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 缺少BirdGlideController组件!");
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: Missing BirdGlideController component!");
        }

        // 获取鸟类自动移动组件
        // Get bird auto-movement component
        birdMove = GetComponent<BirdMove>();
        if (birdMove == null)
        {
            Debug.LogWarning($"[{gameObject.name}] BirdGlideTrigger: 未找到BirdMove组件，鸟类将无法自动移动!");
            Debug.LogWarning($"[{gameObject.name}] BirdGlideTrigger: BirdMove component not found, bird will not auto-move!");
        }

        // 验证附加点
        // Validate attachment point
        if (attachPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 未设置attachPoint!");
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: attachPoint not set!");
        }
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        // 查找玩家
        // Find player
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 场景中未找到Player标签的对象!");
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: No object with Player tag found in scene!");
            return;
        }

        // 获取玩家组件
        // Get player components
        playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] BirdGlideTrigger: 玩家对象缺少PlayerController组件!");
            Debug.LogWarning($"[{gameObject.name}] BirdGlideTrigger: Player object missing PlayerController component!");
        }

        playerRigidbody = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<Collider>();
    }

    private void Update()
    {
        if (player == null) return;

        if (isGliding)
        {
            CheckForExitGliding();
        }
        else
        {
            CheckForInteraction();
        }
    }
    
    private void CheckForExitGliding()
    {
        // 按ESC键退出滑翔模式
        // Press ESC key to exit gliding mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopGliding();
        }
    }

    private void CheckForInteraction()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 与玩家距离: {distance:F2}");
            Debug.Log($"[{gameObject.name}] Distance to player: {distance:F2}");
        }

        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.F))
        {
            StartGliding();
        }
    }

    /// <summary>
    /// 开始滑翔模式
    /// Start gliding mode
    /// </summary>
    public void StartGliding()
    {
        if (isGliding)
        {
            Debug.LogWarning($"[{gameObject.name}] 已经在滑翔状态中!");
            Debug.LogWarning($"[{gameObject.name}] Already in gliding state!");
            return;
        }

        if (attachPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法开始滑翔: attachPoint未设置!");
            Debug.LogError($"[{gameObject.name}] Cannot start gliding: attachPoint not set!");
            return;
        }

        if (glideController == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法开始滑翔: 缺少BirdGlideController组件!");
            Debug.LogError($"[{gameObject.name}] Cannot start gliding: Missing BirdGlideController component!");
            return;
        }

        isGliding = true;

        // 禁用玩家控制器
        // Disable player controller
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 禁用玩家物理
        // Disable player physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }

        // 禁用玩家碰撞器（可选）
        // Disable player collider (optional)
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 绑定玩家到鸟
        // Attach player to bird
        player.transform.SetParent(attachPoint);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;

        // 停止鸟类自动移动
        // Stop bird auto-movement
        if (birdMove != null)
        {
            birdMove.StopMoving();
            birdMove.enabled = false;
        }

        // 启动滑翔控制器
        // Enable glide controller
        glideController.enabled = true;

        // 触发事件
        // Trigger events
        onGlideStart?.Invoke();

        Debug.Log($"[{gameObject.name}] 开始滑翔模式");
        Debug.Log($"[{gameObject.name}] Started gliding mode");
    }

    /// <summary>
    /// 停止滑翔模式
    /// Stop gliding mode
    /// </summary>
    public void StopGliding()
    {
        if (!isGliding)
        {
            Debug.LogWarning($"[{gameObject.name}] 当前不在滑翔状态!");
            Debug.LogWarning($"[{gameObject.name}] Not currently in gliding state!");
            return;
        }

        isGliding = false;

        // 解除玩家绑定
        // Detach player
        if (player != null)
        {
            player.transform.SetParent(null);
        }

        // 重新启用玩家控制器
        // Re-enable player controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 重新启用玩家物理
        // Re-enable player physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        // 重新启用玩家碰撞器
        // Re-enable player collider
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // 重置玩家旋转（保持Y轴旋转，重置X轴和Z轴）
        // Reset player rotation (keep Y-axis rotation, reset X and Z axes)
        if (player != null)
        {
            Vector3 currentRotation = player.transform.rotation.eulerAngles;
            Vector3 resetRotation = new Vector3(0f, currentRotation.y, 0f);
            player.transform.rotation = Quaternion.Euler(resetRotation);
        }

        // 停止滑翔控制器
        // Stop glide controller
        if (glideController != null)
        {
            glideController.enabled = false;
        }

        // 重新启用鸟类自动移动
        // Re-enable bird auto-movement
        if (birdMove != null)
        {
            birdMove.enabled = true;
        }

        // 触发事件
        // Trigger events
        onGlideEnd?.Invoke();

        Debug.Log($"[{gameObject.name}] 结束滑翔模式");
        Debug.Log($"[{gameObject.name}] Ended gliding mode");
    }

    // 公共属性
    // Public Properties
    public bool IsGliding => isGliding; // 是否正在滑翔 Is Currently Gliding
    public GameObject Player => player; // 玩家对象 Player Object

    // 在Inspector中显示交互距离
    // Show interaction distance in Inspector
    private void OnDrawGizmosSelected()
    {
        if (attachPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attachPoint.position, interactDistance);
        }
    }

    // 清理
    // Cleanup
    private void OnDestroy()
    {
        // 确保在销毁时停止滑翔
        // Ensure gliding stops when destroyed
        if (isGliding)
        {
            StopGliding();
        }
    }
}
