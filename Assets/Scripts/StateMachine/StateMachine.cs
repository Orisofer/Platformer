using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.StateMachine
{
    public class StateMachine
    {
        public readonly State Root;
        public readonly TransitionSequencer Sequencer;

        private string m_CurrentPath;
        private bool m_Started;
        private bool m_EnableDebugging;
        
        public string CurrentPath => m_CurrentPath;

        public StateMachine(State root)
        {
            Root = root;
            Sequencer = new TransitionSequencer(this);
        }

        public void Start()
        {
            if (m_Started) return;
            
            m_Started = true;
            m_EnableDebugging = false;
            
            Root.Enter();
        }

        public void Tick(float deltatime)
        {
            if (!m_Started)
            {
                Start();
            }
            
            TickInternal(deltatime);
        }

        internal void TickInternal(float deltatime)
        {
            Root.Update(deltatime);

            if (m_EnableDebugging)
            {
                DebugStatePath();
            }
        }

        private void DebugStatePath()
        {
            m_CurrentPath = StatePath(Root.Leaf());
        }

        public void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null) return;
            
            State lowestCommonAncestor = Sequencer.LowestCommonAncestor(from, to);

            for (State current = from; current != lowestCommonAncestor; current = current.Parent)
            {
                current.Exit();
            }
            
            Stack<State> enterSequence = new Stack<State>();
            
            for (State current = to; current != lowestCommonAncestor; current = current.Parent)
            {
                enterSequence.Push(current);
            }

            while (enterSequence.Count > 0)
            {
                State current = enterSequence.Pop();
                current.Enter();
            }
        }

        public string StatePath(State state)
        {
            string result = string.Join(" -> ", state.PathToRoot().Reverse().Select(stateNode => stateNode.GetType().Name));
            return result;
        }

        public void SetDebugging(bool enabled)
        {
            m_EnableDebugging = enabled;
        }
    }
}

