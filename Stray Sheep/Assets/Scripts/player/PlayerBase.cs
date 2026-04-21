using UnityEngine;
using UnityEngine.InputSystem;

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
        {
            transform.rotation = Quaternion.LookRotation(currentLookDirection);
        }

        if (attackPrefab != null && attackPoint != null)
        {
            Instantiate(attackPrefab, attackPoint.position, transform.rotation);
        }

        nextAttackTime = Time.time + attackCooldown;
    }
}