using UnityEngine;

/// <summary>
/// Constant-speed third-person follow camera.
/// Follows the target at a fixed distance and height and rotates at a fixed angular speed.
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform Target;

    [Header("Framing")]
    public float FollowDistance = 5.5f;
    public float HeightOffset = 2.0f;

    [Header("Speeds")]
    public float PositionSpeed = 5.0f;           // units / sec
    public float RotationSpeedDegPerSec = 180f;  // deg / sec

    void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desiredPos = Target.position
                           - Target.forward * FollowDistance
                           + Vector3.up * HeightOffset;

        transform.position = Vector3.MoveTowards(
            transform.position, desiredPos, PositionSpeed * Time.deltaTime);

        Vector3 lookPoint = Target.position + Vector3.up * 1.0f;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, desiredRot, RotationSpeedDegPerSec * Time.deltaTime);
    }
}
