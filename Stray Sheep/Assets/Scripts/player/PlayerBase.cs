using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Animator animator;

    [Header("Animator Settings")]
    [SerializeField] private string animatorParameter = "Speed"; // Match this to your Animator parameter name

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private float burstSpreadAngle = 15f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    private InputSystem_Actions controls;
    private PlayerStatsBase playerStats;

    private Vector2 moveInput;
    private Vector3 lookDirection;
    private bool isUsingRightStickLook;

    private bool isDashing;

    private float nextAttackTime;
    private float nextDashTime;

    #region Unity Methods

    private void Awake()
    {
        controller ??= GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStatsBase>();

        controls = new InputSystem_Actions();

        controls.Player.Attack.performed += _ => TryAttack();
        controls.Player.Dash.performed += _ => TryDash();
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
        if (PauseManager.IsPaused) return;

        ReadInput();

        // Update Animator Speed
        if (animator != null)
        {
            // Set the animator parameter to the magnitude of the input (0 to 1)
            animator.SetFloat(animatorParameter, moveInput.magnitude);
        }

        if (isDashing)
            return;

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

    // ... (rest of your existing methods remain unchanged)

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
        if (Gamepad.current != null)
        {
            Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

            if (lookInput.sqrMagnitude >= 0.1f)
            {
                isUsingRightStickLook = true;
                return new Vector3(lookInput.x, 0f, lookInput.y).normalized;
            }

            if (isUsingRightStickLook)
                return Vector3.zero;
        }

        isUsingRightStickLook = false;
        return GetMouseLookDirection();
    }

    private Vector3 GetMouseLookDirection()
    {
        if (Camera.main == null || Mouse.current == null)
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

    private Vector3 GetDashDirection()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            return moveDirection.normalized;
        }
        return transform.forward;
    }

    #endregion

    #region Combat

    private void TryAttack()
    {
        if (isDashing)
            return;

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

    private void TryDash()
    {
        if (isDashing)
            return;

        if (Time.time < nextDashTime)
            return;

        Vector3 dashDirection = GetDashDirection();

        if (dashDirection == Vector3.zero)
            return;

        StartCoroutine(DashCoroutine(dashDirection));
    }

    private IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        isDashing = true;

        float elapsedTime = 0f;
        float dashSpeed = dashDistance / Mathf.Max(0.01f, dashDuration);

        while (elapsedTime < dashDuration)
        {
            float stepDistance = dashSpeed * Time.deltaTime;
            controller.Move(dashDirection * stepDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        nextDashTime = Time.time + dashCooldown;
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