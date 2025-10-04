using System;
using System.Collections.Generic;
using Core.StateMachine;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, IFixedUpdate, ILogable
{
    private const int INPUT_UPDATE_PRIORITY = -999;
    
    [SerializeField] private List<PlayerCollider> m_PlayerColliders;
    [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private InputManager m_InputManager;
    [SerializeField] private UpdateManager m_UpdateManager;
    [SerializeField] private Transform m_PlayerGroundPoint;
    [SerializeField] private PlayerContext m_PlayerContext;
    [SerializeField] private LayerMask m_GroundMask;
    
    [field: SerializeField] public bool EnableLogging { get; set; }

    private StateMachine m_PlayerStateMachine;
    private StatePlayerRoot m_StateMachineRoot;
    private ICollisionCheckStrategy m_CollisionCheckStrategy;
    
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
        
        m_PlayerContext.Rigidbody2D = m_Rigidbody;

        m_CollisionCheckStrategy = new CollisionCheckGround(m_GroundMask, 0.2f);

        m_StateMachineRoot = new StatePlayerRoot(null, m_PlayerContext);

        StateMachineBuilder builder = new StateMachineBuilder(this.gameObject, m_StateMachineRoot);
        
        m_PlayerStateMachine = builder.Build();
        
        EnableUpdate = true;
        EnableFixedUpdate = true;
    }
    
    public void OnUpdate(float deltaTime)
    {
        m_PlayerContext.MoveHorizontal = m_InputManager.FrameInput.Direction;
        m_PlayerContext.JumpPressed = m_InputManager.FrameInput.JumpPressed;

        m_PlayerContext.Grounded = m_CollisionCheckStrategy.CheckCollision(m_PlayerGroundPoint);
        
        m_PlayerStateMachine.Tick(deltaTime);
    }
    
    public void OnFixedUpdate()
    {
        Vector2 velocity = m_PlayerContext.Velocity;
        m_Rigidbody.linearVelocity = velocity;

        m_PlayerContext.Velocity = velocity;
    }

    private void OnDestroy()
    {
        EnableUpdate = false;
        EnableFixedUpdate = false;
        
        m_UpdateManager.RemoveFromUpdate(this);
        m_UpdateManager.RemoveFromFixedUpdate(this);
    }
}
