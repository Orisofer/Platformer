using UnityEngine;

public struct CameraPanRequest
{
    public CameraPanDirection PanDirection;
    public Vector2 InitialPosition;
    public float PanDistance;
    public float PanTime;
    public float PanSpeed;
    public bool ResetToInitialPosition;
    
    public Vector2 GetPanDirection()
    {
        Vector2 result = Vector2.zero;
        
        switch (PanDirection)
        {
            case CameraPanDirection.Up:
                result = Vector2.up;
                break;
            case CameraPanDirection.Down:
                result = Vector2.down;
                break;
            case CameraPanDirection.Left:
                result = Vector2.left;
                break;
            case CameraPanDirection.Right:
                result = Vector2.right;
                break;
        }
        
        return result;
    }
}
