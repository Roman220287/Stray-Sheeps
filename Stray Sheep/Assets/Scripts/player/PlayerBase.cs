using UnityEngine;
using UnityEngine.InputSystem;

// fix the dash not far enough, maybe add a dash upgrade that increases dash distance and/or reduces cooldown?-

public class PlayerBase : MonoBehaviour
{
    private InputSystem_Actions controls;
    private CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 10f;

    [Header("Attack Settings")]
    public GameObject attackPrefab;
    public Transform attackPoint;
    public float attackCooldown = 0.4f;
    private float nextAttackTime = 0f;

    private Vector2 moveInput;
    private Vector3 currentLookDirection;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();

        controls.Player.Attack.performed += ctx => PerformAttack();
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        if (Gamepad.current == null || Mouse.current.wasUpdatedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                targetDirection = (point - transform.position).normalized;
            }
        }
        else
        {
            Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
            if (lookInput.sqrMagnitude > 0.1f)
            {
                targetDirection = new Vector3(lookInput.x, 0, lookInput.y);
            }
        }

        if (targetDirection != Vector3.zero)
        {
            targetDirection.y = 0;
            currentLookDirection = targetDirection;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void PerformAttack()
    {
        if (Time.time < nextAttackTime) return;

        if (currentLookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(currentLookDirection);

        PlayerStatsBase playerStats = GetComponent<PlayerStatsBase>();
        int burstCount = playerStats != null ? playerStats.burstShots : 1;

        for (int i = 0; i < burstCount; i++)
            FireProjectile(i, burstCount, playerStats);

        nextAttackTime = Time.time + attackCooldown;
    }

    private void FireProjectile(int burstIndex, int burstCount, PlayerStatsBase playerStats)
    {
        if (attackPrefab == null || attackPoint == null) return;

        GameObject projectile = Instantiate(attackPrefab, attackPoint.position, transform.rotation);
        AttackInstance attackInstance = projectile.GetComponentInChildren<AttackInstance>();

        if (attackInstance != null && playerStats != null)
        {
            attackInstance.damage = playerStats.baseDamage;
            attackInstance.speed = playerStats.bulletSpeed;
            attackInstance.bounceCount = playerStats.bounceCount;
            attackInstance.bleedingDamage = playerStats.bleedingDamage;
            attackInstance.slowDuration = playerStats.slowDuration;
            Debug.Log($"Applied upgrades to projectile. BounceCount: {attackInstance.bounceCount}");
        }
        else
        {
            Debug.LogError("AttackInstance not found on projectile or playerStats is null!");
        }

        if (burstIndex > 0)
        {
            float angle = burstIndex * 15f - (burstCount - 1) * 7.5f;
            projectile.transform.RotateAround(projectile.transform.position, Vector3.up, angle);
        }
    }
}