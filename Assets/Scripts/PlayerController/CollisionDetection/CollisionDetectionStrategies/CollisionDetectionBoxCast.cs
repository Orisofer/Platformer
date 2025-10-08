using UnityEngine;

public class CollisionDetectionBoxCast : ICollisionDetectionStrategy
{
    private PlayerContext m_PlayerContext;
    private BoxCollider2D m_Collider;
    private bool m_CachedQueriesStartInColliders;
    
    public bool EnableDebugging { get; set; }

    public CollisionDetectionBoxCast(PlayerContext playerContext)
    {
        m_PlayerContext = playerContext;
        m_Collider = m_PlayerContext.ColliderBody;

        m_CachedQueriesStartInColliders = Physics2D.queriesStartInColliders;
    }
    
    public CollisionDetectionResult Calculate()
    {
        Physics2D.queriesStartInColliders = false;
        
        CollisionDetectionResult result = default;
        
        Vector2 boundsCenter = m_Collider.bounds.center;
        Vector2 boundsExtents = m_Collider.bounds.extents;
        
        Vector2 origin = new Vector2(boundsCenter.x, boundsCenter.y - boundsExtents.y + 0.05f * 0.5f);
        
        Vector2 castSize = new Vector2(m_Collider.size.x * m_Collider.transform.lossyScale.x * 0.95f,
            m_Collider.size.y * m_Collider.transform.lossyScale.y * 0.95f);
        
        RaycastHit2D hit = Physics2D.BoxCast(origin, castSize, 0f, Vector2.down, 0.05f, m_PlayerContext.PlayerLayer);
        
        if (EnableDebugging)
        {
            Color c = hit ? Color.green : Color.red;
            Debug.DrawLine(origin + Vector2.left * castSize.x * 0.5f, origin + Vector2.right * castSize.x * 0.5f, c);
        }

        if (hit)
        {
            result.Collided = true;
            result.CollidedTransform = hit.transform;
        }
        
        Physics2D.queriesStartInColliders = m_CachedQueriesStartInColliders;

        return result;
    }
}
