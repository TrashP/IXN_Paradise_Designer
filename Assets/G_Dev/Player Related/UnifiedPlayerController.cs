using UnityEngine;
using UnityEngine.Events;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Quaternion = UnityEngine.Quaternion;
using Polyperfect.Crafting.Demo;

[RequireComponent(typeof(CharacterController))]
public class UnifiedPlayerController : CommandablePlayer
{
    public override string __Usage => "A unified player controller that combines movement, camera, and interaction.";
    [Header("Movement Settings")]
    public float PrimarySpeed = 5f;
    public float RunSpeed = 9f;
    public float Acceleration = 20f;
    public float JumpSpeed = 12f;
    public float StickStrength = 3f;

    [Header("Camera Settings")]
    public float MouseSensitivity = 5f;
    public float PitchMax = 60f;
    public Transform PlayerCamera;

    [Header("Inputs")]
    public KeyCode MouseLockToggleKey = KeyCode.E;
    public string HorizontalAxis = "Horizontal";
    public string VerticalAxis = "Vertical";
    public string RunButton = "Fire3";
    public string JumpButton = "Jump";
    public string MouseX = "Mouse X";
    public string MouseY = "Mouse Y";

    [Header("Animator")]
    public Animator animator;

    CharacterController controller;
    Vector3 movementVelocity, physicsVelocity, hitNormal;
    float cameraPitch;
    bool isGrounded, lockCursor = true;

    new void Start()
    {
        base.Start();
        controller = GetComponent<CharacterController>();
        hitNormal = Vector3.up;
        cameraPitch = 0f;

        OnStartInteract.AddListener(e => lockCursor = false);
        OnStopInteract.AddListener(() => lockCursor = true);
    }

    new void Update()
    {
        base.Update();
        HandleMouseLook();
        HandleMovement();
        HandleCursorLock();
        UpdateAnimation();
    }

    void HandleMouseLook()
    {
        if (!lockCursor || PlayerCamera == null) return;

        float mouseX = Input.GetAxisRaw(MouseX) * MouseSensitivity;
        float mouseY = Input.GetAxisRaw(MouseY) * MouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -PitchMax, PitchMax);
        PlayerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        float speed = Input.GetButton(RunButton) ? RunSpeed : PrimarySpeed;

        var camForward = Vector3.ProjectOnPlane(PlayerCamera.forward, Vector3.up).normalized;
        var camRight = Vector3.ProjectOnPlane(PlayerCamera.right, Vector3.up).normalized;

        Vector3 inputVector = new Vector3(Input.GetAxisRaw(HorizontalAxis), 0f, Input.GetAxisRaw(VerticalAxis));
        Vector3 moveDirection = (camForward * inputVector.z + camRight * inputVector.x).normalized;

        physicsVelocity += Physics.gravity * Time.deltaTime;
        movementVelocity = Vector3.MoveTowards(movementVelocity, moveDirection * speed, Acceleration * Time.deltaTime);

        if (isGrounded)
            physicsVelocity = Physics.gravity.normalized * StickStrength;

        if (isGrounded && Input.GetButtonDown(JumpButton))
            physicsVelocity += -Physics.gravity.normalized * (JumpSpeed + StickStrength);

        var appliedVelocity = Quaternion.FromToRotation(Vector3.up, isGrounded ? hitNormal : Vector3.up) * movementVelocity + physicsVelocity;
        var flags = controller.Move(Time.deltaTime * appliedVelocity);
        isGrounded = (flags & CollisionFlags.Below) != 0;
    }

    void HandleCursorLock()
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;

        if (Input.GetKeyDown(MouseLockToggleKey))
            lockCursor = !lockCursor;
    }

    void UpdateAnimation()
    {
        if (!animator) return;

        float currentSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        animator.SetFloat("Speed", currentSpeed);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit)
        {
            hitNormal = hit.normal;
            physicsVelocity = Vector3.zero;
        }
    }

    public override void IssueCommand(ICommand command)
    {
        switch (command)
        {
            case InteractCommand interactCommand:
                InteractWith(interactCommand.Interactable);
                command.Complete();
                break;
            case PlaceCommand placeCommand:
                var placed = ExecutePlace(placeCommand.Info);
                if (placed)
                    placeCommand.Complete();
                else
                    placeCommand.Cancel();
                break;
            case StopCommand stopCommand:
                StopInteracting();
                stopCommand.Complete();
                break;
            default:
                command.Cancel();
                break;
        }
    }
}
