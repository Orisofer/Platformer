using UnityEngine;

[System.Serializable]
public class PlayerContext
{
    public BoxCollider2D ColliderBody;
    public Transform LastGround;
    public FrameInput FrameInput;
    public LayerMask PlayerLayer;
    public Vector2 FrameVelocity;
    public double TimeLeftTheGround;
    public float SkinWidth;
    public int AvailableJumps;
    public byte CollisionPattern;
    public bool Grounded;
    public bool AllowJump;
    public bool Jumping;
}
