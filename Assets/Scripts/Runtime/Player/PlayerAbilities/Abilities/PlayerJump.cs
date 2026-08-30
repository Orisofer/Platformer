using UnityEngine;

public class PlayerJump : PlayerAbility
{
    private float m_JumpBufferTimer;
    private bool m_RaisedJumpingEvent;
    private bool m_RaisedFallingEvent;

    public PlayerJump(PlayerController controller, bool enabled = false) : base(controller, enabled)
    {
        m_JumpBufferTimer = 0f;
    }

    public override void OnUpdate(float deltaTime)
    {
        // no jumps left for the player
        if (m_Config.MaxJumps == 0)
        {
            return;
        }

        // check if jump button released while jumping
        if (m_PlayerContext.Jumping && !m_PlayerContext.JumpHeld)
        {
            m_PlayerContext.Jumping = false;
        }
        
        // 2. Register new jump press into buffer
        if (m_PlayerContext.JumpPressed)
        {
            m_JumpBufferTimer = m_Config.JumpBuffer;
        }
        else if (m_JumpBufferTimer > 0f)
        {
            m_JumpBufferTimer -= deltaTime;
        }

        if (m_JumpBufferTimer > 0f && AllowJump())
        {
            m_JumpBufferTimer = 0f;
            
            JumpStarted();
        }
    }

    private bool AllowJump()
    {
        // Grounded or Coyote Time available
        if (m_PlayerContext.Grounded || m_PlayerContext.CoyoteTime > 0f)
        {
            return true;
        }

        if (m_PlayerContext.AvailableJumps > 0)
        {
            int usableJumps = m_PlayerContext.AvailableJumps;
            
            if (m_PlayerContext.Falling && usableJumps == m_Config.MaxJumps)
            {
                usableJumps--;
            }

            return usableJumps > 0;
        }

        return false;
    }
    
    private void JumpStarted()
    {
        m_PlayerContext.Jumping = true;

        // handle penalty if player falls from ledge
        if (!m_PlayerContext.Grounded &&
            m_PlayerContext.CoyoteTime <= 0f && m_PlayerContext.Falling &&
            m_PlayerContext.AvailableJumps == m_Config.MaxJumps)
        {
            m_PlayerContext.AvailableJumps--;
        }
        
        if (m_PlayerContext.AvailableJumps <= 0)
        {
            m_PlayerContext.AvailableJumps = 0;
        }
        
        m_PlayerContext.Grounded = false;
        m_PlayerContext.CoyoteTime = 0f;
        m_PlayerContext.AvailableJumps--;
    }

    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        HandleJumpPhysics();
    }
    
    private void HandleJumpPhysics()
    {
        // execute jump logic first
        if (m_PlayerContext.Jumping)
        {
            ExecuteJump();
        }
        else
        {
            JumpEnded();
        }
        
        // reset velocity + snap if player hit the ceiling
        if (m_PlayerContext.FrameVelocity.y > 0f)
        {
            ref readonly var ceilingCheck = ref m_PlayerContext.CollisionContext.Ceiling;

            if (ceilingCheck)
            {
                m_Controller.RequestSnap(ceilingCheck.CollidedTransform, SnapDirection.Ceiling, ceilingCheck.Distance);
                
                m_PlayerContext.FrameVelocity.y = 0f;
                    
                JumpEnded();
            }
        }

        if (!m_PlayerContext.Grounded && m_PlayerContext.FrameVelocity.y < 0f)
        {
            m_PlayerContext.Falling = true;
            
            if (!m_RaisedFallingEvent)
            {
                m_Controller.RaisePlayerFallingEvent();

                m_RaisedFallingEvent = true;
            }
        }
        else
        {
            m_PlayerContext.Falling = false;

            if (m_RaisedFallingEvent)
            {
                m_RaisedFallingEvent = false;
            }
        }
    }
    
    private void ExecuteJump()
    {
        if (!m_RaisedJumpingEvent)
        {
            m_RaisedJumpingEvent = true;
            
            m_Controller.RaiseJumpEvent();
        }
        
        // give initial impulse so player will have minimum jump height
        if (m_PlayerContext.FrameVelocity.y < m_Config.JumpStartImpulse)
        {
            m_PlayerContext.FrameVelocity.y = m_Config.JumpStartImpulse;
        }
        
        m_PlayerContext.FrameVelocity.y = Mathf.MoveTowards(
            m_PlayerContext.FrameVelocity.y,
            m_Config.MaxJumpVelocity,
            m_Config.JumpPower * Time.fixedDeltaTime);

        // end jump
        if (m_PlayerContext.FrameVelocity.y >= m_Config.MaxJumpVelocity)
        {
            JumpEnded();
        }
    }
    
    private void JumpEnded()
    {
        m_PlayerContext.Jumping = false;
        m_RaisedJumpingEvent = false;
    }
}
