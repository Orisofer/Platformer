using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using OriGame.Core;
using OriGame.Player;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, ICameraManager
{
    private const int CAMERA_UPDATE_PRIORITY = -998;

    [SerializeField] private CameraSettingsConfiguration m_CameraConfiguration;
    [SerializeField] private Transform m_CamerasHolder;
    [SerializeField] private Transform m_CameraTriggersHolder;

    private Dictionary<CinemachineCamera, GameCamera> m_Cameras;
    private CancellationTokenSource m_CtsChangeYDamping;
    private CancellationTokenSource m_CtsPanCamera;
    private GameCamera m_ActiveCamera;
    private PlayerController m_Player;
    private IGameLogger m_Logger;
    private float m_YDampingSpeedThreshold;

    public CinemachineCamera ActiveCamera => m_ActiveCamera.Camera;
    public CinemachinePositionComposer PositionComposer => m_ActiveCamera.PositionComposer;
    public int UpdatePriority { get; set; }
    public bool EnableLogging { get; set; }

    public void Initialize(IServiceLocator serviceLocator)
    {
        if (!InitializeServices(serviceLocator))
        {
            return;
        }
        
        InitializeSelfParams();
        InitializeActiveCamera();
        InitializeTriggers();
        RegisterEvents();
    }

    private bool InitializeServices(IServiceLocator serviceLocator)
    {
        m_Logger = serviceLocator.GetService<IGameLogger>();
        m_Player = serviceLocator.GetService<PlayerController>();

        if (m_Player == null)
        {
            m_Logger.LogWarning($"[ Camera Manager ] Player Controller could not be fetched from service locator");
            return false;
        }
        return true;
    }

    private void InitializeTriggers()
    {
        if (m_CameraTriggersHolder == null)
        {
            m_Logger.LogError("[ Camera Manager ] No camera trigger holder attached.");
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

    public void SwapCameras(CinemachineCamera from, CinemachineCamera to)
    {
        if (m_ActiveCamera.Camera != from)
        {
            m_Logger.LogError($"[ Camera Manager ] Cannot swap camera from {from} to {to}.");
        }

        if (m_ActiveCamera.Camera != to)
        {
            if (m_Cameras.TryGetValue(to, out GameCamera toCamera))
            {
                SetActiveCamera(toCamera);
            }

            if (m_Cameras.TryGetValue(from, out GameCamera fromCamera))
            {
                fromCamera.SetActive(false);
            }
        }
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
        float startDamping = m_ActiveCamera.PositionComposer.Damping.y;
        float startOffset = m_ActiveCamera.PositionComposer.TargetOffset.y;
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
            endDamping = m_ActiveCamera.OriginalDamping.y;
            endOffset = m_ActiveCamera.OriginalOffset.y;
        }

        float elapsed = 0f;

        while (elapsed < dampingTime)
        {
            ct.ThrowIfCancellationRequested();
            
            if (!falling || (playerContext.PredictedVelocity.y <= -m_YDampingSpeedThreshold))
            {
                float dampingLerpVal = Mathf.Lerp(startDamping, endDamping, elapsed / dampingTime);
                float offsetLerpVal = Mathf.Lerp(startOffset, endOffset, elapsed / dampingTime);
                
                m_ActiveCamera.PositionComposer.Damping.y = dampingLerpVal;
                m_ActiveCamera.PositionComposer.TargetOffset.y = offsetLerpVal;
                
                elapsed += Time.deltaTime;
            }
            
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, ct);
        }

        if (!falling)
        {
            m_ActiveCamera.PositionComposer.Damping.y = endDamping;
            m_ActiveCamera.PositionComposer.TargetOffset.y = endOffset;
        }
    }

    private async UniTask PanCameraAsync(CameraPanRequest context, CancellationToken ct)
    {
        Vector2 panDirection = context.GetPanDirection();
        Vector2 startPosition = m_ActiveCamera.PositionComposer.TargetOffset;
        Vector2 target = context.PanDistance * panDirection;

        if (context.ResetToInitialPosition)
        {
            target = context.InitialPosition;
        }

        float totalTime = context.PanTime;
        float elapsed = 0f;
        
        while (elapsed < totalTime)
        {
            ct.ThrowIfCancellationRequested();
            
            elapsed += Time.deltaTime;
            
            Vector3 offsetLerpVal = Vector3.Lerp(startPosition, target, elapsed / totalTime);
            
            m_ActiveCamera.PositionComposer.TargetOffset = offsetLerpVal;
            
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, ct);
        }
    }

    private void SetActiveCamera(GameCamera camera)
    {
        m_ActiveCamera = camera;
        m_ActiveCamera.PositionComposer.TargetOffset = camera.OriginalOffset;
        
        camera.SetActive(true);
    }

    private void InitializeSelfParams()
    {
        m_Cameras = new Dictionary<CinemachineCamera, GameCamera>();
        
        m_CtsChangeYDamping = new CancellationTokenSource();
        m_CtsPanCamera = new CancellationTokenSource();
        
        UpdatePriority = CAMERA_UPDATE_PRIORITY;
        
        m_YDampingSpeedThreshold = m_Player.PlayerConfiguration.MaxFallSpeed;
    }
    
    private void InitializeActiveCamera()
    {
        if (m_Cameras == null)
        {
            m_Cameras = new Dictionary<CinemachineCamera, GameCamera>();
        }

        foreach (Transform cameraTransform in m_CamerasHolder)
        {
            CinemachineCamera camera = cameraTransform.GetComponent<CinemachineCamera>();

            if (camera == null)
            {
                m_Logger.LogError($"[ Camera Manager ] Camera not found on game object: {cameraTransform}.");
                continue;
            }

            GameCamera gameCamera = new GameCamera(camera);
            
            m_Cameras.Add(camera, gameCamera);

            if (gameCamera.IsActive)
            {
                SetActiveCamera(gameCamera);
            }
        }
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
