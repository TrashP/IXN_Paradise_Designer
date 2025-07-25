using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour
{
    [Header("Swim Settings")]
    [SerializeField] public float swimSpeed = 2f;           // Swim speed
    [SerializeField] public float turnSpeed = 3f;           // Turn speed
    [SerializeField] public float changeDirectionInterval = 3f; // Time interval to change direction
    [SerializeField] public float boundaryBuffer = 1f;      // Boundary buffer
    
    [Header("Stay Settings")]
    [SerializeField] private float stayProbability = 0.3f;  // Stay probability (0-1)
    [SerializeField] private float minStayTime = 1f;        // Minimum stay time
    [SerializeField] private float maxStayTime = 3f;        // Maximum stay time
    [SerializeField] private float stayCheckInterval = 2f;  // Time interval to check if staying
    
    [Header("Animation Settings")]
    [SerializeField] private Animator fishAnimator;          // Fish animator
    [SerializeField] private string isStayingParameterName = "IsStaying"; // Staying state parameter name
    
    [Header("Water Boundary")]
    [SerializeField] private Transform waterVolume;          // Water volume transform
    [SerializeField] private Vector3 waterBounds = new Vector3(10f, 5f, 10f); // Water boundary size
    
    [Header("Swim Behavior")]
    [SerializeField] private float minSwimHeight = 0.5f;    // Minimum swim height
    [SerializeField] private float maxSwimHeight = 4.5f;    // Maximum swim height
    [SerializeField] private float heightChangeSpeed = 1f;   // Height change speed
    
    private Vector3 targetDirection;                         // Target direction
    private Vector3 currentVelocity;                         // Current velocity
    private float nextDirectionChange;                       // Time to change direction next
    private float targetHeight;                             // Target height
    
    // Staying related variables
    private bool isStaying = false;                         // Whether staying
    private float stayEndTime;                              // Stay end time
    private float nextStayCheck;                            // Time to check if staying next
    
    // Water boundary
    private Vector3 waterCenter;                            // Water center
    private Vector3 waterMinBounds;                         // Water minimum boundary
    private Vector3 waterMaxBounds;                         // Water maximum boundary
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeWaterBounds();
        SetNewTargetDirection();
        SetNewTargetHeight();
        nextStayCheck = Time.time + stayCheckInterval;
        
        // If no Animator is specified, try to get it automatically
        if (fishAnimator == null)
        {
            fishAnimator = GetComponent<Animator>();
        }
        
        // Initialize animation state
        UpdateAnimatorState();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStayBehavior();
        
        if (!isStaying)
        {
            UpdateSwimming();
            CheckBoundaries();
            UpdateDirection();
        }
        
        // Synchronize state to Animator
        UpdateAnimatorState();
    }
    
    /// <summary>
    /// Update staying behavior
    /// </summary>
    private void UpdateStayBehavior()
    {
        // If staying, check if it should end staying
        if (isStaying)
        {
            if (Time.time >= stayEndTime)
            {
                isStaying = false;
                SetNewTargetDirection();
                SetNewTargetHeight();
                nextStayCheck = Time.time + stayCheckInterval;
            }
        }
        // If swimming, check if it should start staying
        else if (Time.time >= nextStayCheck)
        {
            if (Random.Range(0f, 1f) < stayProbability)
            {
                StartStaying();
            }
            else
            {
                nextStayCheck = Time.time + stayCheckInterval;
            }
        }
    }
    
    /// <summary>
    /// Start staying
    /// </summary>
    private void StartStaying()
    {
        isStaying = true;
        float stayDuration = Random.Range(minStayTime, maxStayTime);
        stayEndTime = Time.time + stayDuration;
    }
    
    /// <summary>
    /// Update Animator state
    /// </summary>
    private void UpdateAnimatorState()
    {
        if (fishAnimator != null)
        {
            fishAnimator.SetBool(isStayingParameterName, isStaying);
        }
    }
    
    /// <summary>
    /// Initialize water boundary
    /// </summary>
    private void InitializeWaterBounds()
    {
        if (waterVolume != null)
        {
            waterCenter = waterVolume.position;
            waterMinBounds = waterCenter - waterBounds * 0.5f;
            waterMaxBounds = waterCenter + waterBounds * 0.5f;
        }
        else
        {
            // If no water volume is specified, use default boundaries
            waterCenter = Vector3.zero;
            waterMinBounds = -waterBounds * 0.5f;
            waterMaxBounds = waterBounds * 0.5f;
        }
    }
    
    /// <summary>
    /// Update swimming logic
    /// </summary>
    private void UpdateSwimming()
    {
        // Smoothly turn to target direction
        Vector3 currentDirection = transform.forward;
        Vector3 newDirection = Vector3.Slerp(currentDirection, targetDirection, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
        
        // Swim forward
        transform.position += transform.forward * swimSpeed * Time.deltaTime;
        
        // Smoothly adjust height
        float currentHeight = transform.position.y;
        float heightDifference = targetHeight - currentHeight;
        if (Mathf.Abs(heightDifference) > 0.1f)
        {
            float newHeight = Mathf.MoveTowards(currentHeight, targetHeight, heightChangeSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
        }
    }
    
    /// <summary>
    /// Check boundaries and adjust direction
    /// </summary>
    private void CheckBoundaries()
    {
        Vector3 position = transform.position;
        bool needsDirectionChange = false;
        
        // Check X axis boundaries
        if (position.x <= waterMinBounds.x + boundaryBuffer || position.x >= waterMaxBounds.x - boundaryBuffer)
        {
            targetDirection.x = -targetDirection.x;
            needsDirectionChange = true;
        }
        
        // Check Z axis boundaries
        if (position.z <= waterMinBounds.z + boundaryBuffer || position.z >= waterMaxBounds.z - boundaryBuffer)
        {
            targetDirection.z = -targetDirection.z;
            needsDirectionChange = true;
        }
        
        // Check Y axis boundaries
        if (position.y <= waterMinBounds.y + boundaryBuffer || position.y >= waterMaxBounds.y - boundaryBuffer)
        {
            targetDirection.y = -targetDirection.y;
            needsDirectionChange = true;
        }
        
        // Ensure the fish does not swim out of boundaries
        if (needsDirectionChange)
        {
            // Pull the fish back into the boundaries
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(position.x, waterMinBounds.x + boundaryBuffer, waterMaxBounds.x - boundaryBuffer),
                Mathf.Clamp(position.y, waterMinBounds.y + boundaryBuffer, waterMaxBounds.y - boundaryBuffer),
                Mathf.Clamp(position.z, waterMinBounds.z + boundaryBuffer, waterMaxBounds.z - boundaryBuffer)
            );
            transform.position = clampedPosition;
        }
    }
    
    /// <summary>
    /// Update direction
    /// </summary>
    private void UpdateDirection()
    {
        if (Time.time >= nextDirectionChange)
        {
            SetNewTargetDirection();
            SetNewTargetHeight();
            nextDirectionChange = Time.time + changeDirectionInterval;
        }
    }
    
    /// <summary>
    /// Set new target direction
    /// </summary>
    private void SetNewTargetDirection()
    {
        // Generate random direction, but keep horizontal direction as the main direction
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 0.3f), // Vertical direction changes slightly
            Random.Range(-1f, 1f)
        ).normalized;
    }
    
    /// <summary>
    /// Set new target height
    /// </summary>
    private void SetNewTargetHeight()
    {
        targetHeight = Random.Range(minSwimHeight, maxSwimHeight);
    }
    
    /// <summary>
    /// Set water boundary (can be called in Inspector)
    /// </summary>
    public void SetWaterBounds(Vector3 bounds)
    {
        waterBounds = bounds;
        InitializeWaterBounds();
    }
    
    /// <summary>
    /// Set water center (can be called in Inspector)
    /// </summary>
    public void SetWaterCenter(Vector3 center)
    {
        waterCenter = center;
        InitializeWaterBounds();
    }
    
    /// <summary>
    /// Manually set animator (can be called in Inspector)
    /// </summary>
    public void SetAnimator(Animator animator)
    {
        fishAnimator = animator;
    }
    
    /// <summary>
    /// Manually set staying state parameter name (can be called in Inspector)
    /// </summary>
    public void SetIsStayingParameterName(string parameterName)
    {
        isStayingParameterName = parameterName;
    }
    
    /// <summary>
    /// Get current staying state (can be called in Inspector)
    /// </summary>
    public bool GetIsStaying()
    {
        return isStaying;
    }
    
    /// <summary>
    /// Draw boundaries in Scene view (only for debugging)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (waterVolume != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(waterCenter, waterBounds);
            
            // Draw buffer
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(waterCenter, waterBounds - Vector3.one * boundaryBuffer * 2);
        }
    }
}
