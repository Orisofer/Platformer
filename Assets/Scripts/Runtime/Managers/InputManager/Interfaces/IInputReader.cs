using UnityEngine;
using UnityEngine.InputSystem;
using static InputEditor;

public interface IInputReader
{
    public Vector2 Direction { get; }
    public void EnablePlayerActions();
}
