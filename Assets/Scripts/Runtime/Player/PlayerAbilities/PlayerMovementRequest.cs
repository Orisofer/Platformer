using UnityEngine;

namespace OriGame.Player
{
    public struct PlayerMovementRequest
    {
        public Vector2 Target;
        public int PriorityOnX;
        public int PriorityOnY;
        public float GravityScale;

        public PlayerMovementRequest(Vector2 target,  int priorityOnX, int priorityOnY, float gravityScale)
        {
            Target  = target;
            PriorityOnX = priorityOnX;
            PriorityOnY = priorityOnY;
            GravityScale = gravityScale;
        }

        public static readonly PlayerMovementRequest bypass = new PlayerMovementRequest()
        {
            Target = Vector2.zero,
            PriorityOnX = -1,
            PriorityOnY = -1,
            GravityScale = 1f
        };
    }
}