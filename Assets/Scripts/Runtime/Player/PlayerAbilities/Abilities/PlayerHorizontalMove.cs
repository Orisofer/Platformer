using UnityEngine;

public class PlayerHorizontalMove : PlayerAbility
{
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
    
    public override void OnFixedUpdate(float fixedDeltaTime)
    {
        float horizontalInputDir = m_PlayerContext.HorizontalInputDir.x;
        
        // handle walking physics
        if (!m_PlayerContext.Walking)
        {
            float deceleration = m_Config.HorizontalDeceleration;
            
            m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.x,
                0,
                deceleration * fixedDeltaTime);
        }
        else
        {
            float maxHorizontalSpeed = m_Config.MaxSpeed * horizontalInputDir;
            float acceleration = m_Config.Acceleration;
            
            m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.x,
                maxHorizontalSpeed,
                acceleration * Time.fixedDeltaTime);
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
        
        // handle horizontal collisions
        if (m_PlayerContext.FrameVelocity.x != 0)
        {
            float currentPlayerDir = Mathf.Sign(m_PlayerContext.FrameVelocity.x);

            if (HorizontalCollision(currentPlayerDir))
            {
                m_PlayerContext.FrameVelocity.x = 0f;
            }
        }
    }
    
    private bool HorizontalCollision(float direction)
    { 
        // moving left
        if (direction < 0)
        {
            ref readonly CollisionDetectionResult leftWallCheck = ref m_PlayerContext.CollisionContext.WallLeft;

            if (leftWallCheck)
            {
                m_Controller.RequestSnap(leftWallCheck.CollidedTransform, SnapDirection.WallLeft, leftWallCheck.Distance);
                    
                m_PlayerContext.FrameVelocity.x = 0f;
                m_PlayerContext.CollisionPattern |= leftWallCheck.HitPattern;
                    
                return true;
            }
        }
            
        // moving right
        if (direction > 0)
        {
            ref readonly CollisionDetectionResult rightWallCheck = ref m_PlayerContext.CollisionContext.WallRight;

            if (rightWallCheck)
            {
                m_Controller.RequestSnap(rightWallCheck.CollidedTransform, SnapDirection.WallRight, rightWallCheck.Distance);
                    
                m_PlayerContext.FrameVelocity.x = 0f;
                m_PlayerContext.CollisionPattern |= rightWallCheck.HitPattern;

                return true;
            }
        }

        return false;
    }
}
