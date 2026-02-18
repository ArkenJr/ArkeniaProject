using UnityEngine;

/// <summary>
/// Teleports this GameObject to a designated spawn point during Awake(),
/// before physics, gravity, or any other script has run.
///
/// Why disable CharacterController first:
///   Unity's CharacterController caches its grounding state.  If you just
///   set transform.position while it is enabled, the controller can fight
///   the new position on the very next FixedUpdate, causing the player to
///   appear one frame below the surface or to clip through thin geometry.
///   Disabling → move → enabling gives it a clean slate.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SpawnAtPointOnStart : MonoBehaviour
{
    [Tooltip("Drag SpawnPoint_Rooftop (or any Transform) here.  " +
             "Leave empty to keep the object's current scene position.")]
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "SpawnAtPointOnStart: spawnPoint is not assigned — " +
                "player will start at its current scene position.", this);
            return;
        }

        var cc = GetComponent<CharacterController>();

        // Disable the controller so it doesn't resist the teleport.
        cc.enabled = false;
        transform.position = spawnPoint.position;
        cc.enabled = true;
    }
}
