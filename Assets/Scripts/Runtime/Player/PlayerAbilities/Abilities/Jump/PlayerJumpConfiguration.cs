using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerJumpConfiguration", menuName = "Configuration/Player/Abilities/Jump", order = 1)]
    public class PlayerJumpConfiguration : PlayerAbilityConfiguration
    {
        [Tooltip("Toggle Ability")]
        public bool JumpEnabled = true;
    
        [Tooltip("The time upward force will move the player up")]
        public float JumpAcceleration = 4f;
    
        [Tooltip("Jump will end when reaching max velocity")]
        public float MaxJumpVelocity = 11;
    
        [Tooltip("The immediate velocity applied when starting jumping")]
        public float JumpStartImpulse = 4;
    
        [Tooltip("The maximum vertical movement speed")]
        public int MaxJumps = 1;

        [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public float CoyoteTime = .15f;

        [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
        public float JumpBuffer = .2f;

        [Tooltip("The time upward force will move the player up")]
        public float MaxJumpHoldTime = .8f;
        
        [Tooltip("The magnitude to decelerate when reaching a peak of a jump till y velocity zeros")]
        public float JumpReleaseDeceleration = 4f;
        
        [Tooltip("How much gravity happens to the player with the ability triggered")]
        public float JumpGravityScale = 0f;
    }
}

