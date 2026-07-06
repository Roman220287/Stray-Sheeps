using UnityEngine;

public class AttackInstance : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 1.5f;
    public float speed = 15f;
    public bool isProjectile = true;

    // Upgrade stats
    public int bounceCount = 1; // Make sure this is 1 or higher in the inspector!
    public float bleedingDamage = 0f;
    public float slowDuration = 0f;
    public float slowAmount = 0.5f;

    private int bouncesRemaining = -1;
    private Vector3 lastDirection;

    private void Start()
    {
        lastDirection = transform.root.forward;
        Destroy(transform.root.gameObject, lifetime);
    }

    private void Update()
    {
        if (!isProjectile) return;

        if (bouncesRemaining == -1)
            bouncesRemaining = bounceCount;

        HandleEnvironmentBounce();
        transform.root.Translate(transform.root.forward * speed * Time.deltaTime, Space.World);
    }

    // --- REPLACE THIS METHOD ---
    private void HandleEnvironmentBounce()
    {
        float castDistance = (speed * Time.deltaTime) + 0.15f;

        if (!Physics.Raycast(transform.root.position, transform.root.forward, out RaycastHit hitInfo, castDistance))
            return;

        if (hitInfo.collider.CompareTag("Enemy"))
            return;

        // Checks for either "Wall" or "Environment" tags
        if (!hitInfo.collider.CompareTag("Wall") && !hitInfo.collider.CompareTag("Environment"))
            return;

        if (bouncesRemaining <= 0)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        bouncesRemaining--;
        BounceProjectile(hitInfo.normal);

        // This teleports the bullet slightly away from the wall so it doesn't clip inside
        transform.root.position = hitInfo.point + hitInfo.normal * 0.1f;
        Debug.Log($"Bounced off environment! Bounces remaining: {bouncesRemaining}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Debug.Log("Hit Enemy");
        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (bleedingDamage > 0f)
        {
            BleedEffect bleed = other.gameObject.AddComponent<BleedEffect>();
            bleed.Initialize(bleedingDamage, 3f);
        }

        if (slowDuration > 0f)
        {
            SlowEffect slow = other.gameObject.AddComponent<SlowEffect>();
            slow.Initialize(slowDuration, slowAmount);
        }

        Destroy(transform.root.gameObject);
    }

    // --- REPLACE THIS METHOD ---
    private void BounceProjectile(Vector3 normal)
    {
        Vector3 newDirection = Vector3.Reflect(transform.root.forward, normal);
        transform.root.rotation = Quaternion.LookRotation(newDirection);
    }
}