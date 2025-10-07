using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetection : ILogable
{
    private Dictionary<Type, ICollisionDetectionStrategy> m_Checks;
    
    private Transform m_PlayerTransform;
    
    private bool m_EnableCollisionDetection;
    public bool EnableLogging { get; set; }
    public Transform transform => m_PlayerTransform;

    public PlayerCollisionDetection(PlayerController playerController, bool enableLogging)
    {
        m_Checks = new Dictionary<Type, ICollisionDetectionStrategy>();
        m_EnableCollisionDetection = true;
        EnableLogging = enableLogging;
        m_PlayerTransform = playerController.transform;
    }

    public CollisionDetectionResult Perform<T>() where T : ICollisionDetectionStrategy
    {
        if (m_Checks.TryGetValue(typeof(T), out ICollisionDetectionStrategy check))
        {
            return check.Calculate();
        }
        
        Logger.LogError(this,
            $"Trying to perform a collision check for type {typeof(T).Name}. No such check is allocated inside dictionary");

        return default;
    }

    public void AddCheck(ICollisionDetectionStrategy strategy)
    {
        m_Checks.Add(strategy.GetType(), strategy);
    }

    public void RemoveCheck(ICollisionDetectionStrategy strategy)
    {
        m_Checks.Remove(strategy.GetType());
    }

    public void EnableCollisionDetection(bool enable)
    {
        m_EnableCollisionDetection = enable;
    }
}
