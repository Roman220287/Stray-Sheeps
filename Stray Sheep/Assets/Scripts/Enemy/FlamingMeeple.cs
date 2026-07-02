using UnityEngine;

public class FlamingMeeple : EnemyBase
{
    [Header("Flaming Meeple")]
    public GameObject matchObject;
    public ParticleSystem fireEffect;
    public float baseMovementSpeed = 3.5f;
    public float speedMultiplierOnFire = 1.75f;
    public float burnDuration = 8f;

    private bool isIgnited;
    private float ignitionTime;
    private Animator mAnimator;

    protected override void Awake()
    {
        base.Awake();

        if (agent != null)
            agent.speed = baseMovementSpeed;

        if (fireEffect != null)
            fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    protected override void Update()
    {
        if (PauseManager.IsPaused) return;

        if (!isIgnited && playerTarget != null)
        {
            Ignite();
        }

        if (isIgnited && Time.time >= ignitionTime + burnDuration)
        {
            Die();
            return;
        }

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

    private void Ignite()
    {
        if (isIgnited) return;

        isIgnited = true;
        ignitionTime = Time.time;

        if (matchObject != null && matchObject != gameObject)
            matchObject.SetActive(false);

        if (fireEffect != null)
            fireEffect.Play();

        if (agent != null)
            agent.speed = baseMovementSpeed * speedMultiplierOnFire;
    }

    protected override void MoveBehavior()
    {
        if (agent == null || !agent.isOnNavMesh || playerTarget == null) return;

        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
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
        Vector3 lookDir = (playerTarget.position - transform.position).normalized;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (mAnimator == null) mAnimator = GetComponent<Animator>();
        mAnimator.SetTrigger("Attack");
        StartCoroutine(DelayDamage(0.5f)); // 0.5 seconds delay
    }

    private System.Collections.IEnumerator DelayDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerTarget.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (!isIgnited) return;

        float burnTimeRemaining = burnDuration - (Time.time - ignitionTime);
        float burnProgress = Mathf.Clamp01(burnTimeRemaining / burnDuration);

        Gizmos.color = Color.Lerp(Color.red, Color.yellow, burnProgress);
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        if (burnTimeRemaining > 0)
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * burnTimeRemaining);
    }
}
