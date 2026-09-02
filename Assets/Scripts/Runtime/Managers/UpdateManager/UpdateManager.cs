using System.Collections.Generic;
using OriGame.Core;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    private List<IUpdate> m_UpdateActors = new List<IUpdate>();
    private HashSet<IUpdate> m_UpdateAddPendingActors = new HashSet<IUpdate>();
    private HashSet<IUpdate> m_UpdateRemovePendingActors = new HashSet<IUpdate>();

    private List<IFixedUpdate> m_FixedUpdateActors = new List<IFixedUpdate>();
    private HashSet<IFixedUpdate> m_FixedUpdateAddPendingActors = new HashSet<IFixedUpdate>();
    private HashSet<IFixedUpdate> m_FixedUpdateRemovePendingActors = new HashSet<IFixedUpdate>();
    
    private List<ILateUpdate> m_LateUpdateActors = new List<ILateUpdate>();
    private HashSet<ILateUpdate> m_LateUpdateAddPendingActors = new HashSet<ILateUpdate>();
    private HashSet<ILateUpdate> m_LateUpdateRemovePendingActors = new HashSet<ILateUpdate>();
    
    private void Update()
    {
        // tick all actors
        for (int i = m_UpdateActors.Count - 1; i >= 0; --i)
        {
            if (m_UpdateActors[i].EnableUpdate)
            {
                m_UpdateActors[i].OnUpdate(Time.deltaTime);
            }
        }

        // remove unregistered actors
        if (m_UpdateRemovePendingActors.Count > 0)
        {
            foreach (IUpdate actor in m_UpdateRemovePendingActors)
            {
                m_UpdateActors.Remove(actor);
            }
        }

        // add newly registered actors
        if (m_UpdateAddPendingActors.Count > 0)
        {
            foreach (IUpdate actor in m_UpdateAddPendingActors)
            {
                m_UpdateActors.Add(actor);
            }
            
            // smaller priority values are executed earlier
            m_UpdateActors.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
        }

        // clear queues states
        m_UpdateRemovePendingActors.Clear();
        m_UpdateAddPendingActors.Clear();
    }

    private void FixedUpdate()
    {
        // tick all actors
        for (int i = m_FixedUpdateActors.Count - 1; i >= 0; --i)
        {
            if (m_FixedUpdateActors[i].EnableFixedUpdate)
            {
                m_FixedUpdateActors[i].OnFixedUpdate(Time.fixedDeltaTime);
            }
        }
        
        // remove unregistered actors
        if (m_FixedUpdateRemovePendingActors.Count > 0)
        {
            foreach (IFixedUpdate actor in m_FixedUpdateRemovePendingActors)
            {
                m_FixedUpdateActors.Remove(actor);
            }
        }
        
        // add newly registered actors
        if (m_FixedUpdateAddPendingActors.Count > 0)
        {
            foreach (IFixedUpdate actor in m_FixedUpdateAddPendingActors)
            {
                m_FixedUpdateActors.Add(actor);
            }
            
            // smaller priority values are executed earlier
            m_FixedUpdateActors.Sort((a, b) => a.FixedUpdatePriority.CompareTo(b.FixedUpdatePriority));
        }

        // clear queues states
        m_FixedUpdateRemovePendingActors.Clear();
        m_FixedUpdateAddPendingActors.Clear();
    }

    private void LateUpdate()
    {
        // tick all actors
        for (int i = m_LateUpdateActors.Count - 1; i >= 0; --i)
        {
            if (m_LateUpdateActors[i].EnableLateUpdate)
            {
                m_LateUpdateActors[i].OnLateUpdate();
            }
        }
        
        // remove unregistered actors
        if (m_LateUpdateRemovePendingActors.Count > 0)
        {
            foreach (ILateUpdate actor in m_LateUpdateRemovePendingActors)
            {
                m_LateUpdateActors.Remove(actor);
            }
        }
        
        // add newly registered actors
        if (m_LateUpdateAddPendingActors.Count > 0)
        {
            foreach (ILateUpdate actor in m_LateUpdateAddPendingActors)
            {
                m_LateUpdateActors.Add(actor);
            }
            
            // smaller priority values are executed earlier
            m_LateUpdateActors.Sort((a, b) => a.LateUpdatePriority.CompareTo(b.LateUpdatePriority));
        }

        // clear queues states
        m_LateUpdateRemovePendingActors.Clear();
        m_LateUpdateAddPendingActors.Clear();
    }

    public void AddToUpdate(IUpdate update)
    {
        m_UpdateAddPendingActors.Add(update);
    }

    public void AddToFixedUpdate(IFixedUpdate update)
    {
        m_FixedUpdateAddPendingActors.Add(update);
    }

    public void AddToLateUpdate(ILateUpdate update)
    {
        m_LateUpdateAddPendingActors.Add(update);
    }

    public void RemoveFromUpdate(IUpdate update)
    {
        m_UpdateRemovePendingActors.Add(update);
    }

    public void RemoveFromFixedUpdate(IFixedUpdate update)
    {
        m_FixedUpdateRemovePendingActors.Add(update);
    }

    public void RemoveFromLateUpdate(ILateUpdate update)
    {
        m_LateUpdateRemovePendingActors.Add(update);
    }
    
    public void Initialize(IServiceLocator serviceLocator)
    {
        // no - op //
    }
}