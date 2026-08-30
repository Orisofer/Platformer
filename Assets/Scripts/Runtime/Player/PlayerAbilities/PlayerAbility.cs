using UnityEngine;

public abstract class PlayerAbility
{
    protected PlayerController m_Controller;
    protected PlayerContext m_PlayerContext;
    protected PlayerControllerConfiguration m_Config;
    protected CollisionDetection m_CollisionDetection;
    protected BoxCollider2D m_BoxCollider;

    public bool Enabled { get; set; } = true;

    public virtual void Initialize(PlayerController controller, InputManager inputManager, bool enabled = false)
    {
        m_Controller = controller;
        m_PlayerContext = controller.PlayerContext;
        m_Config = controller.PlayerConfiguration;
        m_CollisionDetection = controller.CollisionDetection;
        m_BoxCollider = controller.BoxCollider;
        
        Enabled  = enabled;
    }

    public abstract void OnUpdate(float deltaTime);
    public abstract void OnFixedUpdate(float fixedDeltaTime);
}