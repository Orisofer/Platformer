using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerContext
{
    public BoxCollider2D ColliderBody;
    public FrameInput FrameInput;
    public LayerMask PlayerLayer;
    public Vector2 FrameVelocity;
    public double TimeLeftTheGround;
    public float SkinWidth;
    public int AvailableJumps;
    public bool Grounded;
    public bool AllowJump;
    public bool Jumping;
}
