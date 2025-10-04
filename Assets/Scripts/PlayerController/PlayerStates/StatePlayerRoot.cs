using UnityEngine;
using Core.StateMachine;

public class StatePlayerRoot : State
{
    public readonly StatePlayerGrounded Grounded;
    public readonly StatePlayerAirborne Airborne;

    private readonly PlayerContext m_PlayerContext;
    
    public StatePlayerRoot(StateMachine stateMachine, PlayerContext playerContext) : base(stateMachine, null)
    {
        m_PlayerContext = playerContext;
        
        Grounded = new StatePlayerGrounded(stateMachine, this, m_PlayerContext);
        Airborne = new StatePlayerAirborne(stateMachine, this, m_PlayerContext);
    }

    protected override State GetInitializeState()
    {
        return Grounded;
    }

    protected override State GetTransition()
    {
        if (!m_PlayerContext.Grounded)
        {
            return Airborne;
        }

        return null;
    }
}
