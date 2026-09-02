using UnityEngine;

namespace OriGame.Player
{
    [System.Serializable]
    public class PlayerContext
    {
        public BoxCollider2D ColliderBody;
        public Transform LastGround;
        public CollisionContext CollisionContext;
        public SnapRequest SnapRequest;
        public LayerMask PlayerLayer;
        public Vector2 FrameVelocity;
        public Vector2 LastFrameVelocity;
        public Vector2 HorizontalInputDir;
        public double TimeLeftTheGround;
        public float SkinWidth;
        public float CoyoteTime;
        public int AvailableJumps;
        public byte CollisionPattern;
        public bool JumpHeld;
        public bool JumpPressed;
        public bool FacingRight;
        public bool Walking;
        public bool Grounded;
        public bool Jumping;
        public bool Falling;
    }
}
    

