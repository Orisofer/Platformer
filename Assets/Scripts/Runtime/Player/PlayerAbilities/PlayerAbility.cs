using UnityEngine;

namespace OriGame.Player
{
    public abstract class PlayerAbility
    {
        protected PlayerController m_Controller;
        protected PlayerContext m_PlayerContext;
        protected PlayerControllerConfiguration m_Config;

        public bool Enabled { get; set; } = true;

        protected PlayerAbility(PlayerController controller, bool enabled = false)
        {
            m_Controller = controller;
            m_PlayerContext = controller.PlayerContext;
            m_Config = controller.PlayerConfiguration;
        
            Enabled  = enabled;
        }

        public abstract void OnUpdate(float deltaTime);
        public abstract void OnFixedUpdate(float fixedDeltaTime);
    }
}
    
