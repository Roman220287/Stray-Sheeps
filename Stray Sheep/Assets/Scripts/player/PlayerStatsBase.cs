using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
public class PlayerStatsBase : MonoBehaviour
{
    public static PlayerStatsBase Instance { get; private set; }
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
    [Header("References")]
    [SerializeField] private VisualEffect hitEffect; // Drag your VFX prefab/object here in Inspector
    private HeartUI heartUI;
    private SmoothCameraFollow cameraFollow;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        // Cache references once at start to save performance
        cameraFollow = FindFirstObjectByType<SmoothCameraFollow>();
        heartUI = FindFirstObjectByType<HeartUI>();

        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (cameraFollow != null) cameraFollow.Shake(0.05f, 0.5f);

        // Play VFX
        if (hitEffect != null) hitEffect.Play();

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (heartUI != null) heartUI.UpdateHearts((int)currentHealth);
    }

    private void Die()
    {
        // Optional: Trigger death animation or effects here
        SceneManager.LoadScene("Title Screen");
        Destroy(gameObject);
        NextLevelManager.instance.ResetGameEntirely();
    }
}
