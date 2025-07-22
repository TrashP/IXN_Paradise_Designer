using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    [Header("相机控制设置")]
    [SerializeField] private float lookSpeed = 2.0f;         // 鼠标灵敏度
    [SerializeField] private float minPitch = -60f;          // 向下最小角度
    [SerializeField] private float maxPitch = 60f;           // 向上最大角度

    [Header("调试设置")]
    [SerializeField] private bool enableDebugLogs = false;   // 是否启用调试日志

    private float yaw = 0f;                // 左右旋转
    private float pitch = 0f;              // 上下旋转
    private bool isInitialized = false;    // 初始化状态标志

    void Start()
    {
        InitializeCamera();
    }

    void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("FreeLookCamera: 相机未正确初始化，跳过更新");
            return;
        }

        HandleMouseInput();
    }

    /// <summary>
    /// 初始化相机设置
    /// </summary>
    private void InitializeCamera()
    {
        try
        {
            // 验证参数有效性
            if (lookSpeed <= 0f)
            {
                Debug.LogError("FreeLookCamera: lookSpeed 必须大于0，已重置为默认值2.0f");
                lookSpeed = 2.0f;
            }

            if (minPitch >= maxPitch)
            {
                Debug.LogError("FreeLookCamera: minPitch 必须小于 maxPitch，已重置为默认值");
                minPitch = -60f;
                maxPitch = 60f;
            }

            // 锁定鼠标光标
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            isInitialized = true;

            if (enableDebugLogs)
            {
                Debug.Log("FreeLookCamera: 相机初始化完成");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreeLookCamera: 初始化失败 - {e.Message}");
            isInitialized = false;
        }
    }

    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void HandleMouseInput()
    {
        try
        {
            // 获取鼠标输入
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            // 检查输入是否有效
            if (float.IsNaN(mouseX) || float.IsNaN(mouseY))
            {
                Debug.LogWarning("FreeLookCamera: 检测到无效的鼠标输入，跳过此帧");
                return;
            }

            // 更新旋转角度
            yaw += mouseX;
            pitch -= mouseY;

            // 限制上下角度
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // 应用旋转
            Vector3 newRotation = new Vector3(pitch, yaw, 0f);
            
            // 验证旋转值是否有效
            if (IsValidRotation(newRotation))
            {
                transform.eulerAngles = newRotation;
            }
            else
            {
                Debug.LogWarning("FreeLookCamera: 检测到无效的旋转值，跳过此帧");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FreeLookCamera: 处理鼠标输入时发生错误 - {e.Message}");
        }
    }

    /// <summary>
    /// 验证旋转值是否有效
    /// </summary>
    /// <param name="rotation">要验证的旋转值</param>
    /// <returns>旋转值是否有效</returns>
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
    /// 重置相机旋转
    /// </summary>
    public void ResetRotation()
    {
        yaw = 0f;
        pitch = 0f;
        transform.eulerAngles = Vector3.zero;
        
        if (enableDebugLogs)
        {
            Debug.Log("FreeLookCamera: 相机旋转已重置");
        }
    }

    /// <summary>
    /// 设置鼠标灵敏度
    /// </summary>
    /// <param name="newSpeed">新的灵敏度值</param>
    public void SetLookSpeed(float newSpeed)
    {
        if (newSpeed > 0f)
        {
            lookSpeed = newSpeed;
            if (enableDebugLogs)
            {
                Debug.Log($"FreeLookCamera: 鼠标灵敏度已设置为 {newSpeed}");
            }
        }
        else
        {
            Debug.LogWarning("FreeLookCamera: 尝试设置无效的鼠标灵敏度值");
        }
    }

    /// <summary>
    /// 设置俯仰角度限制
    /// </summary>
    /// <param name="min">最小角度</param>
    /// <param name="max">最大角度</param>
    public void SetPitchLimits(float min, float max)
    {
        if (min < max)
        {
            minPitch = min;
            maxPitch = max;
            if (enableDebugLogs)
            {
                Debug.Log($"FreeLookCamera: 俯仰角度限制已设置为 {min} 到 {max}");
            }
        }
        else
        {
            Debug.LogWarning("FreeLookCamera: 尝试设置无效的俯仰角度限制");
        }
    }

    void OnDestroy()
    {
        // 恢复鼠标光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (enableDebugLogs)
        {
            Debug.Log("FreeLookCamera: 相机已销毁，鼠标光标已恢复");
        }
    }
}
