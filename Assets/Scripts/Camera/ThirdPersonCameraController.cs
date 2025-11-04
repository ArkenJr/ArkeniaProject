using UnityEngine;

/// <summary>
/// Positions the camera behind the player and keeps it smoothly aligned with the movement direction.
/// Attach this script to the Camera object and assign the player transform as the target.
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [Tooltip("Reference to the player transform that the camera should follow.")]
    public Transform target;

    [Tooltip("Offset from the target position for third-person framing.")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Tooltip("How quickly the camera follows the player position.")]
    public float followSmoothTime = 0.1f;

    [Tooltip("Speed at which the camera rotates to align behind the player.")]
    public float rotationSmoothTime = 0.2f;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime / rotationSmoothTime);
    }
}
