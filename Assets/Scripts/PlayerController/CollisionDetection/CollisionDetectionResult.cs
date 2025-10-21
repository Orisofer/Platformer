using UnityEngine;

public struct CollisionDetectionResult
{
    public Transform CollidedTransform;
    public byte HitPattern;
    public float Distance;
    public bool Collided;

    public static implicit operator bool(CollisionDetectionResult collisionDetectionResult)
    {
        return collisionDetectionResult.Collided;
    }
    /*
     * --- Collision Key map -----
     * 
     * Vertical rays collisions map:
     * 
     * 0b00000100 = only left most ray was hit
     * 0b00000110 = left and middle was hit
     * 0b00000111 = full hit (all 3 rays)
     * 0b00000011 = right and middle hit
     * 0b00000001 = only right
     * 
     * Horizontal rays collisions map: (todo)
     * 
     * 0b00000100 = only upper ray was hit
     * 0b00000110 = upper ray and mid ray hit
     * 0b00000111 = full hit (all 3 rays)
     * 0b00000011 = mid ray and lower ray was hit
     * 0b00000001 = only lowe ray was hit
     */
}
