using UnityEngine;

namespace OriGame.Player
{
    public struct PlayerMovementRequest
    {
        public Vector2 Target;
        public int PriorityOnX;
        public int PriorityOnY;

        public PlayerMovementRequest(Vector2 target,  int priorityOnX, int priorityOnY)
        {
            Target  = target;
            PriorityOnX = priorityOnX;
            PriorityOnY = priorityOnY;
        }
    }
}