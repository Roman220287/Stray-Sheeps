using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour
{
    private InputSystem_Actions controls;
    private CharacterController controller;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 10f; // Higher = Snappier, Lower = Heavier

    private Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();
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

        // CHECK FOR MOUSE/KEYBOARD
        if (Mouse.current != null && Mouse.current.wasUpdatedThisFrame)
        {
            // Create a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            // Create a virtual plane at the player's height to find where the mouse hits
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                targetDirection = (point - transform.position).normalized;
            }
        }
        // CHECK FOR GAMEPAD (Right Stick / Look Action)
        else
        {
            Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
            if (lookInput.sqrMagnitude > 0.1f)
            {
                targetDirection = new Vector3(lookInput.x, 0, lookInput.y);
            }
        }

        // SMOOTH LERP/SLERP ROTATION
        if (targetDirection != Vector3.zero)
        {
            targetDirection.y = 0; // Keep the character upright
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            // Slerp provides a smooth arc between current and target rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
}