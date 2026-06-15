using UnityEngine;

public class BlowpipeMeeple : EnemyBase
{
    [Header("Blowpipe Settings")]
    [Tooltip("Prefab used as the knalerwt (snap pea) projectile.")]
    [SerializeField] private GameObject blowpipeProjectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shortStrawRange = 10f;
    [SerializeField] private float shootInterval = 0.65f;

    private float shootTimer;
    private bool wasPlayerInRange;

    protected override void Awake()
    {
        base.Awake();
        attackRange = shortStrawRange;
        shootTimer = shootInterval;
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

        // Fallback: simple direct movement when NavMeshAgent is missing or not on a NavMesh
        Debug.Log("BlowpipeMeeple MoveBehavior: using fallback movement (no NavMeshAgent or not on NavMesh).");
        float step = (3f * Time.deltaTime); // fallback speed (tweakable)
        Vector3 targetPos = playerTarget.position;
        targetPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }


    protected override void Update()
    {
        if (PauseManager.IsPaused) return;

        base.Update();

        bool playerInRange = IsPlayerInRange();

        if (playerInRange)
        {
            if (agent != null)
                agent.isStopped = true;

            if (!wasPlayerInRange)
            {
                // Fire shortly after the player enters range so this enemy feels active.
                shootTimer = Mathf.Min(shootTimer, 0.15f);
            }

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }
        else
        {
            shootTimer = shootInterval; // Reset timer when player is out of range
            MoveBehavior();
        }

        wasPlayerInRange = playerInRange;
    }

    private void Shoot()
    {
        if (blowpipeProjectilePrefab == null || shootPoint == null) return;

        Vector3 shootDirection = transform.forward;
        if (playerTarget != null)
        {
            shootDirection = (playerTarget.position - shootPoint.position).normalized;
            shootDirection.y = 0f;
        }

        if (shootDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(shootDirection);

        Quaternion projectileRotation = shootDirection.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(shootDirection)
            : shootPoint.rotation;

        Instantiate(blowpipeProjectilePrefab, shootPoint.position, projectileRotation);
    }

    private bool IsPlayerInRange()
    {
        if (playerTarget == null) return false;
        return Vector3.Distance(transform.position, playerTarget.position) <= attackRange;
    }

    private void OnValidate()
    {
        shortStrawRange = Mathf.Max(0.1f, shortStrawRange);
        shootInterval = Mathf.Max(0.1f, shootInterval);

        if (!Application.isPlaying)
            attackRange = shortStrawRange;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
