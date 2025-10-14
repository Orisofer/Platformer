using UnityEngine;

public struct CollisionDetectionResult
{
    public Transform CollidedTransform;
    public byte HitPattern;
    public float Distance;
    public bool Collided;
    
    /*
     * Collision Key map:
     * -1 = only left most ray was hit
     * -2 = left and middle was hit
     * 3 = full hit (all 3 rays)
     * 2 = right and middle hit
     * 1 = only right
     */
}
