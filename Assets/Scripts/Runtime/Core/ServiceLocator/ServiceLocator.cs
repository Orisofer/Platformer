using System;
using System.Collections.Generic;

namespace OriGame.Core
{
    public class ServiceLocator : IServiceLocator
    {
        private Dictionary<Type, object> m_Services;
        private IGameLogger m_GameLogger;

        public ServiceLocator(IGameLogger gameLogger)
        {
            m_Services =  new Dictionary<Type, object>();
            m_GameLogger  = gameLogger;
        }
    
        public bool Register<T>(T service) where T : class
        {
            Type serviceType = typeof(T);

            if (!m_Services.TryAdd(serviceType, service))
            {
                m_GameLogger.LogError($"[ Service Locator ] Service is already registered: {nameof(T)}");
                return false;
            }

            return true;
        }

        public bool Unregister<T>(T service) where T : class
        {
            Type serviceType = typeof(T);
            
            if (!m_Services.Remove(serviceType))
            {
                m_GameLogger.LogError($"[ Service Locator ] Removing Non-Existing Service: {nameof(T)}");
                return false;
            }
            
            return true;
        }

        public T GetService<T>() where T : class
        {
            Type serviceType = typeof(T);

            if (m_Services.TryGetValue(serviceType, out object serviceObject))
            {
                return (T)serviceObject;
            }

            return null;
        }
    }
}