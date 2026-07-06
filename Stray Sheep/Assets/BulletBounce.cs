using UnityEngine;

public class BulletBounce : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 lastVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Only update if the bullet is actually moving to avoid zero-length vectors
        if (rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            lastVelocity = rb.linearVelocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            // Safety Check: Ensure we have a valid direction to raycast
            if (lastVelocity.sqrMagnitude < 0.001f) return;

            // Define the ray using the cached velocity
            Ray ray = new Ray(transform.position, -lastVelocity.normalized);

            if (other.Raycast(ray, out RaycastHit hit, 2f))
            {
                // Calculate reflection
                Vector3 reflectDir = Vector3.Reflect(lastVelocity.normalized, hit.normal);

                // Apply velocity
                rb.linearVelocity = reflectDir * lastVelocity.magnitude;

                // Update rotation
                transform.forward = reflectDir;

                // Nudge out
                transform.position = hit.point + (hit.normal * 0.05f);
            }
        }
    }
}