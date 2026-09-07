using OriGame.Input;
using UnityEngine;

namespace OriGame.Player
{
    public class PlayerDash : PlayerAbility
    {
        // High priority overrides both horizontal walk (0) and jump (100)
        private const int DASH_PRIORITY_X = 500;
        private const int DASH_PRIORITY_Y = 500;

        private float m_DashCooldownTimer;
        private float m_DashDurationTimer;
        private float m_DashBufferTimer;
        private Vector2 m_DashDirection;

        public PlayerDash(PlayerController controller, bool enabled = true) : base(controller, enabled)
        {
            m_DashBufferTimer = 0f;
        }

        public override void OnUpdate(in FrameInput frameInput, float deltaTime)
        {
            m_DashCooldownTimer -= deltaTime;

            if (m_DashCooldownTimer <= 0f)
            {
                m_DashCooldownTimer = 0f;
            }

            if (frameInput.DashPressed)
            {
                m_DashBufferTimer = m_Config.DashBuffer;
            }
            else if (m_DashBufferTimer > 0f)
            {
                m_DashBufferTimer -= deltaTime;
            }
        }

        public override PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime)
        {
            // Trigger dash intent on press if off cooldown
            if (m_DashBufferTimer > 0f && m_DashCooldownTimer <= 0f && !m_PlayerContext.Dashing)
            {
                m_PlayerContext.Dashing = true;
                
                m_DashDurationTimer = m_Config.DashDuration;
                m_DashCooldownTimer = m_Config.DashCooldown;

                if (m_PlayerContext.HorizontalInputDir.x != 0)
                {
                    m_DashDirection = new Vector2(Mathf.Sign(m_PlayerContext.HorizontalInputDir.x), 0f);
                }
                else
                {
                    if (m_PlayerContext.FacingRight)
                    {
                        m_DashDirection = new Vector2(1, 0);
                    }
                    else
                    {
                        m_DashDirection =  new Vector2(-1, 0);
                    }
                }
            }
            
            if (!m_PlayerContext.Dashing)
            {
                return new PlayerMovementRequest(Vector2.zero, -1, -1);
            }

            m_DashDurationTimer -= fixedDeltaTime;
            
            if (m_DashDurationTimer <= 0f)
            {
                m_PlayerContext.Dashing = false;
            }

            Vector2 dashVelocity = m_DashDirection * m_Config.DashSpeed * fixedDeltaTime;

            return new PlayerMovementRequest(dashVelocity, DASH_PRIORITY_X, DASH_PRIORITY_Y);
        }
    }
}