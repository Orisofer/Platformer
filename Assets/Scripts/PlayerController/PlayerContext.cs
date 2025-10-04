using UnityEngine;

[System.Serializable]
public class PlayerContext
{
    public Rigidbody2D Rigidbody2D;
    public Animator Animator;
    public Vector2 MoveHorizontal;
    public Vector2 Velocity;
    public float HorizontalSpeed;
    public float JumpForce;
    public bool JumpPressed;
    public bool Grounded;
}
