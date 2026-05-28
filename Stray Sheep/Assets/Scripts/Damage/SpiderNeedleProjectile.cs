using UnityEngine;

public class SpiderNeedleProjectile : MonoBehaviour
{
    [Tooltip("Radius used to register contact damage against the current target.")]
    [SerializeField] private float hitRadius = 0.25f;

    [Tooltip("Position tolerance used to destroy the projectile after it reaches the owner.")]
    [SerializeField] private float despawnDistancePadding = 0.1f;

    private Transform owner;
    private Transform target;
    private Vector3 launchOrigin;
    private Vector3 launchDirection;
    private float damage;
    private float travelDistance;
    private float throwSpeed;
    private float returnSpeed;
    private float distanceTraveled;
    private bool returning;
    private bool launched;

    public void Launch(
        Transform owner,
        Transform target,
        float damage,
        float travelDistance,
        float throwSpeed,
        float returnSpeed)
    {
        this.owner = owner;
        this.target = target;
        this.damage = damage;
        this.travelDistance = travelDistance;
        this.throwSpeed = throwSpeed;
        this.returnSpeed = returnSpeed;

        launchOrigin = transform.position;
        launchDirection = target != null
            ? (target.position - launchOrigin).normalized
            : transform.forward;

        launchDirection.y = 0f;
        if (launchDirection.sqrMagnitude < 0.001f)
            launchDirection = transform.forward;

        transform.rotation = Quaternion.LookRotation(launchDirection);
        launched = true;
    }

    private void Update()
    {
        if (!launched) return;

        float stepSpeed = returning ? returnSpeed : throwSpeed;
        float stepDistance = stepSpeed * Time.deltaTime;

        if (!returning)
        {
            transform.position += launchDirection * stepDistance;
            distanceTraveled += stepDistance;

            if (target != null)
            {
                float hitDistance = Vector3.Distance(transform.position, target.position);
                if (hitDistance <= hitRadius)
                {
                    target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                    BeginReturn();
                    return;
                }
            }

            if (distanceTraveled >= travelDistance)
            {
                BeginReturn();
            }
        }
        else
        {
            if (owner == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 returnTarget = owner.position;
            returnTarget.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, returnTarget, stepDistance);

            if (Vector3.Distance(transform.position, returnTarget) <= despawnDistancePadding)
            {
                Destroy(gameObject);
            }
        }
    }

    private void BeginReturn()
    {
        returning = true;
        distanceTraveled = travelDistance;
    }
}