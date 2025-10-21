using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate, ILogable
{
    private const int INPUT_UPDATE_PRIORITY = -999;
    
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
        m_PlayerContext.SkinWidth = 0.05f;
    }

    private void InitializeCollisionDetections()
    {
        m_CollisionDetection = new PlayerCollisionDetection(this, m_PlayerContext, EnableLogging, EnableCollisionVisualizers);
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateJumpParameters();
    }

    private void UpdateJumpParameters()
    {
        if (m_InputManager.FrameInput.JumpPressed)
        {
            if (m_PlayerContext.Grounded && m_PlayerContext.AvailableJumps > 0)
            {
                m_PlayerContext.AllowJump = true;
            }
        }
    }

    public void OnFixedUpdate()
    {
        StoreLastFrameState();
        
        HandleGroundState();
        HandleHorizontalState();
        HandleJumpState();
        HandleDiagonalLand();
        HandleGravity();
        ApplyMovement();
    }

    private void StoreLastFrameState()
    {
        m_PlayerContext.FrameVelocity = m_Rigidbody.linearVelocity;
    }

    private void HandleGroundState()
    {
        CollisionDetectionResult groundCheck = m_CollisionDetection.GroundCheck();

        // we hit the ground
        if (!m_PlayerContext.Grounded && groundCheck)
        {
            m_PlayerContext.Grounded = true;
            m_PlayerContext.CollisionPattern = m_PlayerContext.CollisionPattern |= groundCheck.HitPattern;
            m_PlayerContext.LastGround = groundCheck.CollidedTransform;

            ResetJumpParameters();
            
            if (SnapToGround(groundCheck))
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
        }
    }

    private void ResetJumpParameters()
    {
        m_PlayerContext.Jumping = false;
        m_PlayerContext.TimeLeftTheGround = 0;
        m_PlayerContext.AvailableJumps = 1;
    }

    private void HandleJumpState()
    {
        if (m_PlayerContext.Jumping)
        {
            CollisionDetectionResult ceilingCheck = m_CollisionDetection.CeilingCheck();

            if (ceilingCheck)
            {
                if (SnapToCeiling(ceilingCheck))
                {
                    m_PlayerContext.FrameVelocity.y = 0f;
                }
            }
        }
        
        if (!m_PlayerContext.AllowJump) return;
        if (m_PlayerContext.AvailableJumps == 0) return;
        
        ExecuteJump();
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
                CollisionDetectionResult leftWallCheck = m_CollisionDetection.LeftWallCheck();

                if (leftWallCheck)
                {
                    m_PlayerContext.FrameVelocity.x = 0f;
                    m_PlayerContext.CollisionPattern = m_PlayerContext.CollisionPattern |= leftWallCheck.HitPattern;
                    
                    return;
                }
            }
            
            if (direction > 0)
            {
                CollisionDetectionResult rightWallCheck = m_CollisionDetection.RightWallCheck();

                if (rightWallCheck)
                {
                    m_PlayerContext.FrameVelocity.x = 0f;
                    m_PlayerContext.CollisionPattern = m_PlayerContext.CollisionPattern |= rightWallCheck.HitPattern;
                    
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
    
    private void ExecuteJump()
    {
        m_PlayerContext.AvailableJumps--;

        if (m_PlayerContext.AvailableJumps == 0)
        {
            m_PlayerContext.AllowJump = false;
        }
        
        m_PlayerContext.Jumping = true;
        m_PlayerContext.FrameVelocity.y = m_PlayerControllerConfiguration.JumpPower;
    }
    
    private void HandleDiagonalLand()
    {
        float direction = m_InputManager.FrameInput.Direction.x;
        byte collisionPattern = m_PlayerContext.CollisionPattern;
    
        if (direction > 0)
        {
            if (collisionPattern == CollisionDetectionResult.RightDiagonalPattern)
            {
                SnapToGround(m_PlayerContext.LastGround);
            }
        }
        else if (direction < 0)
        {
            if (collisionPattern == CollisionDetectionResult.LeftDiagonalPattern)
            {
                SnapToGround(m_PlayerContext.LastGround);
            }
        }
    }

    private bool SnapToGround(Transform playerContextLastGround)
    {
        float playerHalfSize = m_BoxCollider.bounds.extents.y;
        float groundHalfSize = playerContextLastGround.localScale.y * 0.5f;
        
        float playerOrigin = transform.position.y - playerHalfSize;
        float groundOrigin = playerContextLastGround.position.y + groundHalfSize;
        
        float actualPlayerGroundDistance = playerOrigin - groundOrigin;

        if (actualPlayerGroundDistance > 0.001f)
        {
            transform.position = transform.position.Add(y : -actualPlayerGroundDistance);
            return true;
        }
        
        if (actualPlayerGroundDistance < 0.001f)
        {
            transform.position = transform.position.Add(y : -actualPlayerGroundDistance);
            return true;
        }

        return false;
    }

    private bool SnapToGround(CollisionDetectionResult collisionResult)
    {
        return SnapToGround(collisionResult.CollidedTransform);
    }
    
    private bool SnapToCeiling(CollisionDetectionResult collisionResult)
    {
        float playerHalfSize = m_BoxCollider.bounds.extents.y;
        float ceilingHalfSize = collisionResult.CollidedTransform.transform.localScale.y * 0.5f;
        
        float playerOrigin = transform.position.y + playerHalfSize;
        float ceilingOrigin = collisionResult.CollidedTransform.transform.position.y - ceilingHalfSize;
        
        float actualPlayerCeilingDistance = ceilingOrigin - playerOrigin;

        if (actualPlayerCeilingDistance > 0.001f)
        {
            transform.position = transform.position.Add(y : actualPlayerCeilingDistance);
            return true;
        }

        return false;
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