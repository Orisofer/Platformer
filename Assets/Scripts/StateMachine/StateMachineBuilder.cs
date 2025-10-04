using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.StateMachine
{
    public class StateMachineBuilder : ILogable
    {
        private const
            BindingFlags BINDING_FLAGS =
            BindingFlags.FlattenHierarchy |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;
        
        private const string STATE_MACHINE_FIELD_NAME = "StateMachine";
        
        private readonly GameObject m_Owner;
        
        public readonly State Root;
        public Transform transform => m_Owner.transform;
        public bool EnableLogging { get; set; }
        

        public StateMachineBuilder(GameObject owner, State root)
        {
            m_Owner = owner;
            Root = root;
            EnableLogging = true;
        }

        public StateMachine Build()
        {
            StateMachine stateMachine = new StateMachine(Root);
            
            Wire(Root, stateMachine, new HashSet<State>());
            
            return stateMachine;
        }

        public void Wire(State state, StateMachine stateMachine, HashSet<State> visited)
        {
            if (state == null) return;
            if (visited.Contains(state)) return;
            
            var stateMachineField = typeof(State).GetField(STATE_MACHINE_FIELD_NAME, BINDING_FLAGS);

            if (stateMachineField == null)
            {
                Logger.LogError(this, $"{m_Owner.name} -> Couldn't find state machine field by string name in builder");
                return;
            }
            
            stateMachineField.SetValue(state, stateMachine);

            visited.Add(state);

            foreach (var field in state.GetType().GetFields(BINDING_FLAGS))
            {
                if (!typeof(State).IsAssignableFrom(field.FieldType)) continue;
                if (field.Name == "Parent") continue;
                
                State child = field.GetValue(state) as State;
                
                if (child == null) continue;
                
                Wire(child, stateMachine, visited);
            }
        }
    }
}


