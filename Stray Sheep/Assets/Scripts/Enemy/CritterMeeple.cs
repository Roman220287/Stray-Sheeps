using UnityEngine;
using UnityEngine.AI;
public class CritterMeeple : EnemyBase
{
    private float maxHealth = 10f;
    private float damage = 1f;
    private float attackRange = 2.5f;
    private float attackCooldown = 1.5f;
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
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
    }

    protected override void AttackBehavior()
    {
        agent.isStopped = true;

        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    protected override void PerformAttack()
    {
        Vector3 lookDir = (playerTarget.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

        playerTarget.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }
}