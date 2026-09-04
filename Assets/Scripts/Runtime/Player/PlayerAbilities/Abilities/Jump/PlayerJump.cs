using UnityEngine;

namespace OriGame.Player
{
    public class PlayerJump : PlayerAbility
    {
        private const int PRIORITY_ON_X = 0;
        private const int PRIORITY_ON_Y = 100;

        private float m_JumpBufferTimer;
        private float m_HoldTimer;

        public PlayerJump(PlayerController controller, bool enabled = true) : base(controller, enabled)
        {
            m_JumpBufferTimer = 0f;
            m_HoldTimer = 0f;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (m_Config.MaxJumps == 0) return;

            // Update falling state context
            m_PlayerContext.Falling = !m_PlayerContext.Grounded && m_PlayerContext.CurrentVelocity.y <= 0f;

            // Input buffering
            if (m_PlayerContext.JumpPressed)
            {
                m_JumpBufferTimer = m_Config.JumpBuffer;
            }
            else if (m_JumpBufferTimer > 0f)
            {
                m_JumpBufferTimer -= deltaTime;
            }

            if (m_PlayerContext.JumpHeld)
            {
                m_HoldTimer += deltaTime;

                if (m_HoldTimer >= m_Config.MaxJumpHoldTime)
                {
                    m_HoldTimer = m_Config.MaxJumpHoldTime;
                }
            }
        }

        public override PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime)
        {
            Vector2 requestedVelocity = Vector2.zero;
            requestedVelocity.y = m_PlayerContext.CurrentVelocity.y;

            // 1. Trigger Jump Launch
            if (!m_PlayerContext.Jumping && m_JumpBufferTimer > 0f && AllowJump())
            {
                StartJump(ref requestedVelocity);
            }
            else if (m_PlayerContext.Jumping)
            {
                ProcessJumpHold(fixedDeltaTime, ref requestedVelocity);
            }

            return new PlayerMovementRequest(requestedVelocity, PRIORITY_ON_X, PRIORITY_ON_Y);
        }

        private void StartJump(ref Vector2 requestedVelocity)
        {
            m_JumpBufferTimer = 0f;
            m_HoldTimer = 0f;
            m_PlayerContext.Jumping = true;

            // Deduct jump charge (with penalty handling if slipping off ledges)
            if (!m_PlayerContext.Grounded &&
                m_PlayerContext.CoyoteTime <= 0f && 
                m_PlayerContext.Falling &&
                m_PlayerContext.AvailableJumps == m_Config.MaxJumps)
            {
                m_PlayerContext.AvailableJumps--;
            }
            
            m_PlayerContext.AvailableJumps--;
            m_PlayerContext.Falling = false;
            m_PlayerContext.Grounded = false;
            m_PlayerContext.CoyoteTime = 0f;
            
            requestedVelocity.y = m_Config.JumpStartImpulse;

            m_Controller.RaiseJumpEvent();
        }

        private void ProcessJumpHold(float fixedDeltaTime, ref Vector2 requestedVelocity)
        {
            if (m_PlayerContext.JumpHeld && m_HoldTimer < m_Config.MaxJumpHoldTime)
            {
                // Add upward hold acceleration to current velocity
                float holdAcceleration = m_Config.JumpAcceleration;
            
                requestedVelocity.y = Mathf.MoveTowards(
                    m_PlayerContext.CurrentVelocity.y,
                    m_Config.MaxJumpVelocity,
                    holdAcceleration * fixedDeltaTime);
            }
            else if (!m_PlayerContext.JumpHeld || m_HoldTimer >= m_Config.MaxJumpHoldTime)
            {
                float apexReachedDeceleration = m_Config.JumpReleaseDeceleration;
                
                requestedVelocity.y = Mathf.MoveTowards(
                    m_PlayerContext.CurrentVelocity.y,
                    0,
                    apexReachedDeceleration * fixedDeltaTime);
            }
        }

        private bool AllowJump()
        {
            if (m_PlayerContext.Grounded || m_PlayerContext.CoyoteTime > 0f)
            {
                return true;
            }
            
            int effectiveAirJumps = (m_PlayerContext.AvailableJumps == m_Config.MaxJumps)
                ? m_PlayerContext.AvailableJumps - 1
                : m_PlayerContext.AvailableJumps;

            return effectiveAirJumps > 0;
        }
    }
}