using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CameraTriggerConfiguration m_Configuration;
    [SerializeField] private CinemachineCamera LeftCamera;
    [SerializeField] private CinemachineCamera RightCamera;
    
    public bool SwapCameras = false;
    public bool PanCamera = false;
    
    private ICameraManager m_CameraManager;
    private Bounds m_Bounds;
    private Vector2 m_OriginalOffset;
    private int m_PlayerLayer;
    
    public void Initialize(ICameraManager cameraManager)
    {
        m_CameraManager = cameraManager;
        m_PlayerLayer = LayerMask.NameToLayer("Player");
        m_OriginalOffset = Vector2.zero;
        m_Bounds = GetComponent<BoxCollider2D>().bounds;
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
                Vector2 playerPos = other.transform.position;
                Vector2 colliderCenter = m_Bounds.center;
                Vector2 enterDirection = (playerPos - colliderCenter).normalized;

                if (enterDirection.x < 0)
                {
                    m_CameraManager.SwapCameras(LeftCamera, RightCamera);
                }
                else
                {
                    m_CameraManager.SwapCameras(RightCamera, LeftCamera);
                }
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
                Vector2 playerPos = other.transform.position;
                Vector2 colliderCenter = m_Bounds.center;
                Vector2 exitDir = (playerPos - colliderCenter).normalized;

                if (exitDir.x > 0)
                {
                    m_CameraManager.SwapCameras(LeftCamera, RightCamera);
                }
                else
                {
                    m_CameraManager.SwapCameras(RightCamera, LeftCamera);
                }
            }

            m_OriginalOffset = Vector2.zero;
        }
    }
}
