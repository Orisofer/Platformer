using UnityEngine;

public class CollisionDetectionGroundRaycasts : ICollisionDetectionStrategy, ILogable
{
    private const int NUM_RAYS_FOR_GROUND = 3;
    private const float RAY_DISTANCE = 0.05f;

    private Collider2D m_CachedCollider;
    private Vector2 m_ColliderLocalSize;
    private float m_ColliderHalfHeight;
    private float m_ColliderHalfWidth;
    private float m_RaysXInterval;
    private bool m_CachedQueryStartInColliders;
    public bool EnableLogging { get; set; }
    public bool EnableDebugging { get; set; }
    public Transform transform => m_CachedCollider.transform;
    
    
    public CollisionDetectionGroundRaycasts(Collider2D cachedCollider, bool withDebugging, bool  withLogging)
    {
        EnableDebugging = withDebugging;
        EnableLogging = withLogging;

        CacheColliderProperties(cachedCollider);
    }

    private void CacheColliderProperties(Collider2D collider)
    {
        m_CachedCollider = collider;
        
        if (collider is BoxCollider2D)
        {
            m_ColliderLocalSize = (collider as BoxCollider2D).size;
            
            m_ColliderHalfHeight = m_ColliderLocalSize.y * 0.5f;
            m_ColliderHalfWidth = m_ColliderLocalSize.x * 0.5f;
        
            m_RaysXInterval = (m_ColliderHalfWidth * 2) / (NUM_RAYS_FOR_GROUND - 1);
        }
        else
        {
            Logger.LogError(this, "collider is not BoxCollider2D");
        }
    }

    public CollisionDetectionResult Calculate()
    {
        m_CachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;

        CollisionDetectionResult result = new CollisionDetectionResult()
        {
            Collided = false,
            CollidedTransform = null,
        };
        
        float colliderPositionX = m_CachedCollider.transform.position.x;
        float rayOriginY = m_CachedCollider.transform.position.y - m_ColliderHalfHeight + 0.05f;

        for (int i = 0; i < NUM_RAYS_FOR_GROUND; i++)
        {
            float rayXPos = (colliderPositionX - m_ColliderHalfWidth) + (m_RaysXInterval * i);
            
            Vector2 rayOrigin = new Vector2(rayXPos, rayOriginY);
            RaycastHit2D rayCastHit = Physics2D.Raycast(rayOrigin, Vector2.down, RAY_DISTANCE);
            
            if (EnableDebugging)
            {
                DrawDebugRay(rayOrigin);
            }
            
            if (rayCastHit)
            {
                result.Collided = true;
                result.CollidedTransform = rayCastHit.transform;
            }
        }
        
        Physics2D.queriesStartInColliders = m_CachedQueryStartInColliders;

        return result;
    }

    private void DrawDebugRay(Vector2 rayOrigin)
    {
        Debug.DrawRay(rayOrigin, Vector2.down * RAY_DISTANCE);
    }
}
