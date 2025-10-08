using UnityEngine;

[System.Serializable]
public class PlayerContext
{
    public BoxCollider2D ColliderBody;
    public FrameInput FrameInput;
    public LayerMask PlayerLayer;
    public Vector2 Velocity;
    public double TimeLeftTheGround;
    public bool Grounded;
    public bool Jumping;
    public bool HasJumpToConsume;
}
