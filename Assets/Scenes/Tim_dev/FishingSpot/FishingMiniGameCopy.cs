using UnityEngine;
using UnityEngine.UI;

public class FishingMiniGameCopy : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground;      // 背景条（决定范围）
    public RectTransform catchBar;           // 玩家控制条
    public RectTransform fishIcon;           // 鱼的位置
    public Image catchProgressBar;           // 顶部进度条

    [Header("Victory UI")]
    public GameObject victoryUI;             // 胜利UI面板
    public Text victoryText;                 // 胜利文本
    public Button restartButton;             // 重新开始按钮
    public Button closeButton;               // 关闭按钮

    [Header("Bar Settings")]
    public float barSpeed = 200f;            // 向右前进速度
    public float gravity = 100f;             // 向左后退速度
    public float catchBarWidth = 60f;        // 玩家控制条宽度
    
    [Header("Fish Movement Settings")]
    public float fishSpeed = 1f;             // 鱼移动平滑度
    public float fishTimerMultiplier = 3f;   // 鱼改变方向的时间倍数

    [Header("Game Control")]
    [SerializeField] private bool startGameOnEnable = false;  // 是否在启用时自动开始游戏

    private float catchProgress = 0f;
    private float fishX;                     // 鱼当前 X 位置
    private float fishDestination;           // 鱼的目标位置
    private float fishVelocity;              // 鱼的速度（用于平滑移动）
    private float fishTimer;                 // 鱼改变方向的计时器
    private bool isGameActive = false;       // 游戏是否激活
    private bool isInitialized = false;      // 是否已初始化
    private bool isVictory = false;          // 是否已胜利

    void Start()
    {
        InitializeGame();
    }

    void OnEnable()
    {
        if (startGameOnEnable && isInitialized)
        {
            SetGameActive(true);
        }
    }

    void OnDisable()
    {
        SetGameActive(false);
    }

    void Update()
    {
        if (!isGameActive || !enabled) return;

        float delta = Time.deltaTime;
        if (delta <= 0f) return; // 防止时间异常

        UpdateCatchBar(delta);
        UpdateFishMovement(delta);
        UpdateCatchProgress(delta);
        CheckGameEnd();
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 验证必要组件是否存在
        if (!ValidateComponents())
        {
            Debug.LogError("❌ FishingMiniGame: 缺少必要的UI组件，游戏无法启动！");
            enabled = false;
            return;
        }

        // 验证数值设置
        ValidateSettings();

        // 初始化鱼的位置和状态
        ResetGameState();

        isInitialized = true;
        
        // 初始时游戏不激活
        SetGameActive(false);
    }

    /// <summary>
    /// 重置游戏状态
    /// </summary>
    private void ResetGameState()
    {
        fishX = Random.Range(0f, 1f);
        fishDestination = Random.Range(0f, 1f);
        fishVelocity = 0f;
        fishTimer = Random.Range(0f, fishTimerMultiplier);
        catchProgress = 0f;
        isVictory = false;
        
        // 重置进度条
        if (catchProgressBar != null)
        {
            catchProgressBar.fillAmount = 0f;
        }

        // 隐藏胜利UI
        HideVictoryUI();
    }

    private bool ValidateComponents()
    {
        if (barBackground == null)
        {
            Debug.LogError("❌ barBackground 未设置！");
            return false;
        }
        
        if (catchBar == null)
        {
            Debug.LogError("❌ catchBar 未设置！");
            return false;
        }
        
        if (fishIcon == null)
        {
            Debug.LogError("❌ fishIcon 未设置！");
            return false;
        }
        
        if (catchProgressBar == null)
        {
            Debug.LogError("❌ catchProgressBar 未设置！");
            return false;
        }

        // 验证胜利UI组件（可选）
        if (victoryUI == null)
        {
            Debug.LogWarning("⚠️ victoryUI 未设置，胜利时不会显示UI！");
        }

        return true;
    }

    private void ValidateSettings()
    {
        // 确保数值在合理范围内
        barSpeed = Mathf.Max(0f, barSpeed);
        gravity = Mathf.Max(0f, gravity);
        catchBarWidth = Mathf.Max(1f, catchBarWidth);
        fishSpeed = Mathf.Max(0.1f, fishSpeed);
        fishTimerMultiplier = Mathf.Max(0.5f, fishTimerMultiplier);

        // 确保catchBarWidth不超过背景宽度
        if (barBackground != null && catchBarWidth > barBackground.rect.width)
        {
            catchBarWidth = barBackground.rect.width * 0.8f;
            Debug.LogWarning("⚠️ catchBarWidth 超过背景宽度，已自动调整！");
        }
    }

    private void UpdateCatchBar(float delta)
    {
        if (catchBar == null || barBackground == null) return;

        float barX = catchBar.anchoredPosition.x;
        
        // 控制玩家控制条移动（按住左键向右前进，松开向左后退）
        if (Input.GetMouseButton(0))
        {
            barX += barSpeed * delta;
        }
        else
        {
            barX -= gravity * delta;
        }

        // 限制在有效范围内
        float maxX = barBackground.rect.width - catchBarWidth;
        barX = Mathf.Clamp(barX, 0f, maxX);
        
        catchBar.anchoredPosition = new Vector2(barX, catchBar.anchoredPosition.y);
    }

    private void UpdateFishMovement(float delta)
    {
        if (fishIcon == null || barBackground == null) return;

        // 更新鱼的计时器
        fishTimer -= delta;
        
        // 当计时器归零时，设置新的目标位置
        if (fishTimer <= 0f)
        {
            fishTimer = Random.Range(0f, fishTimerMultiplier);
            fishDestination = Random.Range(0f, 1f);
        }

        // 使用平滑移动让鱼向目标位置移动
        fishX = Mathf.SmoothDamp(fishX, fishDestination, ref fishVelocity, fishSpeed);
        
        // 确保鱼的位置在有效范围内
        fishX = Mathf.Clamp(fishX, 0f, 1f);
        
        // 计算鱼的实际显示位置
        float fishPosX = fishX * (barBackground.rect.width - 20f);
        fishIcon.anchoredPosition = new Vector2(fishPosX, fishIcon.anchoredPosition.y);
    }

    private void UpdateCatchProgress(float delta)
    {
        if (catchBar == null || fishIcon == null || catchProgressBar == null) return;

        // 判断鱼是否在控制条内（左右重合）
        float barLeft = catchBar.anchoredPosition.x;
        float barRight = barLeft + catchBarWidth;
        float fishPosX = fishIcon.anchoredPosition.x;
        
        bool fishInBar = fishPosX >= barLeft && fishPosX <= barRight;

        // 更新捕获进度
        float progressChange = (fishInBar ? 1f : -1f) * delta * 0.5f;
        catchProgress += progressChange;
        catchProgress = Mathf.Clamp01(catchProgress);
        
        // 更新UI显示
        catchProgressBar.fillAmount = catchProgress;
    }

    private void CheckGameEnd()
    {
        // 检查胜利条件
        if (catchProgress >= 1f && !isVictory)
        {
            isVictory = true;
            Debug.Log("✅ 鱼钓上来了！游戏胜利！");
            catchProgress = 1f; // 保持在最大值
            
            // 暂停游戏
            SetGameActive(false);
            
            // 显示胜利UI
            ShowVictoryUI();
        }
        else if (catchProgress <= 0f)
        {
            Debug.Log("❌ 鱼逃跑了！但游戏继续...");
            catchProgress = 0f; // 保持在最小值
        }
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    public void ResetGame()
    {
        if (!ValidateComponents()) return;

        ResetGameState();
        SetGameActive(true);
    }

    /// <summary>
    /// 设置游戏激活状态
    /// </summary>
    /// <param name="active">是否激活游戏</param>
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        
        if (active)
        {
            Debug.Log("🎣 钓鱼游戏已激活");
        }
        else
        {
            Debug.Log("⏸️ 钓鱼游戏已暂停");
        }
    }

    /// <summary>
    /// 获取游戏激活状态
    /// </summary>
    /// <returns>游戏是否激活</returns>
    public bool IsGameActive()
    {
        return isGameActive;
    }

    /// <summary>
    /// 获取当前捕获进度
    /// </summary>
    /// <returns>捕获进度 (0-1)</returns>
    public float GetCatchProgress()
    {
        return catchProgress;
    }

    /// <summary>
    /// 获取胜利状态
    /// </summary>
    /// <returns>是否已胜利</returns>
    public bool IsVictory()
    {
        return isVictory;
    }

    /// <summary>
    /// 显示钓鱼游戏UI
    /// </summary>
    public void ShowGameUI()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏钓鱼游戏UI
    /// </summary>
    public void HideGameUI()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示胜利UI
    /// </summary>
    private void ShowVictoryUI()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = "🎉 钓鱼成功！";
            }
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClick);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }
    }

    /// <summary>
    /// 隐藏胜利UI
    /// </summary>
    private void HideVictoryUI()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(false);
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }

    /// <summary>
    /// 重新开始按钮点击事件
    /// </summary>
    private void OnRestartButtonClick()
    {
        ResetGame();
        HideVictoryUI();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void OnCloseButtonClick()
    {
        HideGameUI();
    }
}
