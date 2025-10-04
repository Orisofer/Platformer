using UnityEngine;

public class CollisionCheckGround : ICollisionCheckStrategy
{
    private LayerMask m_GroundMask;
    private float m_GroundRadius;

    public CollisionCheckGround(LayerMask groundMask, float groundRadius)
    {
        
    }
    
    public bool CheckCollision(Transform source)
    {
        bool grounded = Physics.CheckSphere(source.position, m_GroundRadius, m_GroundMask);
        return grounded;
    }
}
