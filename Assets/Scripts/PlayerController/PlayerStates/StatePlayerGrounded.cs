using UnityEngine;
using Core.StateMachine;

public class StatePlayerGrounded : State
{
    public readonly StatePlayerIdle Idle;
    public readonly StatePlayerMove Move;
    
    private readonly PlayerContext m_PlayerContext;
    
    public StatePlayerGrounded(StateMachine stateMachine, State parent, PlayerContext playerContext) : base(stateMachine, parent)
    {
        m_PlayerContext = playerContext;
        
        Idle = new StatePlayerIdle(stateMachine, this, m_PlayerContext);
        Move = new StatePlayerMove(stateMachine, this, m_PlayerContext);
    }

    protected override State GetInitializeState()
    {
        return Idle;
    }

    protected override State GetTransition()
    {
        if (m_PlayerContext.JumpPressed)
        {
            m_PlayerContext.JumpPressed = false;
            
            Rigidbody2D rigidBody = m_PlayerContext.Rigidbody2D;

            if (rigidBody)
            {
                var velocity = rigidBody.linearVelocity;
                velocity.y = m_PlayerContext.JumpForce;
                rigidBody.linearVelocity = velocity;
            }

            return ((StatePlayerRoot)Parent).Airborne;
        }

        return m_PlayerContext.Grounded ? null : ((StatePlayerRoot)Parent).Airborne;
    }
}
