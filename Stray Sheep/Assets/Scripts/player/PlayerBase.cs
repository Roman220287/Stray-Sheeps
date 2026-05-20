using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackPrefab;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private float burstSpreadAngle = 15f;

    private InputSystem_Actions controls;
    private PlayerStatsBase playerStats;

    private Vector2 moveInput;
    private Vector3 lookDirection;

    private float nextAttackTime;

    #region Unity Methods

    private void Awake()
    {
        controller ??= GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStatsBase>();

        controls = new InputSystem_Actions();

        controls.Player.Attack.performed += _ => TryAttack();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        ReadInput();

        HandleMovement();
        HandleRotation();
    }

    #endregion

    #region Input

    private void ReadInput()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = GetLookDirection();

        if (targetDirection == Vector3.zero)
            return;

        lookDirection = targetDirection;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private Vector3 GetLookDirection()
    {
        // Mouse aiming
        if (Gamepad.current == null || Mouse.current.wasUpdatedThisFrame)
        {
            return GetMouseLookDirection();
        }

        // Controller aiming
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        if (lookInput.sqrMagnitude < 0.1f)
            return Vector3.zero;

        return new Vector3(lookInput.x, 0f, lookInput.y).normalized;
    }

    private Vector3 GetMouseLookDirection()
    {
        if (Camera.main == null)
            return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return Vector3.zero;

        Vector3 hitPoint = ray.GetPoint(distance);

        Vector3 direction = hitPoint - transform.position;
        direction.y = 0f;

        return direction.normalized;
    }

    #endregion

    #region Combat

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        RotateTowardsLookDirection();

        int burstCount = Mathf.Max(1, playerStats.burstShots);

        for (int i = 0; i < burstCount; i++)
        {
            FireProjectile(i, burstCount);
        }
    }

    private void RotateTowardsLookDirection()
    {
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void FireProjectile(int burstIndex, int burstCount)
    {
        if (attackPrefab == null || attackPoint == null)
            return;

        float spreadOffset =
            (burstIndex - (burstCount - 1) / 2f) * burstSpreadAngle;

        Quaternion projectileRotation =
            transform.rotation * Quaternion.Euler(0f, spreadOffset, 0f);

        GameObject projectile = Instantiate(
            attackPrefab,
            attackPoint.position,
            projectileRotation
        );

        ApplyProjectileStats(projectile);
    }

    private void ApplyProjectileStats(GameObject projectile)
    {
        AttackInstance attackInstance =
            projectile.GetComponentInChildren<AttackInstance>();

        if (attackInstance == null)
        {
            Debug.LogError("AttackInstance missing on projectile.");
            return;
        }

        attackInstance.damage = playerStats.baseDamage;
        attackInstance.speed = playerStats.bulletSpeed;
        attackInstance.bounceCount = playerStats.bounceCount;
        attackInstance.bleedingDamage = playerStats.bleedingDamage;
        attackInstance.slowDuration = playerStats.slowDuration;
    }

    #endregion
}