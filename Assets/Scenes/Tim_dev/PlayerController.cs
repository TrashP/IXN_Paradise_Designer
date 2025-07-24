using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    
    [Header("Ground Check")]
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
        
        // Get Animator component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Lock mouse to screen center
        Cursor.lockState = CursorLockMode.Locked;
        
        // If no camera is set, try to find the main camera
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
            }
        }
        
        // Initialize position recording
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
        
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Horizontal rotation (left/right look)
        transform.Rotate(Vector3.up * mouseX);
        
        // Vertical rotation (up/down look)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Light downward force to ensure the character is on the ground
        }
        
        // Get input
        float x = Input.GetAxis("Horizontal"); // A/D or left/right arrows
        float z = Input.GetAxis("Vertical");   // W/S or up/down arrows
        
        // Calculate move direction
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Apply movement
        controller.Move(move * moveSpeed * Time.deltaTime);
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Calculate movement speed and update animation
        UpdateAnimationSpeed();
    }
    
    void UpdateAnimationSpeed()
    {
        if (animator == null) return;
        
        // Calculate current speed (based on position change)
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(lastPosition, currentPosition);
        currentSpeed = distance / Time.deltaTime;
        
        // Update animation state machine Speed parameter
        animator.SetFloat("Speed", currentSpeed);
        
        // Update position recording
        lastPosition = currentPosition;
    }
    
    // Draw ground check range in Scene view (only for debugging)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
} 