using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public class GameCamera
{
    private CinemachineCamera m_Camera;
    private CinemachinePositionComposer m_PositionComposer;
    private Vector2 m_OriginalDamping;
    private Vector2 m_OriginalOffset;
    private string m_Name;
    
    public CinemachineCamera Camera => m_Camera;
    public CinemachinePositionComposer PositionComposer => m_PositionComposer;
    public Vector2 OriginalDamping => m_OriginalDamping;
    public Vector2 OriginalOffset => m_OriginalOffset;
    public bool IsActive => m_Camera.enabled;

    public GameCamera(CinemachineCamera camera)
    {
        m_Camera = camera;
        m_PositionComposer = m_Camera.GetComponent<CinemachinePositionComposer>();
        m_OriginalDamping = m_PositionComposer.Damping;
        m_OriginalOffset = m_PositionComposer.TargetOffset;
        m_Name = m_Camera.gameObject.name;
    }

    public void SetActive(bool active)
    {
        m_Camera.enabled = active;
    }
}
