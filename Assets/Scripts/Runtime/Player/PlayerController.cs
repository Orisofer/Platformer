using System;
using OriGame.Core;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerController, IUpdate, IFixedUpdate
{
    private const int PLAYER_UPDATE_PRIORITY = -999;
    private const float SKIN_WIDTH = 0.05f;
    
    [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private BoxCollider2D m_BoxCollider;
    [SerializeField] private PlayerContext m_PlayerContext;
    [SerializeField] private LayerMask m_PlayerLayer;
    
    private InputManager m_InputManager;
    private UpdateManager m_UpdateManager;
    private PlayerCollisionDetection m_CollisionDetection;
    private IGameLogger m_Logger;
    
    private float m_JumpBufferTimer;
    private float m_CoyoteTimer;
    private bool m_RaisedFallingEvent;
    private bool m_RaisedJumpingEvent;
    private bool m_RaisedGroundedEvent;
    
    public event Action<PlayerContext> PlayerJumped;
    public event Action<PlayerContext> PlayerFalling;
    public event Action<PlayerContext> PlayerGrounded;
    public PlayerControllerConfiguration PlayerConfiguration => m_PlayerControllerConfiguration;
    public PlayerContext PlayerContext => m_PlayerContext;
    public Rigidbody2D Rigidbody => m_Rigidbody;
    [field: SerializeField] public bool EnableCollisionVisualizers { get; set; }
    public int UpdatePriority { get; set; }
    public int FixedUpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }
    public bool EnableFixedUpdate { get; set; }

    public void Initialize(IServiceLocator serviceLocator)
    {
        if (!InitializeServices(serviceLocator))
        {
            return;
        }
        
        InitializePlayerContext();
        InitializeCollisionDetections();
        InitializeUpdate();
    }

    private bool InitializeServices(IServiceLocator serviceLocator)
    {
        m_Logger = serviceLocator.GetService<IGameLogger>();
        m_InputManager = serviceLocator.GetService<InputManager>();
        m_UpdateManager  = serviceLocator.GetService<UpdateManager>();

        if (m_InputManager == null)
        {
            m_Logger.LogError("[ Player Controller ] Input Manager could not be fetched from service locator");
            return false;
        }

        if (m_UpdateManager == null)
        {
            m_Logger.LogError("[ Player Controller ] Update Manager could not be fetched from service locator");
            return false;
        }

        return true;
    }

    private void InitializeUpdate()
    {
        UpdatePriority = PLAYER_UPDATE_PRIORITY;
        
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
        m_PlayerContext.FacingRight = true;
    }

    private void InitializeCollisionDetections()
    {
        m_CollisionDetection = new PlayerCollisionDetection(this, m_PlayerContext, EnableCollisionVisualizers);
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateWalkingParameters();
        UpdateJumpParameters(deltaTime);
    }

    private void UpdateWalkingParameters()
    {
        if (m_InputManager.FrameInput.Direction == Vector2.zero)
        {
            m_PlayerContext.Walking = false;
        }
        else
        {
            m_PlayerContext.Walking = true;
        }
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
            
            if (m_CollisionDetection.SnapToGround(groundCheck.Distance))
            {
                m_PlayerContext.FrameVelocity.y = 0f;
            }
            
            PlayerGrounded?.Invoke(m_PlayerContext);
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
        m_PlayerContext.Falling = false;
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
            
            if (!m_RaisedFallingEvent)
            {
                PlayerFalling?.Invoke(m_PlayerContext);

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
            
            PlayerJumped?.Invoke(m_PlayerContext);
        }
        
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
        m_RaisedJumpingEvent = false;
    }

    private void HandleHorizontalState()
    {
        if (!m_PlayerContext.Walking)
        {
            float deceleration = m_PlayerControllerConfiguration.HorizontalDeceleration;
            
            m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                m_PlayerContext.FrameVelocity.x,
                0,
                deceleration * Time.fixedDeltaTime);
            
            // player decelerating
            if (m_PlayerContext.FrameVelocity.x != 0)
            {
                float direction = m_InputManager.FrameInput.Direction.x;
                
                if (HandleHorizontalCollisions(direction))
                {
                    m_PlayerContext.FrameVelocity.x = 0f;
                }
            }
        }
        else
        {
            float direction = m_InputManager.FrameInput.Direction.x;

            if (HandleHorizontalCollisions(direction))
            {
                m_PlayerContext.FrameVelocity.x = 0f;
            }
            else
            {
                float maxHorizontalSpeed = m_PlayerControllerConfiguration.MaxSpeed * direction;
                float acceleration = m_PlayerControllerConfiguration.Acceleration;
            
                m_PlayerContext.FrameVelocity.x = Mathf.MoveTowards(
                    m_PlayerContext.FrameVelocity.x,
                    maxHorizontalSpeed,
                    acceleration * Time.fixedDeltaTime);
                
            }
        }
    }

    private bool HandleHorizontalCollisions(float direction)
    { 
        // moving left
        if (direction < 0)
        {
            if (m_PlayerContext.FacingRight)
            {
                FlipX(false);
            }
                
            ref readonly CollisionDetectionResult leftWallCheck = ref m_CollisionDetection.LeftWallCheck();

            if (leftWallCheck)
            {
                m_CollisionDetection.SnapToWall(Vector2.right, leftWallCheck.Distance);
                    
                m_PlayerContext.FrameVelocity.x = 0f;
                m_PlayerContext.CollisionPattern |= leftWallCheck.HitPattern;
                    
                return true;
            }
        }
            
        // moving right
        if (direction > 0)
        {
            if (!m_PlayerContext.FacingRight)
            {
                FlipX(true);
            }
                
            ref readonly CollisionDetectionResult rightWallCheck = ref m_CollisionDetection.RightWallCheck();

            if (rightWallCheck)
            {
                m_CollisionDetection.SnapToWall(Vector2.left, rightWallCheck.Distance);
                    
                m_PlayerContext.FrameVelocity.x = 0f;
                m_PlayerContext.CollisionPattern |= rightWallCheck.HitPattern;

                return true;
            }
        }

        return false;
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

    private void FlipX(bool facingRight)
    {
        if (facingRight)
        {
            Vector3 rotation = transform.rotation.eulerAngles.Replace(y: 0f);
            transform.localRotation = Quaternion.Euler(rotation);
        }
        else
        {
            Vector3 rotation = transform.rotation.eulerAngles.Replace(y: 180f);
            transform.localRotation = Quaternion.Euler(rotation);
        }

        m_PlayerContext.FacingRight = facingRight;
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