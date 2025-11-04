using UnityEngine;

[System.Serializable]
public class PlayerContext
{
    public BoxCollider2D ColliderBody;
    public Transform LastGround;
    public LayerMask PlayerLayer;
    public Vector2 FrameVelocity;
    public Vector2 LastFrameVelocity;
    public double TimeLeftTheGround;
    public float SkinWidth;
    public int AvailableJumps;
    public byte CollisionPattern;
    public bool FacingRight;
    public bool Grounded;
    public bool Jumping;
    public bool Falling;
}
