using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate, ILogable
{
    private const int INPUT_UPDATE_PRIORITY = -999;
    private const float SKIN_WIDTH = 0.05f;
    
    [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private BoxCollider2D m_BoxCollider;
    [SerializeField] private InputManager m_InputManager;
    [SerializeField] private UpdateManager m_UpdateManager;
    [SerializeField] private PlayerContext m_PlayerContext;
    [SerializeField] private LayerMask m_PlayerLayer;
    
    [field: SerializeField] public bool EnableLogging { get; set; }
    [field: SerializeField] public bool EnableCollisionVisualizers { get; set; }
    
    private PlayerCollisionDetection m_CollisionDetection;

    private float m_JumpBufferTimer;
    private float m_CoyoteTimer;
    
    public int UpdatePriority { get; set; }
    public int FixedUpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }
    public bool EnableFixedUpdate { get; set; }
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        InitializePlayerContext();
        InitializeCollisionDetections();
        InitializeUpdate();
    }

    private void InitializeUpdate()
    {
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
        m_UpdateManager.AddToFixedUpdate(this);
        
        SetUpdate(true);
    }

    private void InitializePlayerContext()
    {
        m_PlayerContext = new PlayerContext();
        m_PlayerContext.ColliderBody = m_BoxCollider;
        m_PlayerContext.PlayerLayer = m_PlayerLayer;
        m_PlayerContext.SkinWidth = SKIN_WIDTH;
    }

    private void InitializeCollisionDetections()
    {
        m_CollisionDetection = new PlayerCollisionDetection(this, m_PlayerContext, EnableLogging, EnableCollisionVisualizers);
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateJumpParameters(deltaTime);
    }

    private void UpdateJumpParameters(float deltaTime)
    {
        // if *by design* we want to disable jump entirely to the player
        if (m_PlayerControllerConfiguration.MaxJumps == 0)
        {
            return;
        }
        
        if (m_PlayerContext.Jumping && !m_InputManager.FrameInput.JumpHeld)
        {
            m_PlayerContext.Jumping = false;
        }
        
        if (m_InputManager.FrameInput.JumpPressed)
        {
            // jump is allowed by regular params
            if (AllowJump())
            {
                JumpStarted();
            }
            // jump was pressed while player still in the air, buffer the request in a timer
            else
            {
                m_JumpBufferTimer = m_PlayerControllerConfiguration.JumpBuffer;
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

        if (m_PlayerContext.Falling)
        {
            m_CoyoteTimer -= deltaTime;
            
            if (m_CoyoteTimer <= 0f)
            {
                m_CoyoteTimer = 0f;
            }
        }
    }

    private bool AllowJump()
    {
        // check coyote time for grace jump
        if (m_CoyoteTimer > 0f && m_PlayerContext.AvailableJumps > 0)
        {
            return true;
        }
        
        // the player fell without jumping - let him only perform double jump if he has
        if (m_PlayerContext.Falling && m_PlayerContext.AvailableJumps == m_PlayerControllerConfiguration.MaxJumps)
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

    public void OnFixedUpdate()
    {
        StoreLastFrameState();
        ResetFrameContext();
        
        HandleGroundState();
        HandleHorizontalState();
        HandleJumpState();
        HandleDiagonalLand();
        HandleGravity();
        
        ApplyMovement();
    }

    private void HandleGroundState()
    {
        // stop ground detection when player is upward moving
        if (m_PlayerContext.FrameVelocity.y > 0 || m_PlayerContext.Jumping) return;
        
        ref readonly CollisionDetectionResult groundCheck = ref m_CollisionDetection.GroundCheck();

        // we hit the ground
        if (!m_PlayerContext.Grounded && groundCheck)
        {
            ResetJumpParameters();
            
            m_PlayerContext.CollisionPattern |= groundCheck.HitPattern;
            m_PlayerContext.LastGround = groundCheck.CollidedTransform;
            
            if (m_CollisionDetection.SnapToGround(m_BoxCollider, groundCheck.CollidedTransform))
            {
                m_PlayerContext.FrameVelocity.y = 0f;
            }
        }
        // we are no longer on the ground (fall or jump)
        else if (m_PlayerContext.Grounded && !groundCheck)
        {
            m_PlayerContext.Grounded = false;
            m_PlayerContext.LastGround = null;
            m_PlayerContext.CollisionPattern = 0;
            m_PlayerContext.TimeLeftTheGround = m_InputManager.FrameInput.Time;
            m_CoyoteTimer = m_PlayerControllerConfiguration.CoyoteTime;
        }
    }

    // player touched the ground
    private void ResetJumpParameters()
    {
        m_PlayerContext.Grounded = true;
        m_PlayerContext.TimeLeftTheGround = 0;
        m_PlayerContext.AvailableJumps = m_PlayerControllerConfiguration.MaxJumps;
    }

    private void HandleJumpState()
    {
        // check if jump was pressed before landed (buffer is not 0f)
        if (m_PlayerContext.Grounded && m_JumpBufferTimer > 0f)
        {
            // clear the buffer
            m_JumpBufferTimer = 0f;
            
            JumpStarted();
        }
        
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
        }
        else
        {
            m_PlayerContext.Falling = false;
        }
    }
    
    private void ExecuteJump()
    {
        // give initial impulse so player will have minimum jump height
        if (m_PlayerContext.FrameVelocity.y < m_PlayerControllerConfiguration.JumpStartImpulse)
        {
            m_PlayerContext.FrameVelocity.y = m_PlayerControllerConfiguration.JumpStartImpulse;
        }
        
        m_PlayerContext.FrameVelocity.y = Mathf.MoveTowards(
            m_PlayerContext.FrameVelocity.y,
            m_PlayerControllerConfiguration.MaxJumpVelocity,
            m_PlayerControllerConfiguration.JumpPower * Time.fixedDeltaTime);

        // end jump
        if (m_PlayerContext.FrameVelocity.y >= m_PlayerControllerConfiguration.MaxJumpVelocity)
        {
            JumpEnded();
        }
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

    private void JumpEnded()
    {
        m_PlayerContext.Jumping = false;
    }

    private void HandleHorizontalState()
    {
        if (m_InputManager.FrameInput.Direction == Vector2.zero)
        {
            float deceleration;

            if (m_PlayerContext.Grounded)
            {
                deceleration = m_PlayerControllerConfiguration.GroundDeceleration;
            }
            else
            {
                deceleration = m_PlayerControllerConfiguration.AirDeceleration;
            }
            
            m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.x,
                0,
                deceleration * Time.fixedDeltaTime);
        }
        else
        {
            float direction = m_InputManager.FrameInput.Direction.x;
            
            if (direction < 0)
            {
                ref readonly CollisionDetectionResult leftWallCheck = ref m_CollisionDetection.LeftWallCheck();

                if (leftWallCheck)
                {
                    m_PlayerContext.FrameVelocity.x = 0f;
                    m_PlayerContext.CollisionPattern |= leftWallCheck.HitPattern;
                    
                    return;
                }
            }
            
            if (direction > 0)
            {
                ref readonly CollisionDetectionResult rightWallCheck = ref m_CollisionDetection.RightWallCheck();

                if (rightWallCheck)
                {
                    m_PlayerContext.FrameVelocity.x = 0f;
                    m_PlayerContext.CollisionPattern |= rightWallCheck.HitPattern;
                    
                    return;
                }
            }
            
            float maxHorizontalSpeed = m_PlayerControllerConfiguration.MaxSpeed * direction;
            float acceleration = m_PlayerControllerConfiguration.Acceleration;
            
            m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.x,
                maxHorizontalSpeed,
                acceleration * Time.fixedDeltaTime);
        }
    }
    
    private void HandleGravity()
    {
        if (m_PlayerContext.Grounded && m_PlayerContext.FrameVelocity.y <= 0f)
        {
            m_PlayerContext.FrameVelocity.y = 0f;
        }
        else
        {
            float airGravity = m_PlayerControllerConfiguration.FallAcceleration;
            float maxFallSpeed = m_PlayerControllerConfiguration.MaxFallSpeed;

            m_PlayerContext.FrameVelocity.y = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.y,
                -maxFallSpeed,
                airGravity * Time.fixedDeltaTime);
        }
    }
    
    private void ApplyMovement()
    {
        m_Rigidbody.linearVelocity = m_PlayerContext.FrameVelocity;
    }
    
    private void HandleDiagonalLand()
    {
        float direction = m_InputManager.FrameInput.Direction.x;
        byte collisionPattern = m_PlayerContext.CollisionPattern;
    
        if (direction > 0)
        {
            if (collisionPattern == CollisionDetectionResult.RightDiagonalPattern)
            {
                m_CollisionDetection.SnapToGround(m_BoxCollider, m_PlayerContext.LastGround);
            }
        }
        else if (direction < 0)
        {
            if (collisionPattern == CollisionDetectionResult.LeftDiagonalPattern)
            {
                m_CollisionDetection.SnapToGround(m_BoxCollider, m_PlayerContext.LastGround);
            }
        }
    }
    
    private void StoreLastFrameState()
    {
        m_PlayerContext.FrameVelocity = m_Rigidbody.linearVelocity;
    }
    
    private void ResetFrameContext()
    {
        m_PlayerContext.CollisionPattern = 0;
    }

    private void SetUpdate(bool enabled)
    {
        EnableUpdate = enabled;
        EnableFixedUpdate = enabled;
    }

    private void OnDestroy()
    {
        SetUpdate(false);
        
        m_UpdateManager.RemoveFromUpdate(this);
        m_UpdateManager.RemoveFromFixedUpdate(this);
    }
}