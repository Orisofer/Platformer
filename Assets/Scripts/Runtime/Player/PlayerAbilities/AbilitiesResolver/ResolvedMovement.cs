using UnityEngine;

namespace OriGame.Player
{
    public struct ResolvedMovement
    {
        public Vector2 Target;
        public float GravityScale;

        public ResolvedMovement(Vector2 target,  float gravityScale)
        {
            Target = target;
            GravityScale = gravityScale;
        }

        public void Clear()
        {
            Target = Vector2.zero;
            GravityScale = 0f;
        }
    }
}