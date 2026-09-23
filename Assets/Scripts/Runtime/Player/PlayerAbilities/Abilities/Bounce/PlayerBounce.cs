using OriGame.Input;
using UnityEngine;

namespace OriGame.Player
{
    public class PlayerBounce : PlayerAbility<PlayerBounceConfiguration>
    {
        private const int BOUNCE_PRIORITY_X = 600;
        private const int BOUNCE_PRIORITY_Y = 600;

        private Vector2 m_BounceDirection;
        private float m_BounceInputTimer;
        private float m_BounceTimer;
        private bool m_IsAirBouncing;
        
        public PlayerBounce(PlayerBounceConfiguration config, PlayerController controller, bool enabled = false) : base(config, controller, enabled)
        {
        }

        public override void OnUpdate(in FrameInput frameInput, float deltaTime)
        {
            if (!m_Config.BounceEnabled) return;

            if (frameInput.BouncePressed && !m_IsAirBouncing)
            {
                m_BounceInputTimer = m_Config.BounceBuffer;
            }

            m_BounceInputTimer -= deltaTime;
            
            if (m_BounceInputTimer <= 0f)
            {
                m_BounceInputTimer = 0f;
            }

            if (m_PlayerContext.Grounded && !m_PlayerContext.Bouncing)
            {
                m_IsAirBouncing = false;
            }
        }

        public override PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime)
        {
            if (!m_Config.BounceEnabled) return PlayerMovementRequest.bypass;
            
            if (!m_PlayerContext.Bouncing)
            {
                if (m_BounceInputTimer > 0f && !m_PlayerContext.Grounded && !m_IsAirBouncing)
                {
                    Vector2 currentDir = m_PlayerContext.InputDir;
                    
                    if (currentDir.x != 0 && currentDir.y != 0)
                    {
                        StartBounce();
                    }
                }
            }
            
            if (m_PlayerContext.Bouncing)
            {
                if (m_BounceTimer > 0)
                {
                    Vector2 bounceVelocity = UpdateBounce();
                    m_BounceTimer -= fixedDeltaTime;
                    
                    return new PlayerMovementRequest(bounceVelocity, BOUNCE_PRIORITY_X,  BOUNCE_PRIORITY_Y, m_Config.DashGravityScale);
                }
                
                EndBounce();
            }
            
            return PlayerMovementRequest.bypass;
        }
        
        private void StartBounce()
        {
            m_PlayerContext.Bouncing = true;
            m_IsAirBouncing = true;
            m_BounceTimer = m_Config.BounceDuration;
            m_BounceDirection = m_PlayerContext.InputDir;
            m_BounceInputTimer = 0f;
        }
        
        private Vector2 UpdateBounce()
        {
            ref readonly CollisionContext collisions = ref m_PlayerContext.CollisionContext;
            
            if (collisions.WallLeft && m_BounceDirection.x < 0)
            {
                m_BounceDirection.x *= -1;
            }

            if (collisions.WallRight && m_BounceDirection.x > 0)
            {
                m_BounceDirection.x *= -1;
            }

            if (collisions.Ceiling && m_BounceDirection.y > 0)
            {
                m_BounceDirection.y *= -1;
            }

            if (collisions.Ground && m_BounceDirection.y < 0)
            {
                m_BounceDirection.y *= -1;
            }
            
            return m_BounceDirection * m_Config.BounceSpeed;
        }

        private void EndBounce()
        {
            m_PlayerContext.Bouncing = false;
            m_BounceTimer = 0f;
            m_BounceDirection =  Vector2.zero;
            m_BounceInputTimer = 0f;
        }
    }
}
