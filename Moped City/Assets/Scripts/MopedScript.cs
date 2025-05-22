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

    float moveInput;
    public float moveSpeed;

    void Awake()
    {
        controls = new PlayerInput();
        playerMap = controls.PlayerController;
        rb = GetComponent<Rigidbody2D>();
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
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}
