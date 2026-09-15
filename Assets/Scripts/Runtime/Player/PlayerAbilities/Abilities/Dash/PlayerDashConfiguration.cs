using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerDashConfiguration", menuName = "Configuration/Player/Abilities/Dash", order = 1)]
    public class PlayerDashConfiguration : PlayerAbilityConfiguration
    {
        [Tooltip("Toggle Ability")]
        public bool DashEnabled = true;
        
        [Tooltip("How long the dash takes")]
        public float DashDuration = .2f;
        
        [Tooltip("How much time needs until player can dash again")]
        public float DashCooldown = .025f;
        
        [Tooltip("How much velocity player gain while dashing")]
        public float DashSpeed = 2f;
        
        [Tooltip("How much time allowing the player to dash if missed click")]
        public float DashBuffer = .1f;
    }
}
