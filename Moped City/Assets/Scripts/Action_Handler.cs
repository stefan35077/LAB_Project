using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputHandler : MonoBehaviour
{
    private PlayerInput controls;
    private PlayerInput.PlayerControllerActions playerMap;

    private void Awake()
    {
        controls = new PlayerInput();
        playerMap = controls.PlayerController;
    }

    private void OnEnable()
    {

        playerMap.Enable();
        playerMap.Move += OnMove();
    }

    private void OnDisable()
    {

    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
    }
}
