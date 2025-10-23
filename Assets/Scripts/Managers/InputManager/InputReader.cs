using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputEditor;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions, IInputReader
{
    public event Action MoveStarted = delegate { };
    public event Action MoveEnded = delegate { };
    public event Action JumpPressed = delegate { };
    public event Action JumpReleased = delegate { };

    private InputEditor m_InputEditor;
    
    public Vector2 Direction => m_InputEditor.Player.Move.ReadValue<Vector2>();

    public bool IsJumpHeld => m_InputEditor.Player.Jump.IsPressed();
    
    public void EnablePlayerActions()
    {
        if (m_InputEditor == null)
        {
            m_InputEditor = new InputEditor();
            m_InputEditor.Player.SetCallbacks(this);
        }
        
        m_InputEditor.Player.Enable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            MoveStarted?.Invoke();
        }

        if (context.phase == InputActionPhase.Canceled)
        {
            MoveEnded?.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            JumpPressed?.Invoke();
        }

        if (context.phase == InputActionPhase.Canceled)
        {
            JumpReleased?.Invoke();
        }
    }

    private void OnDisable()
    {
        m_InputEditor?.Player.Disable();
        m_InputEditor?.UI.Disable();
        m_InputEditor = null;
    }
}
