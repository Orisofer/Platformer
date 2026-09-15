using OriGame.Input;

namespace OriGame.Player
{
    public abstract class PlayerAbility<T> : IPlayerAbility where T : PlayerAbilityConfiguration
    {
    protected PlayerController m_Controller;
    protected PlayerContext m_PlayerContext;
    protected T m_Config;

    public bool Enabled { get; set; } = true;

    protected PlayerAbility(T config, PlayerController controller, bool enabled = false)
    {
        m_Controller = controller;
        m_PlayerContext = controller.PlayerContext;
        m_Config = config;

        Enabled = enabled;
    }

    public abstract void OnUpdate(in FrameInput frameInput, float deltaTime);

    public abstract PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime);
    }
}
    
