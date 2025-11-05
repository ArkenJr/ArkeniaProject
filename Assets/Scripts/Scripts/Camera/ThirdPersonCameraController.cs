using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform Target;

    [Header("Framing")]
    public float FollowDistance = 5.5f;
    public float HeightOffset = 2.0f;

    [Header("Smoothing (0 = snap, higher = smoother)")]
    public float PositionDamping = 0.18f;
    public float RotationDamping = 0.12f;

    private Vector3 _vel;

    void LateUpdate()
    {
        if (!Target) return;

        // desired camera position behind target
        Vector3 desiredPos = Target.position
                           - Target.forward * FollowDistance
                           + Vector3.up * HeightOffset;

        // smooth position (exponential-like)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, PositionDamping);

        // look at the target smoothly
        var desiredRot = Quaternion.LookRotation(Target.position + Vector3.up * 1.0f - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-RotationDamping * Time.deltaTime));
    }
}
