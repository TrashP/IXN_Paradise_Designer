using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float lookSpeed = 2.0f;         // Mouse sensitivity
    [SerializeField] private float minPitch = -60f;          // Minimum downward angle
    [SerializeField] private float maxPitch = 60f;           // Maximum upward angle

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;   // Whether to enable debug logs

    private float yaw = 0f;                // Left-right rotation
    private float pitch = 0f;              // Up-down rotation
    private bool isInitialized = false;    // Initialization status flag

    void Start()
    {
        InitializeCamera();
    }

    void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("FreeLookCamera: Camera not properly initialized, skipping update");
            return;
        }

        HandleMouseInput();
    }

    /// <summary>
    /// Initialize camera settings
    /// </summary>
    private void InitializeCamera()
    {
        try
        {
            // 验证参数有效性
            if (lookSpeed <= 0f)
            {
                Debug.LogError("FreeLookCamera: lookSpeed must be greater than 0, reset to default value 2.0f");
                lookSpeed = 2.0f;
            }

            if (minPitch >= maxPitch)
            {
                Debug.LogError("FreeLookCamera: minPitch must be less than maxPitch, reset to default value");
                minPitch = -60f;
                maxPitch = 60f;
            }

            // Lock mouse cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isInitialized = true;

            if (enableDebugLogs)
            {
                Debug.Log("FreeLookCamera: Camera initialization completed");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreeLookCamera: Initialization failed - {e.Message}");
            isInitialized = false;
        }
    }

    /// <summary>
    /// Handle mouse input
    /// </summary>
    private void HandleMouseInput()
    {
        try
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            // Check if input is valid
            if (float.IsNaN(mouseX) || float.IsNaN(mouseY))
            {
                Debug.LogWarning("FreeLookCamera: Invalid mouse input, skipping this frame");
                return;
            }

            // Update rotation angles
            yaw += mouseX;
            pitch -= mouseY;

            // Limit up-down angles
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Apply rotation
            Vector3 newRotation = new Vector3(pitch, yaw, 0f);
            
            // Check if rotation value is valid
            if (IsValidRotation(newRotation))
            {
                transform.eulerAngles = newRotation;
            }
            else
            {
                Debug.LogWarning("FreeLookCamera: Invalid rotation value, skipping this frame");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreeLookCamera: Error handling mouse input - {e.Message}");
        }
    }

    /// <summary>
    /// Check if rotation value is valid
    /// </summary>
    /// <param name="rotation">Rotation value to check</param>
    /// <returns>Whether the rotation value is valid</returns>
    private bool IsValidRotation(Vector3 rotation)
    {
        return !float.IsNaN(rotation.x) && 
               !float.IsNaN(rotation.y) && 
               !float.IsNaN(rotation.z) &&
               !float.IsInfinity(rotation.x) && 
               !float.IsInfinity(rotation.y) && 
               !float.IsInfinity(rotation.z);
    }

    /// <summary>
    /// Reset camera rotation
    /// </summary>
    public void ResetRotation()
    {
        yaw = 0f;
        pitch = 0f;
        transform.eulerAngles = Vector3.zero;
        
        if (enableDebugLogs)
        {
            Debug.Log("FreeLookCamera: Camera rotation reset");
        }
    }

    /// <summary>
    /// Set mouse sensitivity
    /// </summary>
    /// <param name="newSpeed">New sensitivity value</param>
    public void SetLookSpeed(float newSpeed)
    {
        if (newSpeed > 0f)
        {
            lookSpeed = newSpeed;
            if (enableDebugLogs)
            {
                Debug.Log($"FreeLookCamera: Mouse sensitivity set to {newSpeed}");
            }
        }
        else
        {
            Debug.LogWarning("FreeLookCamera: Invalid mouse sensitivity value, skipping this frame");
        }
    }

    /// <summary>
    /// Set pitch angle limits
    /// </summary>
    /// <param name="min">Minimum angle</param>
    /// <param name="max">Maximum angle</param>
    public void SetPitchLimits(float min, float max)
    {
        if (min < max)
        {
            minPitch = min;
            maxPitch = max;
            if (enableDebugLogs)
            {
                Debug.Log($"FreeLookCamera: Pitch angle limits set to {min} to {max}");
            }
        }
        else
        {
            Debug.LogWarning("FreeLookCamera: Invalid pitch angle limits, skipping this frame");
        }
    }

    void OnDestroy()
    {
        // Restore mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (enableDebugLogs)
        {
            Debug.Log("FreeLookCamera: Camera destroyed, mouse cursor restored");
        }
    }
}
