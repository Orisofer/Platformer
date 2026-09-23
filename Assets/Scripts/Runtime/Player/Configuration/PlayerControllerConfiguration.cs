using UnityEngine;

namespace OriGame.Player
{
    [CreateAssetMenu(fileName = "PlayerControllerConfiguration", menuName = "Configuration/Player/Player Controller Configuration", order = 1)]
    public class PlayerControllerConfiguration : ScriptableObject
    {
        [Header("LAYERS")]
    
        [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;
        
        [Header("General")]
        
        [Tooltip("Player Max Fall Speed")]
        public float MaxFallSpeed = 40;
        
        [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
        public float FallAcceleration = 110;
    
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------

        [Header("MOVEMENT")]
        
        public PlayerMoveConfiguration PlayerMoveConfiguration;
    
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------
        
        [Header("JUMP")]
        
        public PlayerJumpConfiguration PlayerJumpConfiguration;
        
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------

        [Header("Dash")]
        
        public PlayerDashConfiguration PlayerDashConfiguration;
        
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------

        [Header("Bounce")]
        
        public PlayerBounceConfiguration PlayerBounceConfiguration;
    }
}
    
