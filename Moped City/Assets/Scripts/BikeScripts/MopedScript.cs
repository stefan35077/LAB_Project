using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MopedScript : MonoBehaviour
{
    private PlayerInput controls;
    private PlayerInput.PlayerControllerActions playerMap;

    public Rigidbody2D rb;
    public SuspensionAnchor2D[] anchors;

    [Header("Ground Movement")]
    public float maxSpeed = 5f;              // Maximum speed (formerly moveSpeed)
    public float acceleration = 10f;          // How quickly the bike accelerates
    public float deceleration = 15f;         // How quickly the bike slows down
    public float directionChangeMultiplier = 0.5f; // Reduces acceleration when changing directions

    [Header("Air Control")]
    public float flipTorque = 400f;         // Force of the flip rotation
    public float maxAngularVelocity = 900f; // Maximum rotation speed

    private float moveInput;
    private float currentSpeed = 0f;         // Current speed of the bike
    private bool isGrounded;
    private float lastMoveDirection = 0f;    // Tracks last movement direction

    void Awake()
    {
        controls = new PlayerInput();
        playerMap = controls.PlayerController;
        rb = GetComponent<Rigidbody2D>();

        // Find suspension anchors if not assigned
        if (anchors == null || anchors.Length == 0)
        {
            anchors = GetComponentsInChildren<SuspensionAnchor2D>();
        }
    }

    private void OnEnable()
    {
        playerMap.Enable();
        playerMap.Move.performed += OnMove;
        playerMap.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        playerMap.Move.performed -= OnMove;
        playerMap.Move.canceled -= OnMove;
        playerMap.Disable();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isGrounded)
        {
            // Ground movement with acceleration
            ApplyGroundMovement();
        }
        else
        {
            // Air control - Flips
            ApplyFlipRotation();
            // Maintain some horizontal momentum in air
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }

        // Update last direction when there's significant movement
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            lastMoveDirection = Mathf.Sign(moveInput);
        }
    }

    private void ApplyGroundMovement()
    {
        float targetSpeed = moveInput * maxSpeed;
        float accelerationRate = acceleration;

        // Check if we're changing directions
        if (currentSpeed != 0 && Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed))
        {
            // Apply direction change penalty
            accelerationRate *= directionChangeMultiplier;
        }

        // If no input, decelerate
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Accelerate towards target speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        }

        // Apply the movement
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void ApplyFlipRotation()
    {
        float flipDirection = moveInput;
        rb.AddTorque(flipDirection * flipTorque);
    }

    private void CheckGrounded()
    {
        isGrounded = false;
        foreach (var anchor in anchors)
        {
            var hitInfo = (RaycastHit2D)typeof(SuspensionAnchor2D)
                .GetField("lastHit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(anchor);

            if (hitInfo.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
    }
}