using UnityEngine;
using Core.StateMachine;

public class StatePlayerMove : State
{
    private PlayerContext m_PlayerContext;
    
    public StatePlayerMove(StateMachine stateMachine, State parent, PlayerContext playerContext) : base(stateMachine, parent)
    {
        m_PlayerContext = playerContext;
    }

    protected override State GetTransition()
    {
        if (!m_PlayerContext.Grounded)
        {
            return ((StatePlayerRoot)Parent).Airborne;
        }
        if (Mathf.Abs(m_PlayerContext.MoveHorizontal.x) < 0.1f)
        {
            return ((StatePlayerGrounded)Parent).Idle;
        }

        return null;
    }

    protected override void OnUpdate(float deltaTime)
    {
        var target = m_PlayerContext.MoveHorizontal.x * m_PlayerContext.HorizontalSpeed;
        m_PlayerContext.Velocity.x = target * target;
    }
}
