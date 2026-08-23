using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraTriggerConfiguration", menuName = "Configuration/Camera/Camera Trigger Configuration", order = 0)]
public class CameraTriggerConfiguration : ScriptableObject
{
    public CameraPanDirection PanDirection;
    public float PanDistance = 3f;
    public float PanSpeed = 0.35f;
    public float PanTime = 0.5f;
}
