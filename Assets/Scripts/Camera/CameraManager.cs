using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, ILogable
{
    private const int CAMERA_UPDATE_PRIORITY = -998;

    [SerializeField] private CameraSettingsConfiguration m_CameraConfiguration;
    [SerializeField] private CinemachineCamera[] m_Cameras;
    [SerializeField] private PlayerController m_Player;

    private CinemachineCamera m_ActiveCamera;
    private CinemachinePositionComposer m_PositionComposer;
    private float m_OriginalYDamping;
    private float m_YDampingSpeedThreshold;
    
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

        m_PositionComposer = m_ActiveCamera.GetComponent<CinemachinePositionComposer>();

        m_OriginalYDamping = m_PositionComposer.Damping.y;
    }

    private void RegisterEvents()
    {
        m_Player.PlayerJumped += OnPlayerJumped;
        m_Player.PlayerFalling += OnPlayerFalling;
        m_Player.PlayerGrounded += OnPlayerGrounded;
    }

    private void OnPlayerGrounded(PlayerContext playerContext)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeYDamping(playerContext));
    }

    private void OnPlayerFalling(PlayerContext playerContext)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeYDamping(playerContext));
    }

    private void OnPlayerJumped(PlayerContext playerContext)
    {
    }

    private IEnumerator ChangeYDamping(PlayerContext playerContext)
    {
        float dampingTime = m_CameraConfiguration.DampingChangeTime;
        float startDamping = m_PositionComposer.Damping.y;
        float endDamping = 0f;

        if (playerContext.Falling)
        {
            endDamping = 0f;
        }
        else
        {
            endDamping = m_OriginalYDamping;
        }

        float elapsed = 0f;

        while (elapsed < dampingTime)
        {
            if (playerContext.FrameVelocity.y <= m_YDampingSpeedThreshold)
            {
                float lerpVal = Mathf.Lerp(startDamping, endDamping, elapsed / dampingTime);
                elapsed += Time.deltaTime;
            
                m_PositionComposer.Damping.y = lerpVal;
            }
            
            yield return null;
        }
    }

    private void InitializeSelfParams()
    {
        UpdatePriority = CAMERA_UPDATE_PRIORITY;
        m_YDampingSpeedThreshold = m_Player.PlayerConfiguration.MaxFallSpeed;
    }
    
    private void UnregisterEvents()
    {
        m_Player.PlayerJumped -= OnPlayerJumped;
        m_Player.PlayerFalling -= OnPlayerFalling;
        m_Player.PlayerGrounded -= OnPlayerGrounded;
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }
}
