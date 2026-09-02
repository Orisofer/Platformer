using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerControllerConfiguration", menuName = "Configuration/Player/Player Controller Configuration", order = 1)]
    public class PlayerControllerConfiguration : ScriptableObject
    {
        [Header("LAYERS")]
    
        [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;
    
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------

        [Header("MOVEMENT")]
    
        [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float HorizontalDeceleration = 60;
    
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------
    
        [Header("JUMP")]
    
        [Tooltip("The over-time velocity applied when jumping")]
        public float JumpPower = 36;
    
        [Tooltip("Jump will end when reaching max velocity")]
        public float MaxJumpVelocity = 11;
    
        [Tooltip("The immediate velocity applied when starting jumping")]
        public float JumpStartImpulse = 4;
    
        [Tooltip("The maximum vertical movement speed")]
        public int MaxJumps = 1;

        [Tooltip("The maximum vertical movement speed")]
        public float MaxFallSpeed = 40;

        [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
        public float FallAcceleration = 110;

        [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public float CoyoteTime = .15f;

        [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
        public float JumpBuffer = .2f;
    }
}
    
