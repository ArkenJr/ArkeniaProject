using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Provides MMO-style character movement with smooth acceleration, sprinting, and camera-relative input.
/// Requires a CharacterController component on the same GameObject and optional InputAction references.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ArkenPlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Walking speed in meters per second.")]
    public float walkSpeed = 3.5f;

    [Tooltip("Multiplier applied to walkSpeed while sprinting.")]
    public float sprintMultiplier = 1.75f;

    [Tooltip("Seconds the input takes to fully accelerate/decelerate.")]
    public float inputSmoothingTime = 0.12f;

    [Header("Rotation")]
    [Tooltip("Maximum turning speed in degrees per second.")]
    public float maxTurnSpeed = 180f;

    [Header("References")]
    [Tooltip("Camera transform used to translate input into world-space movement.")]
    public Transform cameraTransform;

    [Tooltip("Optional InputAction that supplies a Vector2 (WASD / left stick) value.")]
    public InputActionReference moveAction;

    [Tooltip("Optional InputAction used to detect sprinting (typically bound to Left Shift).")]
    public InputActionReference sprintAction;

    private CharacterController characterController;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (sprintAction != null)
        {
            sprintAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (sprintAction != null)
        {
            sprintAction.action.Disable();
        }
    }

    private void Update()
    {
        Vector2 targetInput = ReadMovementInput();
        smoothedInput = Vector2.SmoothDamp(smoothedInput, targetInput, ref inputVelocity, inputSmoothingTime);

        bool isMoving = smoothedInput.sqrMagnitude > 0.0001f;
        Vector3 moveDirection = Vector3.zero;
        if (isMoving)
        {
            moveDirection = CalculateWorldSpaceDirection(smoothedInput);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.deltaTime);
        }

        float currentSpeed = walkSpeed;
        if (IsSprinting())
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        ApplyGravity();

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(finalVelocity * Time.deltaTime);
    }

    private Vector2 ReadMovementInput()
    {
        if (moveAction != null)
        {
            return moveAction.action.ReadValue<Vector2>();
        }

        float horizontal = Keyboard.current != null ? ReadKeyAxis(Keyboard.current.aKey, Keyboard.current.dKey) : Input.GetAxisRaw("Horizontal");
        float vertical = Keyboard.current != null ? ReadKeyAxis(Keyboard.current.sKey, Keyboard.current.wKey) : Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }

    private static float ReadKeyAxis(KeyControl negative, KeyControl positive)
    {
        float value = 0f;
        if (negative != null && negative.isPressed)
        {
            value -= 1f;
        }

        if (positive != null && positive.isPressed)
        {
            value += 1f;
        }

        return value;
    }

    private Vector3 CalculateWorldSpaceDirection(Vector2 input)
    {
        Transform reference = cameraTransform != null ? cameraTransform : transform;
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;
        return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
    }

    private bool IsSprinting()
    {
        if (sprintAction != null)
        {
            return sprintAction.action.IsPressed();
        }

        return Keyboard.current != null ? Keyboard.current.leftShiftKey.isPressed : Input.GetKey(KeyCode.LeftShift);
    }

    private void ApplyGravity()
    {
        const float gravity = -9.81f;
        const float groundedGravity = -2f;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }
}
