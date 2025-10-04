using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IUpdate, ILogable
{
    private const int INPUT_UPDATE_PRIORITY = -999;
    
    [SerializeField] private List<PlayerCollider> m_PlayerColliders;
    [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
    [SerializeField] private Rigidbody2D m_Rigidbody;
    [SerializeField] private InputManager m_InputManager;
    [SerializeField] private UpdateManager m_UpdateManager;
    
    [field: SerializeField] public bool EnableLogging { get; set; }
    
    private PlayerContext m_PlayerContext;
    private Vector2 m_Velocity;
    
    public int UpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }
    
    
    private void Start()
    {
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
        
        for (int i = 0; i < m_PlayerColliders.Count; i++)
        {
            m_PlayerColliders[i].Initialize();
        }
        
        m_PlayerContext = new PlayerContext();
        
        EnableUpdate = true;
    }
    
    public void OnUpdate(float deltaTime)
    {
        FrameInput input = m_InputManager.FrameInput;

        if (input.JumpPressed)
        {
            Logger.Log(this, "Junmping");
        }

        if (input.JumpHeld)
        {
            Logger.Log(this, "Junmp helllllld");
        }

        if (input.JumpReleased)
        {
            Logger.Log(this, "Junmp released");
        }
    }

    private void OnDestroy()
    {
        EnableUpdate = true;
        
        m_UpdateManager.RemoveFromUpdate(this);
    }
}
