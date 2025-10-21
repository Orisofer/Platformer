using UnityEngine;

public struct CollisionDetectionResult
{
    public static byte RightDiagonalPattern = 0b10001001;
    public static byte LeftDiagonalPattern = 0b00001100;
    
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
     * 0b00100000 = only upper ray was hit
     * 0b00110000 = upper ray and mid ray hit
     * 0b00111000 = full hit (all 3 rays)
     * 0b00011000 = mid ray and lower ray was hit
     * 0b00001000 = only lowe ray was hit
     *
     * most significant bit: 0 for left 1 for right
     */
}
