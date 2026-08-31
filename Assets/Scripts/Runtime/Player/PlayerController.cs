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
    private CollisionDetection m_CollisionDetection;
    private IGameLogger m_Logger;
    
    // Abilities
    // ----------------
    private PlayerJump m_PlayerJump;
    private PlayerHorizontalMove m_PlayerHorizontalMove;
    
    
    public event Action<PlayerContext> PlayerJumped;
    public event Action<PlayerContext> PlayerFalling;
    public event Action<PlayerContext> PlayerGrounded;
    public PlayerControllerConfiguration PlayerConfiguration => m_PlayerControllerConfiguration;
    public PlayerContext PlayerContext => m_PlayerContext;
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
        InitializePlayerAbilities();
        InitializeUpdate();
        
        m_Logger.Log("[Player Controller] Initialized");
    }

    private void InitializePlayerAbilities()
    {
        m_PlayerHorizontalMove = new PlayerHorizontalMove(this);
        m_PlayerJump =  new PlayerJump(this);
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
        m_CollisionDetection = new CollisionDetection(this, m_PlayerContext, EnableCollisionVisualizers);
    }

    public void OnUpdate(float deltaTime)
    {
        UpdatePlayerInput();
        
        m_PlayerHorizontalMove.OnUpdate(deltaTime);
        m_PlayerJump.OnUpdate(deltaTime);
        
        UpdateCoyoteTime(deltaTime);
    }

    private void UpdatePlayerInput()
    {
        if (m_InputManager.FrameInput.JumpPressed)
        {
            m_PlayerContext.JumpPressed = true;
        }
        else
        {
            m_PlayerContext.JumpPressed = false;
        }

        if (m_InputManager.FrameInput.JumpHeld)
        {
            m_PlayerContext.JumpHeld = true;
        }
        else
        {
            m_PlayerContext.JumpHeld = false;
        }

        m_PlayerContext.HorizontalInputDir = m_InputManager.FrameInput.Direction;
    }
    
    private void UpdateCoyoteTime(float deltaTime)
    {
        if (m_PlayerContext.Falling)
        {
            m_PlayerContext.CoyoteTime -= deltaTime;
            
            if (m_PlayerContext.CoyoteTime <= 0f)
            {
                m_PlayerContext.CoyoteTime = 0f;
            }
        }
    }

    public void OnFixedUpdate()
    {
        StoreLastFrameVelocity();
        ResetFrameCollisionPattern();

        UpdateCollisions();
        
        HandleGroundState();
        
        m_PlayerHorizontalMove.OnFixedUpdate(Time.fixedDeltaTime);
        m_PlayerJump.OnFixedUpdate(Time.fixedDeltaTime);
        
        HandleDiagonalLand();
        HandleGravity();
        
        ApplyPendingSnap();
        
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
            m_PlayerContext.CoyoteTime = m_PlayerControllerConfiguration.CoyoteTime;
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
    
    private void UpdateCollisions()
    {
        m_PlayerContext.CollisionContext.Ground = m_CollisionDetection.GroundCheck();
        m_PlayerContext.CollisionContext.Ceiling = m_CollisionDetection.CeilingCheck();
        m_PlayerContext.CollisionContext.WallLeft = m_CollisionDetection.LeftWallCheck();
        m_PlayerContext.CollisionContext.WallRight = m_CollisionDetection.RightWallCheck();
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

    public void FlipX(bool facingRight)
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
    
    private void StoreLastFrameVelocity()
    {
        m_PlayerContext.FrameVelocity = m_Rigidbody.linearVelocity;
    }
    
    private void ResetFrameCollisionPattern()
    {
        m_PlayerContext.CollisionPattern = 0;
    }

    private void SetUpdate(bool updateEnabled)
    {
        EnableUpdate = updateEnabled;
        EnableFixedUpdate = updateEnabled;
    }
    
    private void ApplyPendingSnap()
    {
        var snap = m_PlayerContext.SnapRequest;
    
        switch (snap.Direction)
        {
            case SnapDirection.Ground:
                m_CollisionDetection.SnapToGround(snap.Distance);
                break;

            case SnapDirection.Ceiling:
                m_CollisionDetection.SnapToCeiling(m_BoxCollider, snap.Target);
                break;

            case SnapDirection.WallLeft:
                m_CollisionDetection.SnapToWall(Vector2.right, snap.Distance);
                break;

            case SnapDirection.WallRight:
                m_CollisionDetection.SnapToWall(Vector2.left, snap.Distance);
                break;
        }
        
        m_PlayerContext.SnapRequest.Clear();
    }

    public void RaisePlayerFallingEvent()
    {
        PlayerFalling?.Invoke(m_PlayerContext);
    }

    public void RaiseJumpEvent()
    {
        PlayerJumped?.Invoke(m_PlayerContext);
    }

    public void RequestSnap(Transform target, SnapDirection direction, float distance)
    {
        m_PlayerContext.SnapRequest.Target = target; 
        m_PlayerContext.SnapRequest.Direction = direction;
        m_PlayerContext.SnapRequest.Distance = distance;
    }

    private void OnDestroy()
    {
        SetUpdate(false);
        
        m_UpdateManager.RemoveFromUpdate(this);
        m_UpdateManager.RemoveFromFixedUpdate(this);
    }
}