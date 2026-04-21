using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickUpBase : MonoBehaviour
{
    public enum StatType
    {
        Damage,
        FireRate,
        BulletSpeed,
        MoveSpeed,
        MaxHealth
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
        if (other.CompareTag("Player"))
        {
            PlayerStatsBase playerStats = other.GetComponent<PlayerStatsBase>();
            if (playerStats != null)
            {
                ApplyPickup(playerStats);
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
    protected virtual void ApplyPickup(PlayerStatsBase stats)
    {
        float multiplier = percentageIncrease / 100f;
        switch (statToModify)
        {
            case StatType.Damage:
                stats.baseDamage += (stats.baseDamage * multiplier);
                Debug.Log($"Damage increased by {percentageIncrease}%! New Damage: {stats.baseDamage}");
                break;

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
        }
    }
}
