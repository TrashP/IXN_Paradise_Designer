using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    
    private Rigidbody rb;
    private bool isGrounded;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 如果没有Rigidbody组件，添加一个
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 设置Rigidbody属性
        rb.freezeRotation = true; // 防止角色倾倒
    }
    
    void Update()
    {
        // 获取输入
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S
        
        // 计算移动向量
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        
        // 应用移动
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        
        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    // 检测是否在地面上
    void OnCollisionStay(Collision collision)
    {
        // 检查碰撞点是否在角色下方
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.point.y < transform.position.y + 0.1f)
            {
                isGrounded = true;
                return;
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
} 