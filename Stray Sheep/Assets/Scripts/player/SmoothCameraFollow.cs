using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Tooltip("The tag assigned to your player GameObject.")]
    [SerializeField] private string playerTag = "Player";

    /* [Header("Isometric Settings")]
    [Tooltip("Isometric offset from the player's position (world units).")]
    [SerializeField]*/
    private Vector3 isometricOffset = new Vector3(0f, 20f, -7f);

    [Tooltip("Additional positional offset applied after the isometric offset.")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    [Tooltip("Fixed camera rotation in Euler angles (degrees). Set this for a top-down/isometric view.")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(69f, 0f, 0f);

    [Tooltip("How long the camera takes to catch up to the player's position. Smaller = snappier.")]
    [SerializeField] private float followSmoothTime = 0.25f;

    [Header("Behavior")]
    [Tooltip("If true, the camera smoothly follows the player's position; rotation stays fixed.")]
    [SerializeField] private bool followPosition = true;

    private Transform target;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
            // snap to initial position/rotation to avoid big jumps on start
            transform.position = target.position + isometricOffset + positionOffset;
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else
        {
            Debug.LogError($"SmoothCameraFollow: Could not find a GameObject with the tag '{playerTag}'.");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Position following (smoothed)
        if (followPosition)
        {
            Vector3 desiredPos = target.position + isometricOffset + positionOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, followSmoothTime);
        }

        // Keep rotation fixed for a true isometric/top-down feel
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}
