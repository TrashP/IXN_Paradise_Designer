using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 钓鱼点触发器组件
/// 当玩家进入钓鱼区域时显示提示UI，按下F键时启动钓鱼小游戏
/// </summary>
public class FishingPromptTrigger : MonoBehaviour
{
    [Header("UI 设置")]
    [SerializeField] private GameObject fishingPromptUI;
    [SerializeField] private GameObject fishingGameUI;  // 钓鱼小游戏UI
    
    [Header("事件")]
    [SerializeField] private UnityEvent onPlayerEnterFishingZone;
    [SerializeField] private UnityEvent onPlayerExitFishingZone;
    [SerializeField] private UnityEvent onFishingGameStart;  // 钓鱼游戏开始事件
    [SerializeField] private UnityEvent onFishingGameEnd;    // 钓鱼游戏结束事件
    
    [Header("调试")]
    [SerializeField] private bool enableDebugLogs = false;
    
    private bool isPlayerInZone = false;
    private bool isFishingGameActive = false;
    private FishingMiniGameCopy fishingMiniGame;
    
    #region Unity 生命周期
    
    private void Awake()
    {
        ValidateComponents();
        InitializeFishingGame();
    }
    
    private void OnValidate()
    {
        ValidateComponents();
    }
    
    private void Update()
    {
        // 检测F键输入
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.F))
        {
            ToggleFishingGame();
        }
        
        // 检测ESC键退出钓鱼游戏
        if (isFishingGameActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndFishingGame();
        }
    }
    
    #endregion
    
    #region 触发器事件
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidPlayer(other)) return;
        
        if (enableDebugLogs)
            Debug.Log($"[{gameObject.name}] 玩家进入钓鱼区域");
        
        isPlayerInZone = true;
        ShowFishingPrompt();
        onPlayerEnterFishingZone?.Invoke();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!IsValidPlayer(other)) return;
        
        if (enableDebugLogs)
            Debug.Log($"[{gameObject.name}] 玩家离开钓鱼区域");
        
        isPlayerInZone = false;
        HideFishingPrompt();
        
        // 如果玩家离开区域时正在钓鱼，自动结束钓鱼游戏
        if (isFishingGameActive)
        {
            EndFishingGame();
        }
        
        onPlayerExitFishingZone?.Invoke();
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 检查玩家是否在钓鱼区域内
    /// </summary>
    /// <returns>玩家是否在区域内</returns>
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }
    
    /// <summary>
    /// 检查钓鱼游戏是否正在运行
    /// </summary>
    /// <returns>钓鱼游戏是否激活</returns>
    public bool IsFishingGameActive()
    {
        return isFishingGameActive;
    }
    
    /// <summary>
    /// 手动显示钓鱼提示
    /// </summary>
    public void ShowFishingPrompt()
    {
        if (fishingPromptUI != null)
        {
            fishingPromptUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 钓鱼提示UI未设置");
        }
    }
    
    /// <summary>
    /// 手动隐藏钓鱼提示
    /// </summary>
    public void HideFishingPrompt()
    {
        if (fishingPromptUI != null)
        {
            fishingPromptUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 切换钓鱼游戏状态
    /// </summary>
    public void ToggleFishingGame()
    {
        if (isFishingGameActive)
        {
            EndFishingGame();
        }
        else
        {
            StartFishingGame();
        }
    }
    
    /// <summary>
    /// 开始钓鱼游戏
    /// </summary>
    public void StartFishingGame()
    {
        if (!isPlayerInZone)
        {
            Debug.LogWarning($"[{gameObject.name}] 玩家不在钓鱼区域内，无法开始钓鱼游戏");
            return;
        }
        
        if (fishingGameUI != null)
        {
            fishingGameUI.SetActive(true);
            isFishingGameActive = true;
            
            // 隐藏提示UI
            HideFishingPrompt();
            
            // 重置并启动钓鱼小游戏
            if (fishingMiniGame != null)
            {
                fishingMiniGame.ResetGame();
                fishingMiniGame.SetGameActive(true);
            }
            
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] 钓鱼游戏开始");
            
            onFishingGameStart?.Invoke();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 钓鱼游戏UI未设置");
        }
    }
    
    /// <summary>
    /// 结束钓鱼游戏
    /// </summary>
    public void EndFishingGame()
    {
        if (fishingGameUI != null)
        {
            fishingGameUI.SetActive(false);
            isFishingGameActive = false;
            
            // 暂停钓鱼小游戏
            if (fishingMiniGame != null)
            {
                fishingMiniGame.SetGameActive(false);
            }
            
            // 如果玩家仍在区域内，重新显示提示UI
            if (isPlayerInZone)
            {
                ShowFishingPrompt();
            }
            
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] 钓鱼游戏结束");
            
            onFishingGameEnd?.Invoke();
        }
    }
    
    #endregion
    
    #region 私有方法
    
    /// <summary>
    /// 初始化钓鱼游戏组件
    /// </summary>
    private void InitializeFishingGame()
    {
        if (fishingGameUI != null)
        {
            fishingMiniGame = fishingGameUI.GetComponent<FishingMiniGameCopy>();
            if (fishingMiniGame == null)
            {
                fishingMiniGame = fishingGameUI.GetComponentInChildren<FishingMiniGameCopy>();
            }
            
            // 初始时隐藏钓鱼游戏UI
            fishingGameUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 验证组件设置
    /// </summary>
    private void ValidateComponents()
    {
        if (fishingPromptUI == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 钓鱼提示UI未设置，请在Inspector中分配UI组件");
        }
        
        if (fishingGameUI == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 钓鱼游戏UI未设置，请在Inspector中分配钓鱼游戏UI组件");
        }
        
        // 确保有Collider组件
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] 缺少Collider组件，请添加Collider组件并设置为IsTrigger");
        }
        else if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider组件未设置为IsTrigger，这可能导致触发器不工作");
        }
    }
    
    /// <summary>
    /// 检查是否为有效的玩家对象
    /// </summary>
    /// <param name="other">碰撞的对象</param>
    /// <returns>是否为有效玩家</returns>
    private bool IsValidPlayer(Collider other)
    {
        if (other == null) return false;
        
        // 检查标签
        if (!other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] 非玩家对象进入触发器: {other.name}");
            return false;
        }
        
        return true;
    }
    
    #endregion
}

