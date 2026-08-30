using UnityEngine;

public struct SnapRequest
{
    public Transform Target;
    public SnapDirection Direction;
    public float Distance;

    public void Clear()
    {
        Target = null;
        Direction = SnapDirection.None;
        Distance = 0f;
    }
}
