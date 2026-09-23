using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerBounceConfiguration", menuName = "Configuration/Player/Abilities/Bounce", order = 1)]
    public class PlayerBounceConfiguration : PlayerAbilityConfiguration
    {
        [Tooltip("Toggle Ability")]
        public bool BounceEnabled = true;
        
        [Tooltip("How long the bounce takes")]
        public float BounceDuration = .2f;
        
        [Tooltip("How much velocity player gain while bouncing")]
        public float BounceSpeed = 2f;
        
        [Tooltip("How much time allowing the player to bounce if missed click")]
        public float BounceBuffer = .1f;
        
        [Tooltip("How much gravity happens to the player with the ability triggered")]
        public float DashGravityScale = 0f;
    }
}
