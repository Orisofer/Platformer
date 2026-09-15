using System.Collections.Generic;

namespace OriGame.Player
{
    public class PlayerAbilitiesManifest
    {
        private List<IPlayerAbility> m_Abilities;
        private readonly PlayerController m_PlayerController;
        private readonly PlayerControllerConfiguration m_PlayerControllerConfiguration;
    
        // Abilities
        // -----------------------------------------------------------------
        private PlayerJump m_PlayerJump;
        private PlayerHorizontalMove m_PlayerHorizontalMove;
        private PlayerDash m_PlayerDash;
        // -----------------------------------------------------------------

        public PlayerAbilitiesManifest(PlayerController playerController)
        {
            m_PlayerController  = playerController;
            m_PlayerControllerConfiguration = m_PlayerController.PlayerConfiguration;
        }

        public List<IPlayerAbility> InitializeAbilities()
        {
            List<IPlayerAbility> abilities = new List<IPlayerAbility>();

            m_PlayerJump = new PlayerJump(m_PlayerControllerConfiguration.PlayerJumpConfiguration, m_PlayerController);
            m_PlayerHorizontalMove = new PlayerHorizontalMove(m_PlayerControllerConfiguration.PlayerMoveConfiguration, m_PlayerController);
            m_PlayerDash = new PlayerDash(m_PlayerControllerConfiguration.PlayerDashConfiguration, m_PlayerController);
        
            abilities.Add(m_PlayerJump);
            abilities.Add(m_PlayerHorizontalMove);
            abilities.Add(m_PlayerDash);
        
            return abilities;
        }
    }
}

