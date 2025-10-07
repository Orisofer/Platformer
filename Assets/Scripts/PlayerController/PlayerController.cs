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

        SetUpdate(true);
    }
    
    public void OnUpdate(float deltaTime)
    {
        
    }
    
    public void OnFixedUpdate()
    {
        
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
