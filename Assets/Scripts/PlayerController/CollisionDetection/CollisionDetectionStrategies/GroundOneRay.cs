using UnityEngine;

public class GroundOneRay : ICollisionDetectionStrategy
{
    private PlayerContext m_Ctx;
    private BoxCollider2D m_BoxCollider2D;
    private CollisionDetectionResult m_Result;
    private ContactFilter2D m_Filter;
    private float m_SkinWidth;
    public bool EnableDebugging { get; set; }

    public GroundOneRay(PlayerContext cachedCollider, bool withDebugging)
    {
        m_Ctx = cachedCollider;
        EnableDebugging = withDebugging;
        m_BoxCollider2D = cachedCollider.ColliderBody;
        m_SkinWidth = cachedCollider.SkinWidth;
        m_Result = new CollisionDetectionResult();
        m_Filter = new ContactFilter2D
        {
            useTriggers = false
        };
        m_Filter.SetLayerMask(LayerMask.GetMask("Ground"));
    }

    public CollisionDetectionResult Calculate()
    {
        Bounds colBounds = m_BoxCollider2D.bounds;
        float originY = colBounds.min.y + m_SkinWidth;
        float originX = colBounds.center.x;
        Vector2 origin = new Vector2(originX, originY);
        float rayDistance = m_SkinWidth + Mathf.Max(0f, -m_Ctx.ThisFrameVelocity.y * Time.fixedDeltaTime);
        
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, m_Filter.layerMask);

        if (hit)
        {
            m_Result.Collided = true;
            m_Result.CollidedTransform = hit.transform;
            m_Result.Distance = origin.y - hit.point.y;
        }
        else
        {
            m_Result.Collided = false;
            m_Result.CollidedTransform = null;
            m_Result.Distance = 0f;
        }

        if (EnableDebugging)
        {
            Debug.DrawRay(origin, Vector2.down * rayDistance, hit ? Color.green : Color.red);
            
            Vector2 colStart = new Vector2(colBounds.min.x, colBounds.min.y);
            Vector2 colEnd = new Vector2(colBounds.max.x, colBounds.min.y);
            Debug.DrawRay(colStart, (colEnd - colStart) * (colBounds.size.x), Color.yellow);
        }
        
        return m_Result;
    }
}
