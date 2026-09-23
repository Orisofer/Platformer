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
    }
}