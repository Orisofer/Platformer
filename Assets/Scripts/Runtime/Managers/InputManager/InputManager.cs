using OriGame.Core;
using UnityEngine;


public class InputManager : MonoBehaviour, IUpdate
{
    private const int INPUT_UPDATE_PRIORITY = -1000;
    
    [SerializeField] private InputReader m_InputReader;
        
    private UpdateManager m_UpdateManager;
    private IGameLogger m_GameLogger;
    private FrameInput m_FrameInput;
    
    public FrameInput FrameInput => m_FrameInput;

    public int UpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }

    private double m_Time;
    
    public void Initialize(IServiceLocator serviceLocator)
    {
        if (!InstallServices(serviceLocator))
        {
            return;
        }
            
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
            
        m_InputReader.MoveStarted += OnMoveStarted;
        m_InputReader.MoveEnded += OnMoveEnded;
        m_InputReader.JumpPressed += OnJumpPressed;
        m_InputReader.JumpReleased += OnJumpReleased;
            
        m_InputReader.EnablePlayerActions();
        
        EnableUpdate = true;
    }

    private bool InstallServices(IServiceLocator serviceLocator)
    {
        m_GameLogger = serviceLocator.GetService<IGameLogger>();

        if (m_GameLogger == null)
        {
            return false;
        }
            
        m_UpdateManager = serviceLocator.GetService<UpdateManager>();

        if (m_UpdateManager == null)
        {
            m_GameLogger.LogError($"[ Input Manager ] {nameof(UpdateManager)} could not be fetched from service locator");
            return false;
        }

        return true;
    }

    public void OnUpdate(float deltaTime)
    {
        m_FrameInput = new FrameInput();

        InterpretFrameTime(ref m_FrameInput, deltaTime);
        InterpretMove(ref m_FrameInput);
        InterpretJump(ref m_FrameInput);
    }

    private void InterpretFrameTime(ref FrameInput frameInput, float deltaTime)
    {
        m_Time += deltaTime;
        
        frameInput.Time = m_Time;
    }

    private void InterpretJump(ref FrameInput frameInput)
    {
        bool jumpHeld = m_InputReader.IsJumpHeld;
        
        frameInput.JumpHeld = jumpHeld;
        
        m_GameLogger.Log("Jump Pressed");
    }

    private void InterpretMove(ref FrameInput frameInput)
    {
        Vector2 dir = m_InputReader.Direction;
        
        frameInput.Direction = dir;
        
        m_GameLogger.Log(dir.ToString());
    }

    private void OnMoveStarted()
    {
        m_GameLogger.Log("OnMoveStarted");
    }
        
    private void OnMoveEnded()
    {
        m_FrameInput.MoveFinished = true;
        
        m_GameLogger.Log("OnMoveEnded");
    }

    private void OnJumpPressed()
    {
        m_FrameInput.JumpPressed = true;
        
        m_GameLogger.Log("Jump Pressed");
    }

    private void OnJumpReleased()
    {
        m_FrameInput.JumpReleased = true;
        
        m_GameLogger.Log("Jump Released");
    }
    
    private void OnDestroy()
    {
        EnableUpdate = false;
        
        m_UpdateManager.RemoveFromUpdate(this);
            
        m_InputReader.MoveStarted -= OnMoveStarted;
        m_InputReader.MoveEnded -= OnMoveEnded;
        m_InputReader.JumpPressed -= OnJumpPressed;
        m_InputReader.JumpReleased -= OnJumpReleased;
    }
}