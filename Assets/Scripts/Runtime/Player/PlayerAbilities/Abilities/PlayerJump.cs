using UnityEngine;

public class PlayerJump : PlayerAbility
{
    private float m_JumpBufferTimer;
    private bool m_RaisedJumpingEvent;
    private bool m_RaisedFallingEvent;
    
    public override void OnUpdate(float deltaTime)
    {
        // if *by design* we want to disable jump entirely to the player
        if (m_Config.MaxJumps == 0)
        {
            return;
        }
        
        if (m_PlayerContext.Jumping && !m_PlayerContext.JumpHeld)
        {
            m_PlayerContext.Jumping = false;
            
            return;
        }
        
        if (m_PlayerContext.JumpPressed)
        {
            // jump is allowed by regular params
            if (AllowJump())
            {
                JumpStarted();
            }
            // jump was pressed while player still in the air, buffer the request in a timer
            else
            {
                m_JumpBufferTimer = m_Config.JumpBuffer;
            }
        }
        else
        {
            // constantly decrements the buffer timer
            m_JumpBufferTimer -= deltaTime;

            if (m_JumpBufferTimer <= 0f)
            {
                m_JumpBufferTimer = 0f;
            }
        }
    }
    
    private bool AllowJump()
    {
        // check coyote time for grace jump
        if (m_PlayerContext.CoyoteTime > 0f && m_PlayerContext.AvailableJumps > 0)
        {
            return true;
        }
        
        // check if jump was pressed before landed (buffer is not 0f)
        if (m_PlayerContext.Grounded && m_JumpBufferTimer > 0f)
        {
            // clear the buffer
            m_JumpBufferTimer = 0f;
            
            return true;
        }
        
        // the player fell without jumping - let him only perform double jump if he has
        if (m_PlayerContext.Falling && m_PlayerContext.AvailableJumps == m_Config.MaxJumps)
        {
            m_PlayerContext.AvailableJumps--;
        }
        
        // normal check for when player on ground and wants to start jump
        if (m_PlayerContext.Grounded || m_PlayerContext.AvailableJumps > 0)
        {
            return true;
        }
        
        return false;
    }
    
    private void JumpStarted()
    {
        m_PlayerContext.Jumping = true;
        m_PlayerContext.Grounded = false;
        m_PlayerContext.AvailableJumps--;

        if (m_PlayerContext.AvailableJumps <= 0)
        {
            m_PlayerContext.AvailableJumps = 0;
        }
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
            ref readonly CollisionDetectionResult ceilingCheck = ref m_CollisionDetection.CeilingCheck();

            if (ceilingCheck)
            {
                if (m_CollisionDetection.SnapToCeiling(m_BoxCollider, ceilingCheck.CollidedTransform))
                {
                    m_PlayerContext.FrameVelocity.y = 0f;
                    
                    JumpEnded();
                }
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
