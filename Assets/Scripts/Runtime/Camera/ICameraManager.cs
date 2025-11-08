using Unity.Cinemachine;
using UnityEngine;

public interface ICameraManager
{
    public CinemachineCamera ActiveCamera { get; }
    public CinemachinePositionComposer PositionComposer { get; }
    public void PanCameraOnContact(CameraPanRequest context);
}
