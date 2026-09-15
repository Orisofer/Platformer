using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerMoveConfiguration", menuName = "Configuration/Player/Abilities/Move", order = 1)]
    public class PlayerMoveConfiguration : PlayerAbilityConfiguration
    {
        [Tooltip("Toggle Ability")]
        public bool MovementEnabled = true;
    
        [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float HorizontalDeceleration = 60;
    }
}
