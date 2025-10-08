using UnityEngine;

public class GroundOneRay : ICollisionDetectionStrategy
{
    private PlayerContext m_Ctx;
    private BoxCollider2D m_BoxCollider2D;
    private bool m_CachedQueriesStartInColliders;
    public bool EnableDebugging { get; set; }

    public GroundOneRay(PlayerContext cachedCollider, bool withDebugging)
    {
        m_Ctx = cachedCollider;
        EnableDebugging = withDebugging;
        m_BoxCollider2D = cachedCollider.ColliderBody;
        m_CachedQueriesStartInColliders = Physics2D.queriesStartInColliders;
    }

    public CollisionDetectionResult Calculate()
    {
        Physics2D.queriesStartInColliders = false;
        
        CollisionDetectionResult result = new CollisionDetectionResult
        {
            CollidedTransform = null,
            Collided = false,
        };
        
        Bounds colBounds = m_BoxCollider2D.bounds;
        float skinWidth = 0.02f;
        float originY = colBounds.min.y + skinWidth;
        float originX = colBounds.center.x;
        Vector2 origin = new Vector2(originX, originY);
        float rayDistance = skinWidth + Mathf.Max(0f, -m_Ctx.Velocity.y * Time.fixedDeltaTime);
        
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance);

        if (hit)
        {
            result.Collided = true;
            result.CollidedTransform = hit.transform;
            result.Distance = origin.y - hit.point.y;
        }

        if (EnableDebugging)
        {
            Debug.DrawRay(origin, Vector2.down * rayDistance, hit ? Color.green : Color.red);
            
            Vector2 colStart = new Vector2(colBounds.min.x, colBounds.min.y);
            Vector2 colEnd = new Vector2(colBounds.max.x, colBounds.min.y);
            Debug.DrawRay(colStart, (colEnd - colStart) * colBounds.extents.x, Color.yellow);
        }
        
        Physics2D.queriesStartInColliders = m_CachedQueriesStartInColliders;

        return result;
    }
}
