using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class MopedScript : MonoBehaviour
{
    // SINGLETON INSTANCE
    public static MopedScript Instance { get; private set; }

    private PlayerInput controls;
    private PlayerInput.PlayerControllerActions playerMap;

    public Rigidbody2D rb;
    public SuspensionAnchor2D[] anchors;

    [Header("Ground Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float directionChangeMultiplier = 0.5f;

    [Header("Air Control")]
    public float flipTorque = 400f;
    public float maxAngularVelocity = 900f;

    private float moveInput;
    private float rotateInput;
    private float currentSpeed = 0f;
    private bool isGrounded;
    private float lastMoveDirection = 0f;

    private bool isDead = false;

    void Awake()
    {
        // IMPLEMENT SINGLETON PATTERN
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        controls = new PlayerInput();
        playerMap = controls.PlayerController;
        rb = GetComponent<Rigidbody2D>();

        // IF ANCHORS ARE NOT ASSIGNED, FIND THEM AUTOMATICALLY IN CHILDREN
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
        playerMap.Rotate.performed += OnRotate;
        playerMap.Rotate.canceled += OnRotate;
    }

    private void OnDisable()
    {
        playerMap.Move.performed -= OnMove;
        playerMap.Move.canceled -= OnMove;
        playerMap.Rotate.performed -= OnRotate;
        playerMap.Rotate.canceled -= OnRotate;
        playerMap.Disable();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        CheckGrounded();

        if (isGrounded)
        {
            ApplyGroundMovement();
        }
        else
        {
            ApplyFlipRotation();
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            lastMoveDirection = Mathf.Sign(moveInput);
        }
    }

    private void ApplyGroundMovement()
    {
        float targetSpeed = moveInput * maxSpeed;
        float accelerationRate = acceleration;

        // IF CHANGING DIRECTION, REDUCE ACCELERATION TO AVOID SNAPPY TURNS
        if (currentSpeed != 0 && Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed))
        {
            accelerationRate *= directionChangeMultiplier;
        }

        // IF NO INPUT, SLOW DOWN SMOOTHLY
        if (Mathf.Abs(moveInput) < 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            // ACCELERATE TOWARDS TARGET SPEED
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    public void OnRotate(InputAction.CallbackContext ctx)
    {
        rotateInput = ctx.ReadValue<float>();
    }

    private void ApplyFlipRotation()
    {
        // APPLY TORQUE FOR FLIP BASED ON ROTATE INPUT, INVERTED FOR CONTROLS
        float flipDirection = -rotateInput;
        rb.AddTorque(flipDirection * flipTorque);
    }

    private void CheckGrounded()
    {
        // CHECK IF ANY SUSPENSION ANCHOR IS TOUCHING GROUND BY REFLECTING INTO PRIVATE FIELD
        isGrounded = false;

        foreach (var anchor in anchors)
        {
            // REFLECT TO ACCESS PRIVATE 'lastHit' FIELD IN SuspensionAnchor2D
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

    public void HasDied()
    {
        isDead = true;
        playerMap.Disable();
        moveInput = 0f;
        rotateInput = 0f;
    }
}
