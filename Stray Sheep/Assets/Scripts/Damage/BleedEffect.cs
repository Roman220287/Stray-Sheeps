using UnityEngine;

public class BleedEffect : MonoBehaviour
{
    private float damagePerTick;
    private float tickInterval = 0.5f;
    private float duration;
    private float timeElapsed = 0f;
    private float nextTickTime = 0f;
    private EnemyBase enemy;

    public void Initialize(float damage, float bleedDuration)
    {
        damagePerTick = damage;
        duration = bleedDuration;
        enemy = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= duration)
        {
            Destroy(this);
            return;
        }

        if (Time.time >= nextTickTime && enemy != null)
        {
            enemy.TakeDamage(damagePerTick);
            Debug.Log($"Bleed tick: {damagePerTick} damage");
            nextTickTime = Time.time + tickInterval;
        }
    }
}
