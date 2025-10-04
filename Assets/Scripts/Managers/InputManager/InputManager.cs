using UnityEngine;

public class InputManager : MonoBehaviour, ILogable, IUpdate
{
    private const int INPUT_UPDATE_PRIORITY = -1000;
    
    [SerializeField] private UpdateManager m_UpdateManager;
    [SerializeField] private InputReader m_InputReader;
        
    [field: SerializeField] public bool EnableLogging { get; set; }
    
    private FrameInput m_FrameInput;
    
    public FrameInput FrameInput => m_FrameInput;

    public int UpdatePriority { get; set; }
    public bool EnableUpdate { get; set; }

    private float m_Time;
    
    private void Start()
    {
        UpdatePriority = INPUT_UPDATE_PRIORITY;
        
        m_UpdateManager.AddToUpdate(this);
            
        m_InputReader.MoveStarted += OnMoveStarted;
        m_InputReader.MoveEnded += OnMoveEnded;
        m_InputReader.JumpPressed += OnJumpPressed;
        m_InputReader.JumpReleased += OnJumpReleased;
            
        m_InputReader.EnablePlayerActions();
        
        EnableUpdate = true;
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
        
        Logger.Log(this, "Jump Pressed");
    }

    private void InterpretMove(ref FrameInput frameInput)
    {
        Vector2 dir = m_InputReader.Direction;
        
        frameInput.Direction = dir;
        
        Logger.Log(this, dir.ToString());
    }

    private void OnMoveStarted()
    {
        Logger.Log(this, "OnMoveStarted");
    }
        
    private void OnMoveEnded()
    {
        m_FrameInput.MoveFinished = true;
        
        Logger.Log(this, "OnMoveEnded");
    }

    private void OnJumpPressed()
    {
        m_FrameInput.JumpPressed = true;
        
        Logger.Log(this, "Jump Pressed");
    }

    private void OnJumpReleased()
    {
        m_FrameInput.JumpReleased = true;
        
        Logger.Log(this, "Jump Released");
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