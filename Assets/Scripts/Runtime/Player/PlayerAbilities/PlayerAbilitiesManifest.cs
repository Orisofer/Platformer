using System.Collections.Generic;

namespace OriGame.Player
{
    public class PlayerAbilitiesManifest
    {
        private List<PlayerAbility> m_Abilities;
        private readonly PlayerController m_PlayerController;
    
        // Abilities
        // -----------------------------------------------------------------
        private PlayerJump m_PlayerJump;
        private PlayerHorizontalMove m_PlayerHorizontalMove;
        private PlayerDash m_PlayerDash;
        // -----------------------------------------------------------------

        public PlayerAbilitiesManifest(PlayerController playerController)
        {
            m_PlayerController  = playerController;
        }

        public List<PlayerAbility> InitializeAbilities()
        {
            List<PlayerAbility> abilities = new List<PlayerAbility>();

            m_PlayerJump = new PlayerJump(m_PlayerController);
            m_PlayerHorizontalMove = new PlayerHorizontalMove(m_PlayerController);
            m_PlayerDash = new PlayerDash(m_PlayerController);
        
            abilities.Add(m_PlayerJump);
            abilities.Add(m_PlayerHorizontalMove);
            abilities.Add(m_PlayerDash);
        
            return abilities;
        }
    }
}

