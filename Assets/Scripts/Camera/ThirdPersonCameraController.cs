using UnityEngine;

/// <summary>
/// Smoothly positions a third-person camera behind the target while keeping focus on the character.
/// Uses exponential damping so behaviour remains consistent regardless of frame rate fluctuations.
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Reference to the player transform that the camera should follow.")]
    public Transform target;

    [Header("Positioning")]
    [Tooltip("Horizontal distance maintained behind the target.")]
    public float followDistance = 6f;

    [Tooltip("Vertical offset applied to the camera position.")]
    public float heightOffset = 3f;

    [Tooltip("How quickly the camera position catches up to the target. Larger values mean snappier movement.")]
    public float positionDamping = 6f;

    [Header("Orientation")]
    [Tooltip("Height offset to look towards on the target.")]
    public float lookAtHeight = 1.5f;

    [Tooltip("How quickly the camera rotates to face the target. Larger values mean faster rotation.")]
    public float rotationDamping = 12f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();

        Vector3 desiredPosition = target.position - forward * followDistance + Vector3.up * heightOffset;
        if (!IsFinite(desiredPosition))
        {
            return;
        }

        float positionLerpFactor = 1f - Mathf.Exp(-positionDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionLerpFactor);

        Vector3 lookTarget = target.position + Vector3.up * lookAtHeight;
        Vector3 lookDirection = lookTarget - transform.position;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            lookDirection = transform.forward;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        float rotationLerpFactor = 1f - Mathf.Exp(-rotationDamping * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerpFactor);
    }

    private static bool IsFinite(Vector3 value)
    {
        return !(float.IsNaN(value.x) || float.IsInfinity(value.x)
            || float.IsNaN(value.y) || float.IsInfinity(value.y)
            || float.IsNaN(value.z) || float.IsInfinity(value.z));
    }
}
