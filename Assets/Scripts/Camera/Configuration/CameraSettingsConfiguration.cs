using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettingsConfiguration", menuName = "Configuration/Camera/Camera Settings Configuration", order = 0)]
public class CameraSettingsConfiguration : ScriptableObject
{
    [Header("Y Damping")]
    
    [Tooltip("Amount of Y Damping to when the player is falling")]
    public float FallingYDampingAmount = 0;
    
    [Tooltip("The Amount of time it takes for the full transition in Y Damping")]
    public float DampingChangeTime = 0.6f;
    
    [Tooltip("The time player is falling before the camera start changing Y Damping")]
    public float TimeUntillYDampingStart = 0.45f;

    // --------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------
}
