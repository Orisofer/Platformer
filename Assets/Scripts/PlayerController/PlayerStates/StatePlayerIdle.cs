using UnityEngine;
using Core.StateMachine;

public class StatePlayerIdle : State
{
    private PlayerContext m_PlayerContext;
    
    public StatePlayerIdle(StateMachine stateMachine, State parent, PlayerContext playerContext) : base(stateMachine, parent)
    {
        m_PlayerContext = playerContext;
    }

    protected override State GetTransition()
    {
        if (Mathf.Abs(m_PlayerContext.MoveHorizontal.x) > 0.01f)
        {
            return ((StatePlayerGrounded)Parent).Move;
        }

        return null;
    }

    protected override void OnEnter()
    {
        m_PlayerContext.Velocity = Vector2.zero;
    }
}
