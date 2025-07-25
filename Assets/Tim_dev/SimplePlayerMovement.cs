using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    
    private Rigidbody rb;
    private bool isGrounded;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // If no Rigidbody component, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Set Rigidbody properties
        rb.freezeRotation = true; // Prevent character from tilting
    }
    
    void Update()
    {
        // Get input
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S
        
        // Calculate move vector
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        
        // Apply movement
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    // Check if on the ground
    void OnCollisionStay(Collision collision)
    {
        // Check if the collision point is below the character
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