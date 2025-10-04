using System.Collections.Generic;
using UnityEngine;

namespace Core.StateMachine
{
    public abstract class State : IState
    {
        public readonly StateMachine StateMachine;
        public readonly State Parent;

        public State ActiveChild;

        public State(StateMachine stateMachine, State parent = null)
        {
            StateMachine = stateMachine;
            Parent = parent;
        }

        protected virtual State GetInitializeState() => null;
        protected virtual State GetTransition() => null;

        internal void Enter()
        {
            if (Parent != null)
            {
                Parent.ActiveChild = this;
            }
            
            OnEnter();
            
            State init = GetInitializeState();

            if (init != null)
            {
                init.Enter();
            }
        }

        internal void Exit()
        {
            if (ActiveChild != null)
            {
                ActiveChild.Exit();
            }
            
            ActiveChild = null;
            
            OnExit();
        }

        internal void Update(float deltaTime)
        {
            State nextState = GetTransition();

            if (nextState != null)
            {
                StateMachine.Sequencer.RequestTransition(this, nextState);
                return;
            }

            if (ActiveChild != null)
            {
                ActiveChild.Update(deltaTime);
            }
            
            OnUpdate(deltaTime);
        }

        public State Leaf()
        {
            State current = this;

            while (current.ActiveChild != null)
            {
                current = current.ActiveChild;
            }
            
            return current;
        }

        public IEnumerable<State> PathToRoot()
        {
            for (State state = this; state.Parent != null; state = state.Parent)
            {
                yield return state;
            }
        }

        protected virtual void OnEnter()
        {
            
        }

        protected virtual void OnExit()
        {
            
        }

        protected virtual void OnUpdate(float deltaTime)
        {
            
        }
    }
}

