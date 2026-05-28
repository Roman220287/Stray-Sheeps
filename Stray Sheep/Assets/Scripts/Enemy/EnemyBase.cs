using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    [HideInInspector] public float maxHealth = 10f;
    [HideInInspector] public float damage = 1f;
    [HideInInspector] public float attackRange = 2.5f;
    [HideInInspector] public float attackCooldown = 1.5f;

    protected float currentHealth;
    protected float nextAttackTime;
    protected Transform playerTarget;
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Prevents player/explosions from physically pushing them
        rb.useGravity = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        if (NextLevelManager.instance != null)
            NextLevelManager.instance.RegisterEnemy();
    }

    protected virtual void Update()
    {
        // Override in subclasses
    }

    protected virtual void MoveBehavior()
    {
        // Override in subclasses
    }

    protected virtual void AttackBehavior()
    {
        // Override in subclasses
    }

    protected virtual void PerformAttack()
    {
        // Override in subclasses
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        NextLevelManager.instance.UnregisterEnemy();
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
    }
}