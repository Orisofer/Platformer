using System.Collections.Generic;
using UnityEngine;

namespace Core.StateMachine
{
    public class TransitionSequencer
    {
        public readonly StateMachine StateMachine;

        public TransitionSequencer(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public void RequestTransition(State from, State to)
        {
            StateMachine.ChangeState(from, to);
        }

        public State LowestCommonAncestor(State stateA, State stateB)
        {
            HashSet<State> parentsOfA = new HashSet<State>();
            
            for (State current = stateA; current != null; current = current.Parent)
            {
                parentsOfA.Add(current);
            }
            
            for (State current = stateB; current != null; current = current.Parent)
            {
                if (parentsOfA.Contains(current))
                {
                    return current;
                }
            }

            return null;
        }
    }
}


