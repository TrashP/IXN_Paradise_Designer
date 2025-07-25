using UnityEngine;
using UnityEngine.Events;

public class BirdGlideTrigger : MonoBehaviour
{
    [Header("滑翔设置")]
    public Transform attachPoint; // 鸟背上的点
    public float interactDistance = 3f;
    
    [Header("事件")]
    public UnityEvent onGlideStart;
    public UnityEvent onGlideEnd;

    [Header("调试")]
    public bool showDebugInfo = false;

    private bool isGliding = false;
    private GameObject player;
    private PlayerController playerController;
    private BirdGlideController glideController;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    // 缓存组件引用
    private void Awake()
    {
        // 获取滑翔控制器组件
        glideController = GetComponent<BirdGlideController>();
        if (glideController == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 缺少BirdGlideController组件!");
        }

        // 验证附加点
        if (attachPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 未设置attachPoint!");
        }
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        // 查找玩家
        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] BirdGlideTrigger: 场景中未找到Player标签的对象!");
            return;
        }

        // 获取玩家组件
        playerController = player.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] BirdGlideTrigger: 玩家对象缺少PlayerController组件!");
        }

        playerRigidbody = player.GetComponent<Rigidbody>();
        playerCollider = player.GetComponent<Collider>();
    }

    private void Update()
    {
        if (isGliding || player == null) return;

        CheckForInteraction();
    }

    private void CheckForInteraction()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 与玩家距离: {distance:F2}");
        }

        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.F))
        {
            StartGliding();
        }
    }

    public void StartGliding()
    {
        if (isGliding)
        {
            Debug.LogWarning($"[{gameObject.name}] 已经在滑翔状态中!");
            return;
        }

        if (attachPoint == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法开始滑翔: attachPoint未设置!");
            return;
        }

        if (glideController == null)
        {
            Debug.LogError($"[{gameObject.name}] 无法开始滑翔: 缺少BirdGlideController组件!");
            return;
        }

        isGliding = true;

        // 禁用玩家控制器
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 禁用玩家物理
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }

        // 禁用玩家碰撞器（可选）
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 绑定玩家到鸟
        player.transform.SetParent(attachPoint);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;

        // 启动滑翔控制器
        glideController.enabled = true;

        // 触发事件
        onGlideStart?.Invoke();

        Debug.Log($"[{gameObject.name}] 开始滑翔模式");
    }

    public void StopGliding()
    {
        if (!isGliding)
        {
            Debug.LogWarning($"[{gameObject.name}] 当前不在滑翔状态!");
            return;
        }

        isGliding = false;

        // 解除玩家绑定
        if (player != null)
        {
            player.transform.SetParent(null);
        }

        // 重新启用玩家控制器
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 重新启用玩家物理
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        // 重新启用玩家碰撞器
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // 停止滑翔控制器
        if (glideController != null)
        {
            glideController.enabled = false;
        }

        // 触发事件
        onGlideEnd?.Invoke();

        Debug.Log($"[{gameObject.name}] 结束滑翔模式");
    }

    // 公共属性
    public bool IsGliding => isGliding;
    public GameObject Player => player;

    // 在Inspector中显示交互距离
    private void OnDrawGizmosSelected()
    {
        if (attachPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attachPoint.position, interactDistance);
        }
    }

    // 清理
    private void OnDestroy()
    {
        // 确保在销毁时停止滑翔
        if (isGliding)
        {
            StopGliding();
        }
    }
}
