using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerStatsBase : MonoBehaviour
{
    [Header("Combat Stats")]
    public float baseDamage = 1f;
    public float fireRateDelay = 0.5f; // Time between shots in seconds
    public float bulletSpeed = 15f;

    [Header("Health Stats")]
    public float maxHealth = 3f;
    public float currentHealth;

    [Header("Weapon Upgrades")]
    public int bounceCount = 0;
    public int burstShots = 1;
    public float bleedingDamage = 0f; // Damage per tick
    public float slowDuration = 0f;
    public float slowAmount = 0.5f; // Multiplier (0.5 = 50% slower)

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("Title Screen");
    }
}
