using UnityEngine;

public class PlayerStatsBase : MonoBehaviour
{
    [Header("Combat Stats")]
    public float baseDamage = 10f;
    public float fireRateDelay = 0.5f; // Time between shots in seconds
    public float bulletSpeed = 15f;

    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }
}
