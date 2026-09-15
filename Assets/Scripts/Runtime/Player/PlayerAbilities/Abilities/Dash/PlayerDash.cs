using OriGame.Input;
using UnityEngine;

namespace OriGame.Player
{
    public class PlayerDash : PlayerAbility<PlayerDashConfiguration>
    {
        // High priority overrides both horizontal walk (0) and jump (100)
        private const int DASH_PRIORITY_X = 500;
        private const int DASH_PRIORITY_Y = 500;

        private Vector2 m_DashDirection;
        private float m_DashCooldownTimer;
        private float m_DashDurationTimer;
        private float m_DashBufferTimer;
        private bool m_IsAirDashing;

        public PlayerDash(PlayerDashConfiguration config, PlayerController controller, bool enabled = true) : base(config, controller, enabled)
        {
            m_DashBufferTimer = 0f;
        }

        public override void OnUpdate(in FrameInput frameInput, float deltaTime)
        {
            if (!m_Config.DashEnabled) return;
            
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

            // reset air dashing state
            if (m_IsAirDashing && m_PlayerContext.Grounded)
            {
                m_IsAirDashing = false;
            }
        }
        
        public override PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime)
        {
            Vector2 dashVelocity = Vector2.zero;

            if (!m_PlayerContext.Dashing)
            {
                // start new dash
                if (m_DashBufferTimer > 0f && m_DashCooldownTimer <= 0f && !m_IsAirDashing)
                {
                    dashVelocity = StartDash(fixedDeltaTime);
                }
                else
                {
                    return new PlayerMovementRequest(dashVelocity, -1, -1);
                }
            }
            else
            {
                if (m_DashDurationTimer >= 0f)
                {
                    dashVelocity = UpdateDash(fixedDeltaTime);
                }
                else
                {
                    EndDash();
                }
            }

            return new PlayerMovementRequest(dashVelocity, DASH_PRIORITY_X, DASH_PRIORITY_Y);
        }

        private void EndDash()
        {
            m_PlayerContext.Dashing = false;
        }

        private Vector2 UpdateDash(float fixedDeltaTime)
        {
            m_DashDurationTimer -= fixedDeltaTime;
            return m_DashDirection * (m_Config.DashSpeed * fixedDeltaTime);
        }

        private Vector2 StartDash(float fixedDeltaTime)
        {
            m_PlayerContext.Dashing = true;
            m_DashDurationTimer = m_Config.DashDuration;
            m_DashCooldownTimer = m_Config.DashCooldown;

            if (!m_PlayerContext.Grounded)
            {
                m_IsAirDashing = true;
            }
                    
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

            return m_DashDirection * (m_Config.DashSpeed * fixedDeltaTime);
        }
    }
}