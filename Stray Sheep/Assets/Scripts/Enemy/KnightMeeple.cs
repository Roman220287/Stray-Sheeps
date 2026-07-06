using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;


public class KnightMeeple : EnemyBase
{
    [Header("Parts")]
    [SerializeField] private GameObject shieldObject;
    [SerializeField] private GameObject spearObject;

    [Header("Shield Settings")]
    [Tooltip("Whether the meeple starts with its shield raised.")]
    [SerializeField] private bool shieldUpOnStart = true;

    private bool shieldActive;
    private Animator mAnimator;

    protected override void Awake()
    {
        base.Awake();
        shieldActive = shieldUpOnStart;
        if (shieldObject != null)
            shieldObject.SetActive(shieldActive);
        if (spearObject != null)
            spearObject.SetActive(true);

        Debug.Log($"KnightMeeple Awake: agent={(agent != null)} isOnNavMesh={(agent != null ? agent.isOnNavMesh.ToString() : "N/A")}, shieldActive={shieldActive}");
    }

    // Overrides base damage handling to implement shield blocking
    // and back-hit instant kills.
    public override void TakeDamage(float amount)
    {
        // Determine attacker direction using the stored player target when available.
        Vector3 attackerDir = Vector3.zero;
        if (playerTarget != null)
            attackerDir = (playerTarget.position - transform.position).normalized;

        float forwardDot = Vector3.Dot(transform.forward, attackerDir);

        bool hitFromFront = forwardDot >= 0.3f;
        bool hitFromBack = forwardDot <= -0.3f;

        // If the shield is up and the attack comes from the front, block and drop shield.
        if (shieldActive && hitFromFront)
        {
            shieldActive = false;
            if (shieldObject != null)
                shieldObject.SetActive(false);
            return; // attack blocked
        }

        // Back hits always count as an instant kill.
        if (hitFromBack)
        {
            Die();
            return;
        }

        // Otherwise, take normal damage.
        base.TakeDamage(amount);
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
        Debug.Log("KnightMeeple MoveBehavior: using fallback movement (no NavMeshAgent or not on NavMesh).");
        float step = (3f * Time.deltaTime); // fallback speed (tweakable)
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
        nextAttackTime = Time.time + attackCooldown;
    }

    protected override void PerformAttack()
    {
        if (playerTarget == null) return;

        Vector3 lookDir = (playerTarget.position - transform.position).normalized;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

        if (mAnimator == null) mAnimator = GetComponent<Animator>();
        mAnimator.SetTrigger("Attack");
        StartCoroutine(DelayDamage(0.5f)); // 0.5 seconds delay
    }

    private System.Collections.IEnumerator DelayDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerTarget != null)
            playerTarget.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    protected override void Die()
    {
        if (deathEffectPrefab != null)
        {
            VisualEffect vfxInstance = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            vfxInstance.Play();
            
            Destroy(vfxInstance.gameObject, 3f); 
        }
        base.Die();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Shield state indicator
        Gizmos.color = shieldActive ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.2f);

        // Visualize front/back hit cones used by TakeDamage
        float frontDot = 0.3f;
        float backDot = -0.3f;

        float frontAngle = Mathf.Acos(frontDot) * Mathf.Rad2Deg;
        float backAngle = Mathf.Acos(backDot) * Mathf.Rad2Deg;

        // Front cone (yellow)
        Gizmos.color = Color.yellow;
        Vector3 leftFront = Quaternion.Euler(0f, frontAngle, 0f) * transform.forward;
        Vector3 rightFront = Quaternion.Euler(0f, -frontAngle, 0f) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftFront * 1.5f);
        Gizmos.DrawLine(transform.position, transform.position + rightFront * 1.5f);

        // Back cone (cyan)
        Gizmos.color = Color.cyan;
        float backConeAngle = 180f - backAngle;
        Vector3 leftBack = Quaternion.Euler(0f, backConeAngle, 0f) * transform.forward;
        Vector3 rightBack = Quaternion.Euler(0f, -backConeAngle, 0f) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftBack * 1.5f);
        Gizmos.DrawLine(transform.position, transform.position + rightBack * 1.5f);
    }
}
