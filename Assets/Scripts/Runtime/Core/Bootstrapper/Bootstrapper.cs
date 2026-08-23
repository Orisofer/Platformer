using Cysharp.Threading.Tasks.Triggers;
using OriGame.Managers;
using UnityEngine;

namespace OriGame.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private UpdateManager m_UpdateManager;
        [SerializeField] private InputManager m_InputManager;
        [SerializeField] private CameraManager m_CameraManager;
        [SerializeField] private PlayerController m_PlayerController;
        
        private IServiceLocator m_ServiceLocator;
        private IGameLogger m_GameLogger;

        private void Awake()
        {
            InitializeCore();
            RegisterServices();
            InitializeServices();
        }

        private void InitializeCore()
        {
            m_GameLogger = new UnityGameLogger();
            m_ServiceLocator = new ServiceLocator(m_GameLogger);
            
            m_ServiceLocator.Register(m_GameLogger);
        }

        private void RegisterServices()
        {
            m_ServiceLocator.Register(m_UpdateManager);
            m_ServiceLocator.Register(m_InputManager);
            m_ServiceLocator.Register(m_CameraManager);
            m_ServiceLocator.Register(m_PlayerController);
        }
        
        private void InitializeServices()
        {
            m_UpdateManager.Initialize(m_ServiceLocator);
            m_InputManager.Initialize(m_ServiceLocator);
            m_CameraManager.Initialize(m_ServiceLocator);
            m_PlayerController.Initialize(m_ServiceLocator);
        }
    }
}

