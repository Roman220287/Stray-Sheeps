using UnityEngine;

//     fix burst shot not spreading out properly,
//     duration of slow effect needs to be duration for both slow and bleed,




[RequireComponent(typeof(Collider))]
public class PickUpBase : MonoBehaviour
{
    public enum StatType
    {
        WeaponDamage,
        FireRate,
        BulletSpeed,
        MoveSpeed,
        MaxHealth,
        ExtraBounce,
        Burst,
        BleedingDamage,
        SlowingEnemies,
        AttackSpeed
    }

    [Header("Pickup Settings")]
    [Tooltip("What stat does this pickup modify?")]
    public StatType statToModify;

    [Tooltip("The percentage to increase the stat by (e.g., 10 for 10%)")]
    public float percentageIncrease = 10f;

    [Header("Effects")]
    public GameObject pickupEffect;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerStatsBase playerStats = other.GetComponent<PlayerStatsBase>();
        if (playerStats == null) return;

        ApplyPickup(playerStats);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    protected virtual void ApplyPickup(PlayerStatsBase stats)
    {
        float multiplier = percentageIncrease / 100f;
        switch (statToModify)
        {
            case StatType.FireRate:
                stats.fireRateDelay -= (stats.fireRateDelay * multiplier);
                Debug.Log($"Fire Rate increased! New Delay: {stats.fireRateDelay}");
                break;

            case StatType.BulletSpeed:
                stats.bulletSpeed += (stats.bulletSpeed * multiplier);
                Debug.Log($"Bullet Speed increased by {percentageIncrease}%! New Speed: {stats.bulletSpeed}");
                break;

            case StatType.MoveSpeed:
                PlayerBase controller = stats.GetComponent<PlayerBase>();
                if (controller != null)
                {
                    controller.moveSpeed += (controller.moveSpeed * multiplier);
                    Debug.Log($"Move Speed increased by {percentageIncrease}%! New Speed: {controller.moveSpeed}");
                }
                break;

            case StatType.MaxHealth:
                float healthIncrease = stats.maxHealth * multiplier;
                stats.maxHealth += healthIncrease;
                stats.currentHealth += healthIncrease;

                Debug.Log($"Max Health increased by {percentageIncrease}% ({healthIncrease} HP). New Max: {stats.maxHealth}");
                break;

            case StatType.ExtraBounce:
                stats.bounceCount += (int)(1 * (1 + multiplier));
                Debug.Log($"Extra Bounce added! Total Bounces: {stats.bounceCount}");
                break;

            case StatType.Burst:
                stats.burstShots += (int)(1 * (1 + multiplier));
                Debug.Log($"Burst upgraded! Total Burst Shots: {stats.burstShots}");
                break;

            case StatType.BleedingDamage:
                stats.bleedingDamage += (1f * multiplier);
                Debug.Log($"Bleeding Damage added! Bleed Damage per tick: {stats.bleedingDamage}");
                break;

            case StatType.SlowingEnemies:
                stats.slowDuration += (1f * multiplier);
                Debug.Log($"Enemy Slowing effect upgraded! Slow Duration: {stats.slowDuration}s");
                break;

            case StatType.AttackSpeed:
                stats.fireRateDelay -= (stats.fireRateDelay * multiplier);
                Debug.Log($"Attack Speed increased! New Delay: {stats.fireRateDelay}");
                break;

            case StatType.WeaponDamage:
                stats.baseDamage += (stats.baseDamage * multiplier);
                Debug.Log($"Weapon Damage increased by {percentageIncrease}%! New Damage: {stats.baseDamage}");
                break;
        }
    }
}
