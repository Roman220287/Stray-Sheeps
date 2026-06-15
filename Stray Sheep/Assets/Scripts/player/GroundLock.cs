using UnityEngine;

public class GroundLock : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer; // Set this to your Ground/Environment layer
    [SerializeField] private float raycastDistance = 1.5f; // Adjust based on player height
    [SerializeField] private float groundOffset = 0f; // Fine-tune player feet position

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Shoot a ray straight down from the center of the player
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            // Calculate exactly where the player's feet should be
            float targetY = hit.point.y + groundOffset;

            // 1. Force the Rigidbody position to stay on the ground
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = targetY;
            transform.position = clampedPosition;

            // 2. Kill any upward physics velocity instantly
            if (rb != null && rb.linearVelocity.y > 0)
            {
                Vector3 flatVelocity = rb.linearVelocity;
                flatVelocity.y = 0;
                rb.linearVelocity = flatVelocity;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the raycast in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 0.1f + Vector3.down * raycastDistance);
    }
}
