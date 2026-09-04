using UnityEngine;

namespace OriGame.Player
{
    public interface IAbilityResolver
    {
        public Vector2 ResolveMovement(ref PlayerMovementRequest[] requests, PlayerContext playerContext);
    }
}

