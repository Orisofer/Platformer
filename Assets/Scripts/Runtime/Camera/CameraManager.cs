using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, ICameraManager, ILogable
{
    private const int CAMERA_UPDATE_PRIORITY = -998;

    [SerializeField] private CameraSettingsConfiguration m_CameraConfiguration;
    [SerializeField] private CinemachineCamera[] m_Cameras;
    [SerializeField] private Transform m_CameraTriggersHolder;
    [SerializeField] private PlayerController m_Player;

    private CinemachineCamera m_ActiveCamera;
    private CinemachinePositionComposer m_PositionComposer;
    private CancellationTokenSource m_CtsChangeYDamping;
    private CancellationTokenSource m_CtsPanCamera;
    private Vector2 m_ActiveCamerasDeadZoneSize;
    private float m_OriginalYDamping;
    private float m_OriginalYOffset;
    private float m_YDampingSpeedThreshold;

    public CinemachineCamera ActiveCamera => m_ActiveCamera;
    public CinemachinePositionComposer PositionComposer => m_PositionComposer;
    public int UpdatePriority { get; set; }
    public bool EnableLogging { get; set; }
    
    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        InitializeSelfParams();
        InitializeActiveCamera();
        InitializeTriggers();
        RegisterEvents();
    }

    private void InitializeActiveCamera()
    {
        if (m_Cameras == null || m_Cameras.Length == 0)
        {
            Logger.LogError(this, "No cameras in your scene.");
            return;
        }
        
        for (int i = 0; i < m_Cameras.Length; i++)
        {
            if (m_Cameras[i].enabled)
            {
                m_ActiveCamera = m_Cameras[i];
            }
        }
        
        SetActiveCamera(m_ActiveCamera);
    }
    
    private void InitializeTriggers()
    {
        if (m_CameraTriggersHolder == null)
        {
            Logger.LogError(this, "No camera trigger holder attached.");
            return;
        }

        foreach (Transform child in m_CameraTriggersHolder)
        {
            CameraTrigger cameraTrigger = child.GetComponent<CameraTrigger>();
            cameraTrigger.Initialize(this);
        }
    }

    private void RegisterEvents()
    {
        m_Player.PlayerFalling += OnPlayerFalling;
        m_Player.PlayerGrounded += OnPlayerGrounded;
    }
    
    public void PanCameraOnContact(CameraPanRequest context)
    {
        m_CtsPanCamera?.Cancel();
        m_CtsPanCamera = new CancellationTokenSource();
        
        PanCameraAsync(context, m_CtsPanCamera.Token).Forget();
    }

    private void OnPlayerGrounded(PlayerContext playerContext)
    {
        m_CtsChangeYDamping?.Cancel();
        m_CtsChangeYDamping = new CancellationTokenSource();
        
        ChangeYDampingAsync(playerContext, m_CtsChangeYDamping.Token).Forget();
    }

    private void OnPlayerFalling(PlayerContext playerContext)
    {
        m_CtsChangeYDamping?.Cancel();
        m_CtsChangeYDamping = new CancellationTokenSource();
        
        ChangeYDampingAsync(playerContext, m_CtsChangeYDamping.Token).Forget();
    }

    private async UniTask ChangeYDampingAsync(PlayerContext playerContext, CancellationToken ct)
    {
        float dampingTime = m_CameraConfiguration.DampingChangeTime;
        float startDamping = m_PositionComposer.Damping.y;
        float startOffset = m_PositionComposer.TargetOffset.y;
        float endDamping = 0f;
        float endOffset = 0f;
        bool falling = playerContext.Falling;

        if (falling)
        {
            endDamping = m_CameraConfiguration.FallingYDampingAmount;
            endOffset = m_CameraConfiguration.FallingYOffsetAmount;
        }
        else
        {
            endDamping = m_OriginalYDamping;
            endOffset = m_OriginalYOffset;
        }

        float elapsed = 0f;

        while (elapsed < dampingTime)
        {
            ct.ThrowIfCancellationRequested();
            
            if (!falling || (playerContext.FrameVelocity.y <= -m_YDampingSpeedThreshold))
            {
                float dampingLerpVal = Mathf.Lerp(startDamping, endDamping, elapsed / dampingTime);
                float offsetLerpVal = Mathf.Lerp(startOffset, endOffset, elapsed / dampingTime);
                
                m_PositionComposer.Damping.y = dampingLerpVal;
                m_PositionComposer.TargetOffset.y = offsetLerpVal;
                
                elapsed += Time.deltaTime;
            }
            
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, ct);
        }
    }

    private async UniTask PanCameraAsync(CameraPanRequest context, CancellationToken ct)
    {
        Vector2 panDirection = context.GetPanDirection();
        Vector2 startPosition = m_PositionComposer.TargetOffset;
        Vector2 target = context.PanDistance * panDirection;

        if (context.ResetToInitialPosition)
        {
            m_PositionComposer.Composition.DeadZone.Size = m_ActiveCamerasDeadZoneSize;
            target = context.InitialPosition;
        }
        else
        {
            m_PositionComposer.Composition.DeadZone.Size = Vector2.zero;
        }

        float totalTime = context.PanTime;
        float elapsed = 0f;
        
        while (elapsed < totalTime)
        {
            ct.ThrowIfCancellationRequested();
            
            elapsed += Time.deltaTime;
            
            Vector3 offsetLerpVal = Vector3.Lerp(startPosition, target, elapsed / totalTime);
            
            m_PositionComposer.TargetOffset = offsetLerpVal;
            
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, ct);
        }
    }

    private void SetActiveCamera(CinemachineCamera camera)
    {
        m_ActiveCamera = camera;
        m_PositionComposer = m_ActiveCamera.GetComponent<CinemachinePositionComposer>();
        m_OriginalYDamping = m_PositionComposer.Damping.y;
        m_OriginalYOffset = m_PositionComposer.TargetOffset.y;
        m_ActiveCamerasDeadZoneSize = m_PositionComposer.Composition.DeadZone.Size;
    }

    private void InitializeSelfParams()
    {
        m_CtsChangeYDamping = new CancellationTokenSource();
        UpdatePriority = CAMERA_UPDATE_PRIORITY;
        m_YDampingSpeedThreshold = m_Player.PlayerConfiguration.MaxFallSpeed;
    }
    
    private void UnregisterEvents()
    {
        m_Player.PlayerFalling -= OnPlayerFalling;
        m_Player.PlayerGrounded -= OnPlayerGrounded;
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }
}
