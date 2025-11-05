using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person MMO-style controller using Unity Input System.
/// New name so it can't conflict with any old duplicates.
/// Requires a CharacterController on the same GameObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ArkenPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base movement speed when walking forward.")]
    public float moveSpeed = 5f;

    [Tooltip("Multiplier applied to the base speed when holding the sprint key.")]
    public float sprintMultiplier = 1.5f;

    [Tooltip("Acceleration blending between current speed and target speed.")]
    public float acceleration = 10f;

    [Header("Jump / Gravity")]
    [Tooltip("Gravity force (negative).")]
    public float gravity = -9.81f;

    [Tooltip("Jump height in meters.")]
    public float jumpHeight = 1.5f;

    [Header("Rotation")]
    [Tooltip("Degrees per second to rotate toward movement direction (lower = slower turn).")]
    public float rotationSpeedDegPerSec = 240f;

    [Tooltip("Ignore tiny inputs so the player doesn't twitch-turn.")]
    public float rotationInputDeadZone = 0.12f;

    [Header("Input (optional)")]
    [Tooltip("Optional InputActions; if not set, WASD/Space/Shift are created at runtime.")]
    public InputActionAsset inputActions;

    // internals
    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector2 currentMoveInput;
    private Vector3 velocity;
    private float currentSpeed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        SetupInputActions();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        sprintAction?.Disable();
    }

    private void Update()
    {
        ReadInput();
        ApplyMovement();
        ApplyGravityAndJump(); // one call, one method
    }

    private void SetupInputActions()
    {
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Player/Move", throwIfNotFound: false);
            jumpAction = inputActions.FindAction("Player/Jump", throwIfNotFound: false);
            sprintAction = inputActions.FindAction("Player/Sprint", throwIfNotFound: false);
        }

        if (moveAction == null)
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
        }

        if (jumpAction == null)
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");

        if (sprintAction == null)
            sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
    }

    private void ReadInput()
    {
        currentMoveInput = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPerformedThisFrame() && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
    }

    private void ApplyMovement()
    {
        // Local input XZ
        Vector3 move = new Vector3(currentMoveInput.x, 0f, currentMoveInput.y);
        move = transform.TransformDirection(move);

        float targetSpeed = moveSpeed;
        if (sprintAction.IsPressed())
            targetSpeed *= sprintMultiplier;

        float magnitude = Mathf.Clamp01(move.magnitude);
        targetSpeed *= magnitude;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        Vector3 movement = (magnitude > 0f ? move.normalized : Vector3.zero) * currentSpeed;
        characterController.Move(movement * Time.deltaTime);

        // Turn with fixed degrees-per-second (prevents super-snappy yaw)
        if (move != Vector3.zero && currentMoveInput.magnitude >= rotationInputDeadZone)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            float step = rotationSpeedDegPerSec * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
        }
    }

    private void ApplyGravityAndJump()
    {
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f; // keep grounded

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
