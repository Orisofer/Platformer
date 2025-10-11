using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerContext
{
    public BoxCollider2D ColliderBody;
    public FrameInput FrameInput;
    public LayerMask PlayerLayer;
    public Vector2 ThisFrameVelocity;
    public Vector2 LastFrameVelocity;
    public float SkinWidth;
    public double TimeLeftTheGround;
    public bool Grounded;
    public bool Jumping;
    public bool HasJumpToConsume;
}
