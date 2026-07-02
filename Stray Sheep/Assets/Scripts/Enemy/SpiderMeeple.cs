using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX;


public class SpiderMeeple : EnemyBase
{
    [Header("Needle Launch System")]
    [Tooltip("Projectile prefab spawned from the spider's needle launcher.")]
    [SerializeField] private SpiderNeedleProjectile needleProjectilePrefab;

    [Tooltip("Muzzle transform that defines the projectile spawn position and forward launch direction.")]
    [SerializeField] private Transform throwPoint;

    [Tooltip("Transform used as the spool hub for the retractable needle visual.")]
    [SerializeField] private Transform spoolVisual;

    [Header("Attack Parameters")]
    [Tooltip("Engagement radius in which the spider switches from locomotion to attack behavior.")]
    [SerializeField] private float spiderAttackRange = 8f;

    [Tooltip("Minimum interval between needle launches, in seconds.")]
    [SerializeField] private float throwCooldown = 1.75f;

    [Tooltip("Maximum outbound travel distance before the needle begins retraction.")]
    [SerializeField] private float needleTravelDistance = 6f;

    [Tooltip("Linear velocity of the outbound needle launch, in units per second.")]
    [SerializeField] private float needleThrowSpeed = 14f;

    [Tooltip("Linear velocity of the needle retraction, in units per second.")]
    [SerializeField] private float needleReturnSpeed = 18f;

    [Tooltip("Angular velocity applied to the spool visual, in degrees per second.")]
    [SerializeField] private float spoolSpinSpeed = 540f;

    protected override void Awake()
    {
        base.Awake();
        attackRange = spiderAttackRange;
    }

    protected override void Update()
    {
        if (PauseManager.IsPaused) return;
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackBehavior();
        }
        else
        {
            MoveBehavior();
        }

        AnimateSpool();
    }

    protected override void MoveBehavior()
    {
        if (playerTarget == null) return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
            return;
        }

        float step = 3f * Time.deltaTime;
        Vector3 targetPos = playerTarget.position;
        targetPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }

    protected override void AttackBehavior()
    {
        if (agent != null)
            agent.isStopped = true;

        if (Time.time < nextAttackTime) return;

        PerformAttack();
        nextAttackTime = Time.time + throwCooldown;
    }

    protected override void PerformAttack()
    {
        if (needleProjectilePrefab == null || throwPoint == null || playerTarget == null)
            return;

        Vector3 throwDirection = (playerTarget.position - throwPoint.position).normalized;
        throwDirection.y = 0f;

        if (throwDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(throwDirection);

        SpiderNeedleProjectile needle = Instantiate(
            needleProjectilePrefab,
            throwPoint.position,
            throwPoint.rotation
        );

        needle.Launch(
            owner: transform,
            target: playerTarget,
            damage: damage,
            travelDistance: needleTravelDistance,
            throwSpeed: needleThrowSpeed,
            returnSpeed: needleReturnSpeed
        );
    }

    private void AnimateSpool()
    {
        if (spoolVisual == null) return;

        spoolVisual.Rotate(Vector3.forward, spoolSpinSpeed * Time.deltaTime);
    }

    private void OnValidate()
    {
        spiderAttackRange = Mathf.Max(0.1f, spiderAttackRange);
        throwCooldown = Mathf.Max(0.1f, throwCooldown);
        needleTravelDistance = Mathf.Max(0.1f, needleTravelDistance);
        needleThrowSpeed = Mathf.Max(0.1f, needleThrowSpeed);
        needleReturnSpeed = Mathf.Max(0.1f, needleReturnSpeed);
        spoolSpinSpeed = Mathf.Max(0f, spoolSpinSpeed);

        if (!Application.isPlaying)
            attackRange = spiderAttackRange;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (throwPoint != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(throwPoint.position, throwPoint.position + transform.forward * needleTravelDistance);
        }
    }
}
