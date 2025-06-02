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
    public float moveSpeed = 5f;

    [Header("Air Control")]
    public float flipTorque = 400f;         // Force of the flip rotation
    public float maxAngularVelocity = 900f; // Maximum rotation speed

    private float moveInput;
    private bool isGrounded;

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
            // Ground movement
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Air control - Flips
            ApplyFlipRotation();
        }
    }

    private void ApplyFlipRotation()
    {
        // Forward input causes forward flip (negative rotation in 2D)
        // Backward input causes backward flip (positive rotation in 2D)
        float flipDirection = -moveInput; // Invert for correct flip direction

        // Apply flip torque
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