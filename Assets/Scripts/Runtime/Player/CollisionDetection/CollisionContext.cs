using UnityEngine;

[System.Serializable]
public struct CollisionContext
{
    public CollisionDetectionResult Ground;
    public CollisionDetectionResult Ceiling;
    public CollisionDetectionResult WallLeft;
    public CollisionDetectionResult WallRight;
}
