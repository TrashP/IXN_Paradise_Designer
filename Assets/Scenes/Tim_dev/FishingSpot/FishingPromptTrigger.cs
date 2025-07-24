using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fishing point trigger component
/// When the player enters the fishing area, the prompt UI is displayed, and when the F key is pressed, the fishing mini game is started
/// </summary>
public class FishingPromptTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject fishingPromptUI;
    [SerializeField] private GameObject fishingGameUI;  // Fishing mini game UI
    
    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerEnterFishingZone;
    [SerializeField] private UnityEvent onPlayerExitFishingZone;
    [SerializeField] private UnityEvent onFishingGameStart;  // Fishing game start event
    [SerializeField] private UnityEvent onFishingGameEnd;    // Fishing game end event
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    
    private bool isPlayerInZone = false;
    private bool isFishingGameActive = false;
    private FishingMiniGameCopy fishingMiniGame;
    
    #region Unity lifecycle
    
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
        // Check F key input
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.F))
        {
            ToggleFishingGame();
        }
        
        // Check ESC key to exit fishing game
        if (isFishingGameActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndFishingGame();
        }
    }
    
    #endregion
    
    #region Trigger events
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidPlayer(other)) return;
        
        if (enableDebugLogs)
            Debug.Log($"[{gameObject.name}] Player entered fishing zone");
        
        isPlayerInZone = true;
        ShowFishingPrompt();
        onPlayerEnterFishingZone?.Invoke();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!IsValidPlayer(other)) return;
        
        if (enableDebugLogs)
            Debug.Log($"[{gameObject.name}] Player left fishing zone");
        
        isPlayerInZone = false;
        HideFishingPrompt();
        
        // If the player leaves the area while fishing, automatically end the fishing game
        if (isFishingGameActive)
        {
            EndFishingGame();
        }
        
        onPlayerExitFishingZone?.Invoke();
    }
    
    #endregion
    
    #region Public methods
    
    /// <summary>
    /// Check if the player is in the fishing area
    /// </summary>
    /// <returns>Whether the player is in the area</returns>
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }
    
    /// <summary>
    /// Check if the fishing game is running
    /// </summary>
    /// <returns>Whether the fishing game is active</returns>
    public bool IsFishingGameActive()
    {
        return isFishingGameActive;
    }
    
    /// <summary>
    /// Manually show fishing prompt
    /// </summary>
    public void ShowFishingPrompt()
    {
        if (fishingPromptUI != null)
        {
            fishingPromptUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Fishing prompt UI not set");
        }
    }
    
    /// <summary>
    /// Manually hide fishing prompt
    /// </summary>
    public void HideFishingPrompt()
    {
        if (fishingPromptUI != null)
        {
            fishingPromptUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Toggle fishing game state
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
    /// Start fishing game
    /// </summary>
    public void StartFishingGame()
    {
        if (!isPlayerInZone)
        {
            Debug.LogWarning($"[{gameObject.name}] Player not in fishing area, cannot start fishing game");
            return;
        }
        
        if (fishingGameUI != null)
        {
            fishingGameUI.SetActive(true);
            isFishingGameActive = true;
            
            // Hide prompt UI
            HideFishingPrompt();
            
            // Reset and start fishing mini game
            if (fishingMiniGame != null)
            {
                fishingMiniGame.ResetGame();
                fishingMiniGame.SetGameActive(true);
            }
            
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Fishing game started");
            
            onFishingGameStart?.Invoke();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Fishing game UI not set");
        }
    }
    
    /// <summary>
    /// End fishing game
    /// </summary>
    public void EndFishingGame()
    {
        if (fishingGameUI != null)
        {
            fishingGameUI.SetActive(false);
            isFishingGameActive = false;
            
            // Pause fishing mini game
            if (fishingMiniGame != null)
            {
                fishingMiniGame.SetGameActive(false);
            }
            
            // If the player is still in the area, redisplay the prompt UI
            if (isPlayerInZone)
            {
                ShowFishingPrompt();
            }
            
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Fishing game ended");
            
            onFishingGameEnd?.Invoke();
        }
    }
    
    #endregion
    
    #region Private methods
    
    /// <summary>
    /// Initialize fishing game components
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
            
            // Hide fishing game UI at the start
            fishingGameUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Validate component settings
    /// </summary>
    private void ValidateComponents()
    {
        if (fishingPromptUI == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Fishing prompt UI not set, please assign UI components in Inspector");
        }
        
        if (fishingGameUI == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Fishing game UI not set, please assign fishing game UI components in Inspector");
        }
        
        // Ensure there is a Collider component
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing Collider component, please add Collider component and set it to IsTrigger");
        }
        else if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider component is not set to IsTrigger, this may cause the trigger to not work");
        }
    }
    
    /// <summary>
    /// Check if the object is a valid player
    /// </summary>
    /// <param name="other">The object that collides</param>
    /// <returns>Whether the object is a valid player</returns>
    private bool IsValidPlayer(Collider other)
    {
        if (other == null) return false;
        
        // Check tag
        if (!other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Non-player object entered trigger: {other.name}");
            return false;
        }
        
        return true;
    }
    
    #endregion
}

