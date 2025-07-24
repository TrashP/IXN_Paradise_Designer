using UnityEngine;
using UnityEngine.UI;

public class FishingMiniGameCopy : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform barBackground;      // Background bar (determines range)
    public RectTransform catchBar;           // Player control bar
    public RectTransform fishIcon;           // Fish position
    public Image catchProgressBar;           // Top progress bar

    [Header("Victory UI")]
    public GameObject victoryUI;             // Victory UI panel
    public Text victoryText;                 // Victory text
    public Button restartButton;             // Restart button
    public Button closeButton;               // Close button

    [Header("Bar Settings")]
    public float barSpeed = 200f;            // Rightward movement speed
    public float gravity = 100f;             // Leftward movement speed
    public float catchBarWidth = 60f;        // Player control bar width
    
    [Header("Fish Movement Settings")]
    public float fishSpeed = 1f;             // Fish movement smoothness
    public float fishTimerMultiplier = 3f;   // Fish change direction time multiplier

    [Header("Game Control")]
    [SerializeField] private bool startGameOnEnable = false;  // Whether to start game automatically when enabled

    private float catchProgress = 0f;
    private float fishX;                     // Fish current X position
    private float fishDestination;           // Fish target position
    private float fishVelocity;              // Fish speed (for smooth movement)
    private float fishTimer;                 // Fish change direction timer
    private bool isGameActive = false;       // Whether the game is active
    private bool isInitialized = false;      // Whether it has been initialized
    private bool isVictory = false;          // Whether it has won

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
        if (delta <= 0f) return; // Prevent time exception

        UpdateCatchBar(delta);
        UpdateFishMovement(delta);
        UpdateCatchProgress(delta);
        CheckGameEnd();
    }

    /// <summary>
    /// Initialize game
    /// </summary>
    private void InitializeGame()
    {
        // Validate necessary components
        if (!ValidateComponents())
        {
            Debug.LogError("❌ FishingMiniGame: Missing necessary UI components, game cannot start!");
            enabled = false;
            return;
        }

        // Validate settings
        ValidateSettings();

        // Initialize fish position and state
        ResetGameState();

        isInitialized = true;
        
        // Game is not active at the start
        SetGameActive(false);
    }

    /// <summary>
    /// Reset game state
    /// </summary>
    private void ResetGameState()
    {
        fishX = Random.Range(0f, 1f);
        fishDestination = Random.Range(0f, 1f);
        fishVelocity = 0f;
        fishTimer = Random.Range(0f, fishTimerMultiplier);
        catchProgress = 0f;
        isVictory = false;
        
        // Reset progress bar
        if (catchProgressBar != null)
        {
            catchProgressBar.fillAmount = 0f;
        }

        // Hide victory UI
        HideVictoryUI();
    }

    private bool ValidateComponents()
    {
        if (barBackground == null)
        {
            Debug.LogError("❌ barBackground not set!");
            return false;
        }
        
        if (catchBar == null)
        {
            Debug.LogError("❌ catchBar not set!");
            return false;
        }
        
        if (fishIcon == null)
        {
            Debug.LogError("❌ fishIcon not set!");
            return false;
        }
        
        if (catchProgressBar == null)
        {
            Debug.LogError("❌ catchProgressBar not set!");
            return false;
        }

        // Validate victory UI components (optional)
        if (victoryUI == null)
        {
            Debug.LogWarning("⚠️ victoryUI not set, victory UI will not be displayed!");
        }

        return true;
    }

    private void ValidateSettings()
    {
        // Ensure values are within reasonable range
        barSpeed = Mathf.Max(0f, barSpeed);
        gravity = Mathf.Max(0f, gravity);
        catchBarWidth = Mathf.Max(1f, catchBarWidth);
        fishSpeed = Mathf.Max(0.1f, fishSpeed);
        fishTimerMultiplier = Mathf.Max(0.5f, fishTimerMultiplier);

        // Ensure catchBarWidth does not exceed background width
        if (barBackground != null && catchBarWidth > barBackground.rect.width)
        {
            catchBarWidth = barBackground.rect.width * 0.8f;
            Debug.LogWarning("⚠️ catchBarWidth exceeds background width, automatically adjusted!");
        }
    }

    private void UpdateCatchBar(float delta)
    {
        if (catchBar == null || barBackground == null) return;

        float barX = catchBar.anchoredPosition.x;
        
        // Control player control bar movement (hold left key to move right, release to move left)
        if (Input.GetMouseButton(0))
        {
            barX += barSpeed * delta;
        }
        else
        {
            barX -= gravity * delta;
        }

        // Limit to valid range
        float maxX = barBackground.rect.width - catchBarWidth;
        barX = Mathf.Clamp(barX, 0f, maxX);
        
        catchBar.anchoredPosition = new Vector2(barX, catchBar.anchoredPosition.y);
    }

    private void UpdateFishMovement(float delta)
    {
        if (fishIcon == null || barBackground == null) return;

        // Update fish timer
        fishTimer -= delta;
        
        // When timer reaches zero, set new target position
        if (fishTimer <= 0f)
        {
            fishTimer = Random.Range(0f, fishTimerMultiplier);
            fishDestination = Random.Range(0f, 1f);
        }

        // Use smooth movement to move fish towards target position
        fishX = Mathf.SmoothDamp(fishX, fishDestination, ref fishVelocity, fishSpeed);
        
        // Ensure fish position is within valid range
        fishX = Mathf.Clamp(fishX, 0f, 1f);
        
        // Calculate fish's actual display position
        float fishPosX = fishX * (barBackground.rect.width - 20f);
        fishIcon.anchoredPosition = new Vector2(fishPosX, fishIcon.anchoredPosition.y);
    }

    private void UpdateCatchProgress(float delta)
    {
        if (catchBar == null || fishIcon == null || catchProgressBar == null) return;

        // Check if fish is within control bar (left and right overlap)
        float barLeft = catchBar.anchoredPosition.x;
        float barRight = barLeft + catchBarWidth;
        float fishPosX = fishIcon.anchoredPosition.x;
        
        bool fishInBar = fishPosX >= barLeft && fishPosX <= barRight;

        // Update catch progress
        float progressChange = (fishInBar ? 1f : -1f) * delta * 0.5f;
        catchProgress += progressChange;
        catchProgress = Mathf.Clamp01(catchProgress);
        
        // Update UI display
        catchProgressBar.fillAmount = catchProgress;
    }

    private void CheckGameEnd()
    {
        // Check victory conditions
        if (catchProgress >= 1f && !isVictory)
        {
            isVictory = true;
            Debug.Log("Fish caught! Game victory!");
            catchProgress = 1f; // Keep at maximum value
            
            // Pause game
            SetGameActive(false);
            
            // Show victory UI
            ShowVictoryUI();
        }
        else if (catchProgress <= 0f)
        {
            Debug.Log("Fish escaped! But the game continues...");
            catchProgress = 0f; // Keep at minimum value
        }
    }

    /// <summary>
    /// Reset game
    /// </summary>
    public void ResetGame()
    {
        if (!ValidateComponents()) return;

        ResetGameState();
        SetGameActive(true);
    }

    /// <summary>
    /// Set game activation status
    /// </summary>
    /// <param name="active">Whether to activate the game</param>
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        
        if (active)
        {
            Debug.Log("Fishing game activated");
        }
        else
        {
            Debug.Log("Fishing game paused");
        }
    }

    /// <summary>
    /// Get game activation status
    /// </summary>
    /// <returns>Whether the game is activated</returns>
    public bool IsGameActive()
    {
        return isGameActive;
    }

    /// <summary>
    /// Get current catch progress
    /// </summary>
    /// <returns>Catch progress (0-1)</returns>
    public float GetCatchProgress()
    {
        return catchProgress;
    }

    /// <summary>
    /// Get victory status
    /// </summary>
    /// <returns>Whether it has won</returns>
    public bool IsVictory()
    {
        return isVictory;
    }

    /// <summary>
    /// Show fishing game UI
    /// </summary>
    public void ShowGameUI()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hide fishing game UI
    /// </summary>
    public void HideGameUI()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show victory UI
    /// </summary>
    private void ShowVictoryUI()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = "Fishing success!";
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
    /// Hide victory UI
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
    /// Restart button click event
    /// </summary>
    private void OnRestartButtonClick()
    {
        ResetGame();
        HideVictoryUI();
    }

    /// <summary>
    /// Close button click event
    /// </summary>
    private void OnCloseButtonClick()
    {
        HideGameUI();
    }
}
