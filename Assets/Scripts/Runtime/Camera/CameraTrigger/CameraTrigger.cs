using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CameraTriggerConfiguration m_Configuration;
    [SerializeField] private CinemachineCamera OnEnterCamera;
    [SerializeField] private CinemachineCamera OnExitCamera;
    
    public bool SwapCameras = false;
    public bool PanCamera = false;
    
    private Collider2D m_Collider;
    private ICameraManager m_CameraManager;
    private Vector2 m_OriginalOffset;
    private int m_PlayerLayer;
    
    public void Initialize(ICameraManager cameraManager)
    {
        m_CameraManager = cameraManager;
        m_Collider = GetComponent<Collider2D>();
        m_PlayerLayer = LayerMask.NameToLayer("Player");
        m_OriginalOffset = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == m_PlayerLayer)
        {
            m_OriginalOffset = m_CameraManager.PositionComposer.TargetOffset;
            
            if (PanCamera)
            {
                CameraPanRequest request = new CameraPanRequest()
                {
                    PanDirection = m_Configuration.PanDirection,
                    PanDistance = m_Configuration.PanDistance,
                    PanTime = m_Configuration.PanTime,
                    PanSpeed = m_Configuration.PanSpeed,
                };
                
                m_CameraManager.PanCameraOnContact(request);
            }

            if (SwapCameras)
            {
                // do swap
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == m_PlayerLayer)
        {
            if (PanCamera)
            {
                CameraPanRequest request = new CameraPanRequest()
                {
                    ResetToInitialPosition = true,
                    InitialPosition = m_OriginalOffset,
                    PanTime = m_Configuration.PanTime,
                    PanSpeed = m_Configuration.PanSpeed,
                };
                
                m_CameraManager.PanCameraOnContact(request);
            }
            
            if (SwapCameras)
            {
                // do swap
            }

            m_OriginalOffset = Vector2.zero;
        }
    }
}
