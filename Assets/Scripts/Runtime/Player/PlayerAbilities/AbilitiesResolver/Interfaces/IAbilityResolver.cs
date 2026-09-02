using UnityEngine;

namespace OriGame.Player
{
    public interface IAbilityResolver
    {
        public Vector2 ResolveAbilities(PlayerContext playerContext);
    }
}

