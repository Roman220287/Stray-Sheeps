using UnityEngine;

public class AttackInstance : MonoBehaviour
{
    [Header("Base Stats")]
    public float damage = 10f;
    public float lifetime = 1.5f;
    public float speed = 15f;
    public bool isProjectile = true;

    [Header("Status Effects")]
    public int bounceCount = 0; 
    public float bleedingDamage = 0f;
    public float slowDuration = 0f;
    public float slowAmount = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject enemyHitVFX;
    [SerializeField] private GameObject wallBounceVFX;

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

    private void HandleEnvironmentBounce()
    {
        float castDistance = (speed * Time.deltaTime) + 0.15f;

        if (!Physics.Raycast(transform.root.position, transform.root.forward, out RaycastHit hitInfo, castDistance))
            return;

        if (hitInfo.collider.CompareTag("Enemy"))
            return;

        if (!hitInfo.collider.CompareTag("Wall") && !hitInfo.collider.CompareTag("Environment"))
            return;

        if (bouncesRemaining <= 0)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        bouncesRemaining--;
        
        // Pass the hit point and surface normal to the bounce method
        BounceProjectile(hitInfo.point, hitInfo.normal);

        transform.root.position = hitInfo.point + hitInfo.normal * 0.1f;
        Debug.Log($"Bounced off environment! Bounces remaining: {bouncesRemaining}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        Debug.Log("Hit Enemy");
        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        
        // SOLVED: Spawns the enemy hit VFX at the projectile's position facing back out
        if (enemyHitVFX != null)
        {
            Instantiate(enemyHitVFX, transform.position, Quaternion.LookRotation(-transform.forward));
        }

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

    private void BounceProjectile(Vector3 hitPoint, Vector3 normal)
    {
        // SOLVED: Spawns the bounce VFX at the point of impact, oriented along the wall's normal
        if (wallBounceVFX != null)
        {
            Instantiate(wallBounceVFX, hitPoint, Quaternion.LookRotation(normal));
        }
        
        Vector3 newDirection = Vector3.Reflect(transform.root.forward, normal);
        transform.root.rotation = Quaternion.LookRotation(newDirection);
    }
}