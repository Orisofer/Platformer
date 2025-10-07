using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate, ILogable
{
    private const int INPUT_UPDATE_PRIORITY = -999;
    
    [SerializeField] private List<PlayerCollider> m_PlayerColliders;
    [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private InputManager m_InputManager;
    [SerializeField] private UpdateManager m_UpdateManager;
    
    [field: SerializeField] public bool EnableLogging { get; set; }
    
    private PlayerCollisionDetection m_CollisionDetection;
    private PlayerContext m_PlayerContext;
    private Vector2 m_Velocity;
    
    public int UpdatePriority { get; set; }
    public int FixedUpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }
    public bool EnableFixedUpdate { get; set; }
    
    private void Start()
    {
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
        m_UpdateManager.AddToFixedUpdate(this);
        
        for (int i = 0; i < m_PlayerColliders.Count; i++)
        {
            m_PlayerColliders[i].Initialize();
        }
        
        m_PlayerContext = new PlayerContext();

        InitializeCollisionDetections();

        SetUpdate(true);
    }

    private void InitializeCollisionDetections()
    {
        m_CollisionDetection = new PlayerCollisionDetection(this, true);

        ICollisionDetectionStrategy groundDetection =
            new CollisionDetectionGroundRaycasts(m_PlayerColliders[0].Collider, true, true);
        
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
            m_PlayerContext.TimeJumpWasPressed = m_InputManager.FrameInput.Time;

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
        CollisionDetectionResult groundCheck = m_CollisionDetection.Perform<CollisionDetectionGroundRaycasts>();

        // we hit the ground
        if (!m_PlayerContext.Grounded && groundCheck.Collided)
        {
            m_PlayerContext.Grounded = true;
        }
        
        // we are no longer on the ground (fall or jump)
        if (m_PlayerContext.Grounded && !groundCheck.Collided)
        {
            m_PlayerContext.Grounded = false;
        }
    }
    
    private void HandleJumpState()
    {
        if (m_PlayerContext.Grounded && m_PlayerContext.HasJumpToConsume)
        {
            PerformJump();
        }
        else if (m_PlayerContext.Grounded && m_PlayerContext.Jumping)
        {
            m_PlayerContext.Jumping = false;
        }
    }
    
    private void PerformJump()
    {
        m_PlayerContext.HasJumpToConsume = false;
        m_PlayerContext.Jumping = true;
        m_PlayerContext.TimeJumpWasPressed = 0;
        
        m_Velocity.y = m_PlayerControllerConfiguration.JumpPower;
    }
    
    private void HandleGravity()
    {
        if (!m_PlayerContext.Grounded && m_PlayerContext.Jumping)
        {
            float airGravity = m_PlayerControllerConfiguration.FallAcceleration;
            float maxFallSpeed = m_PlayerControllerConfiguration.MaxFallSpeed;

            m_Velocity.y = Mathf.MoveTowards(m_Velocity.y, -maxFallSpeed, airGravity * Time.fixedDeltaTime);
        }
        else
        {
            if (!m_PlayerContext.Jumping)
            {
                m_Velocity.y = 0;
            }
        }
    }
    
    private void ApplyMovement()
    {
        m_Rigidbody.linearVelocity = m_Velocity;
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
