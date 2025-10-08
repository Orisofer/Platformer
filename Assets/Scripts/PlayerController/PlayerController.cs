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
    [SerializeField] private LayerMask m_PlayerLayer;
    
    [field: SerializeField] public bool EnableLogging { get; set; }
    
    private PlayerCollisionDetection m_CollisionDetection;
    private PlayerContext m_PlayerContext;
    
    public int UpdatePriority { get; set; }
    public int FixedUpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }
    public bool EnableFixedUpdate { get; set; }
    
    private void Start()
    {
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
        m_UpdateManager.AddToFixedUpdate(this);
        
        m_PlayerContext = new PlayerContext();
        m_PlayerContext.ColliderBody = m_BoxCollider;
        m_PlayerContext.PlayerLayer = m_PlayerLayer;

        InitializeCollisionDetections();

        SetUpdate(true);
    }

    private void InitializeCollisionDetections()
    {
        m_CollisionDetection = new PlayerCollisionDetection(this, true);

        ICollisionDetectionStrategy groundDetection = new CollisionDetectionBoxCast(m_PlayerContext);
        
        m_CollisionDetection.AddCheck(groundDetection);
    }

    public void OnUpdate(float deltaTime)
    {
        HandleJumpParameters();
    }

    private void HandleJumpParameters()
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
        HandleGroundState();
        HandleJumpState();
        HandleGravity();

        ApplyMovement();
    }

    private void HandleGroundState()
    {
        CollisionDetectionResult groundCheck = m_CollisionDetection.Perform<CollisionDetectionBoxCast>();

        // we hit the ground
        if (!m_PlayerContext.Grounded && groundCheck.Collided)
        {
            m_PlayerContext.Grounded = true;
        }
        // we are no longer on the ground (fall or jump)
        else if (m_PlayerContext.Grounded && !groundCheck.Collided)
        {
            m_PlayerContext.Grounded = false;
            m_PlayerContext.TimeLeftTheGround = m_InputManager.FrameInput.Time;
        }
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
        m_PlayerContext.Velocity.y = m_PlayerControllerConfiguration.JumpPower;
    }
    
    private void HandleGravity()
    {
        if (m_PlayerContext.Grounded && m_PlayerContext.Velocity.y <= 0f)
        {
            
        }
        else
        {
            float airGravity = m_PlayerControllerConfiguration.FallAcceleration;
            float maxFallSpeed = m_PlayerControllerConfiguration.MaxFallSpeed;

            m_PlayerContext.Velocity.y = Mathf.MoveTowards(m_PlayerContext.Velocity.y, -maxFallSpeed, airGravity * Time.fixedDeltaTime);
        }
    }
    
    private void ApplyMovement()
    {
        m_Rigidbody.linearVelocity = m_PlayerContext.Velocity;
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
