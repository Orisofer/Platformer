using System.Collections.Generic;
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
        m_CollisionDetection = new PlayerCollisionDetection(this, true);

        ICollisionDetectionStrategy groundDetection = new GroundOneRay(m_PlayerContext, true);
        
        m_CollisionDetection.AddCheck(groundDetection);
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateJumpParameters();
    }

    private void UpdateJumpParameters()
    {
        if (m_InputManager.FrameInput.JumpPressed)
        {
            m_PlayerContext.TimeLeftTheGround = m_InputManager.FrameInput.Time;

            if (!m_PlayerContext.HasJumpToConsume)
            {
                m_PlayerContext.HasJumpToConsume = true;
            }
        }
    }

    public void OnFixedUpdate()
    {
        StoreLastFrameState();
        HandleGroundState();
        HandleGravity();
        ApplyMovement();
    }

    private void StoreLastFrameState()
    {
        m_PlayerContext.LastFrameVelocity = m_Rigidbody.linearVelocity;
    }

    private void HandleGroundState()
    {
        CollisionDetectionResult groundCheck = m_CollisionDetection.Perform<GroundOneRay>();

        // we hit the ground
        if (!m_PlayerContext.Grounded && groundCheck.Collided)
        {
            m_PlayerContext.Grounded = true;
            
            if (SnapToGround(groundCheck))
            {
                m_PlayerContext.ThisFrameVelocity.y = 0f;
            }
        }
        // we are no longer on the ground (fall or jump)
        else if (m_PlayerContext.Grounded && !groundCheck.Collided)
        {
            m_PlayerContext.Grounded = false;
            m_PlayerContext.TimeLeftTheGround = m_InputManager.FrameInput.Time;
        }
    }

    private bool SnapToGround(CollisionDetectionResult collisionResult)
    {
        float playerHalfSize = m_BoxCollider.bounds.extents.y;
        float groundHalfSize = collisionResult.CollidedTransform.transform.localScale.y * 0.5f;
        
        float playerOrigin = transform.position.y - playerHalfSize;
        float groundOrigin = collisionResult.CollidedTransform.transform.position.y + groundHalfSize;
        
        float actualPlayerGroundDistance = playerOrigin - groundOrigin;

        if (actualPlayerGroundDistance > 0.001f)
        {
            transform.position = transform.position.Add(y : -actualPlayerGroundDistance);
            return true;
        }

        return false;
    }

    private void HandleJumpState()
    {
        if (!m_PlayerContext.HasJumpToConsume) return;

        if (m_PlayerContext.Grounded)
        {
            ExecuteJump();
        }
    }
    
    private void ExecuteJump()
    {
        m_PlayerContext.Jumping = true;
        m_PlayerContext.ThisFrameVelocity.y = m_PlayerControllerConfiguration.JumpPower;
    }
    
    private void HandleGravity()
    {
        if (m_PlayerContext.Grounded && m_PlayerContext.ThisFrameVelocity.y <= 0f)
        {
            m_PlayerContext.ThisFrameVelocity.y = 0f;
        }
        else
        {
            float airGravity = m_PlayerControllerConfiguration.FallAcceleration;
            float maxFallSpeed = m_PlayerControllerConfiguration.MaxFallSpeed;

            m_PlayerContext.ThisFrameVelocity.y = Mathf.MoveTowards(
                m_PlayerContext.ThisFrameVelocity.y,
                -maxFallSpeed,
                airGravity * Time.fixedDeltaTime);
        }
    }
    
    private void ApplyMovement()
    {
        m_Rigidbody.linearVelocity = m_PlayerContext.ThisFrameVelocity;
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
        m_UpdateManager.RemoveFromUpdate(this);
    }
}