using UnityEngine;
using Core.StateMachine;

public class StatePlayerAirborne : State
{
    private readonly PlayerContext m_PlayerContext;
    public StatePlayerAirborne(StateMachine stateMachine, State parent, PlayerContext playerContext) : base(stateMachine, parent)
    {
        m_PlayerContext = playerContext;
    }

    protected override void OnEnter()
    {
        //todo update animator
        base.OnEnter();
    }

    protected override State GetTransition()
    {
        if (m_PlayerContext.Grounded)
        {
            return ((StatePlayerRoot)Parent).Grounded;
        }

        return null;
    }
}
