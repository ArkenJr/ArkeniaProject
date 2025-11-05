using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls third-person MMO style character movement using the new Unity Input System.
/// Requires a CharacterController component on the same GameObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MMOPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed when walking forward.")]
    public float moveSpeed = 5f;

    [Tooltip("Multiplier applied to the base speed when holding the sprint key.")]
    public float sprintMultiplier = 1.5f;

    [Tooltip("Acceleration applied when blending between movement speeds.")]
    public float acceleration = 10f;

    [Tooltip("Gravity force applied to keep the character grounded.")]
    public float gravity = -9.81f;

    [Tooltip("Height of the jump impulse when the jump action is triggered.")]
    public float jumpHeight = 1.5f;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second to rotate toward movement direction.")]
    public float rotationSpeedDegPerSec = 360f;

    [Tooltip("Ignore tiny inputs so the player doesn't twitch-turn.")]
    public float rotationInputDeadZone = 0.12f;

    [Header("Input Actions")]
    [Tooltip("Optional reference to an InputAction asset. Leave empty to let the script create actions at runtime.")]
    public InputActionAsset inputActions;

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
        ApplyGravityAndJump();
    }

    /// <summary>
    /// Initializes the move, jump, and sprint actions. If an InputActionAsset is provided, it attempts to use named actions.
    /// Otherwise, the actions are created programmatically and use the WASD keys along with Space and LeftShift.
    /// </summary>
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
        {
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }

        if (sprintAction == null)
        {
            sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
        }
    }

    /// <summary>
    /// Reads movement, jump, and sprint values from the Input System.
    /// </summary>
    private void ReadInput()
    {
        currentMoveInput = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPerformedThisFrame() && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
    }

    /// <summary>
    /// Applies WASD movement with smooth acceleration to emulate MMO controls,
    /// and rotates using a fixed degrees-per-second turn rate to avoid snappy yaw.
    /// </summary>
    private void ApplyMovement()
    {
        // Local-space move vector (relative to the character)
        Vector3 move = new Vector3(currentMoveInput.x, 0f, currentMoveInput.y);
        move = transform.TransformDirection(move);

        // Speed calc with sprint
        float targetSpeed = moveSpeed;
        if (sprintAction.IsPressed())
        {
            targetSpeed *= sprintMultiplier;
        }

        float magnitude = Mathf.Clamp01(move.magnitude);
        targetSpeed *= magnitude;

        // Smooth blend to target speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);

        // Apply movement
        Vector3 movement = (magnitude > 0f ? move.normalized : Vector3.zero) * currentSpeed;
        characterController.Move(movement * Time.deltaTime);

        // Damped rotation toward move direction (if input is meaningful)
        if (move != Vector3.zero)
        {
            float mag = currentMoveInput.magnitude;
            if (mag >= rotationInputDeadZone)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
                float step = rotationSpeedDegPerSec * Time.deltaTime; // degrees per second
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
            }
        }
    }

    /// <summary>
    /// Applies gravity each frame and ensures the character sticks to the ground when grounded.
    /// </summary>
    private void ApplyGravityAndJump()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
