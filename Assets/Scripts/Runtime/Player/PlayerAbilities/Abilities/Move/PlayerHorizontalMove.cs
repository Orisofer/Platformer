using OriGame.Input;
using UnityEngine;

namespace OriGame.Player
{
    public class PlayerHorizontalMove : PlayerAbility<PlayerMoveConfiguration>
    {
        private const float ABILITY_GRAVITY_SCALE = 1.0f;
        private const int PRIORITY_ON_X = 100;
        private const int PRIORITY_ON_Y = 0;
        
        public PlayerHorizontalMove(PlayerMoveConfiguration config, PlayerController controller, bool enabled = true) : base(config, controller, enabled)
        {
        }

        public override void OnUpdate(in FrameInput frameInput, float deltaTime)
        {
            if (!m_Config.MovementEnabled) return;
            
            m_PlayerContext.HorizontalInputDir = frameInput.Direction;
            
            if (m_PlayerContext.HorizontalInputDir == Vector2.zero)
            {
                m_PlayerContext.Walking = false;
            }
            else
            {
                m_PlayerContext.Walking = true;
            }
        }
    
        public override PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime)
        {
            float horizontalInputDir = m_PlayerContext.HorizontalInputDir.x;

            Vector2 requestTarget = Vector2.zero;
        
            // handle walking physics
            if (!m_PlayerContext.Walking)
            {
                float deceleration = m_Config.HorizontalDeceleration;
                
                requestTarget.x = Mathf.MoveTowards(
                    m_PlayerContext.CurrentVelocity.x,
                    0,
                    deceleration * fixedDeltaTime);
            }
            else
            {
                float maxHorizontalSpeed = m_Config.MaxSpeed * horizontalInputDir;
                float acceleration = m_Config.Acceleration;
                
                requestTarget.x = Mathf.MoveTowards(
                    m_PlayerContext.CurrentVelocity.x,
                    maxHorizontalSpeed,
                    acceleration * fixedDeltaTime);
            }
        
            // handle player flip X
            if (horizontalInputDir < 0f)
            {
                if (m_PlayerContext.FacingRight)
                {
                    m_Controller.FlipX(false);
                }
            }

            if (horizontalInputDir > 0f)
            {
                if (!m_PlayerContext.FacingRight)
                {
                    m_Controller.FlipX(true);
                }
            }

            PlayerMovementRequest movementRequest = new PlayerMovementRequest(requestTarget, PRIORITY_ON_X, PRIORITY_ON_Y, ABILITY_GRAVITY_SCALE);
            
            return movementRequest;
        }
    }
}