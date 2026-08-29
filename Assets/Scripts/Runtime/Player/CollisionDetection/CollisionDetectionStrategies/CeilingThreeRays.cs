using UnityEngine;

public class CeilingThreeRays : ICollisionDetectionStrategy
{
    private PlayerContext m_Ctx;
    private BoxCollider2D m_BoxCollider2D;
    private CollisionDetectionResult m_Result;
    private ContactFilter2D m_Filter;
    private RaycastHit2D[] m_HitsBuffer;
    private float m_RaysInterval;
    private float m_SkinWidth;
    
    public bool EnableDebugging { get; set; }
    
    public CeilingThreeRays(PlayerContext cachedCollider, bool withDebugging)
    {
        m_Ctx = cachedCollider;
        EnableDebugging = withDebugging;
        m_BoxCollider2D = cachedCollider.ColliderBody;
        m_SkinWidth = cachedCollider.SkinWidth;
        m_Result = new CollisionDetectionResult();
        
        InitRaysIntervals();
        
        m_Filter = new ContactFilter2D
        {
            useTriggers = false
        };
        
        m_Filter.SetLayerMask(LayerMask.GetMask("Ground"));
    }

    private void InitRaysIntervals()
    {
        m_HitsBuffer = new RaycastHit2D[1];

        m_RaysInterval = (m_BoxCollider2D.bounds.size.x - (m_SkinWidth * 2)) * 0.5f;
    }
    
    public ref readonly CollisionDetectionResult Calculate()
    {
        Bounds colBounds = m_BoxCollider2D.bounds;
        float originY = colBounds.max.y - m_SkinWidth;
        float rayDistance = m_SkinWidth + Mathf.Max(0f, m_Ctx.FrameVelocity.y * Time.fixedDeltaTime);
        
        int hitPattern = 0b00000000;
        int overallHitsThisRound = 0;
        
        for (int i = 0; i < 3; i++)
        {
            float originX = colBounds.min.x + m_SkinWidth + (m_RaysInterval * i);
            
            int numHits = Physics2D.RaycastNonAlloc(new Vector2(originX, originY), Vector2.up, m_HitsBuffer, rayDistance,m_Filter.layerMask);

            if (numHits == 1)
            {
                m_Result.Collided = true;
                m_Result.CollidedTransform = m_HitsBuffer[0].transform;
                m_Result.Distance = originY - m_HitsBuffer[0].point.y;
                
                hitPattern |= 1 << (2 - i);

                overallHitsThisRound++;
            }
            
            if (EnableDebugging)
            {
                Debug.DrawRay(new Vector2(originX, originY), Vector2.up * rayDistance, m_HitsBuffer[0].collider != null ? Color.green : Color.red);
            
                Vector2 colStart = new Vector2(colBounds.min.x, colBounds.min.y);
                Vector2 colEnd = new Vector2(colBounds.max.x, colBounds.min.y);
                Debug.DrawRay(colStart, (colEnd - colStart) * (colBounds.size.x), Color.yellow);
            }
        }

        if (overallHitsThisRound == 0)
        {
            m_Result.Collided = false;
            m_Result.CollidedTransform = null;
            m_Result.Distance = 0f;
        }
        
        m_Result.HitPattern = (byte)hitPattern;
        
        return ref m_Result;
    }
}
