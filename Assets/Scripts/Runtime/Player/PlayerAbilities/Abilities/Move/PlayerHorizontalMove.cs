using UnityEngine;

namespace OriGame.Player
{
    public class PlayerHorizontalMove : PlayerAbility
    {
        private const int PRIORITY_ON_X = 100;
        private const int PRIORITY_ON_Y = 0;
        
        public PlayerHorizontalMove(PlayerController controller, bool enabled = false) : base(controller, enabled)
        {
        }

        public override void OnUpdate(float deltaTime)
        {
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

            PlayerMovementRequest movementRequest = new PlayerMovementRequest(requestTarget, PRIORITY_ON_X, PRIORITY_ON_Y);
            
            return movementRequest;
        }
    }
}