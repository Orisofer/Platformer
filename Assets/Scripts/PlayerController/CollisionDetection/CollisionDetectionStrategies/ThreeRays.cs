using UnityEngine;

public class ThreeRays : ICollisionDetectionStrategy
{
    private PlayerContext m_Ctx;
    private BoxCollider2D m_BoxCollider2D;
    private CollisionDetectionResult m_Result;
    private ContactFilter2D m_Filter;
    private RaycastHit2D[] m_HitsBuffer;
    private Vector2 m_Direction;
    private float[] m_RaysIntervals;
    private float m_SkinWidth;
    
    public bool EnableDebugging { get; set; }
    
    public ThreeRays(PlayerContext cachedCollider, Vector2 direction, string layer, bool withDebugging)
    {
        m_Ctx = cachedCollider;
        EnableDebugging = withDebugging;
        m_BoxCollider2D = cachedCollider.ColliderBody;
        m_SkinWidth = cachedCollider.SkinWidth;
        m_Result = new CollisionDetectionResult();
        m_Direction = direction;
        
        InitRaysIntervals();
        
        m_Filter = new ContactFilter2D
        {
            useTriggers = false
        };
        
        m_Filter.SetLayerMask(LayerMask.GetMask(layer));
    }

    private void InitRaysIntervals()
    {
        m_HitsBuffer = new RaycastHit2D[1];
        m_RaysIntervals = new float[3];

        float interval = 0f;

        if (m_Direction == Vector2.left || m_Direction == Vector2.right)
        {
            interval = (m_BoxCollider2D.bounds.size.y - m_SkinWidth) / (m_RaysIntervals.Length - 1);
        }
        else if (m_Direction == Vector2.up || m_Direction == Vector2.down)
        {
            interval = (m_BoxCollider2D.bounds.size.x - m_SkinWidth) / (m_RaysIntervals.Length - 1);
        }
        
        float currentInterval = 0f;

        for (int i = 0; i < m_RaysIntervals.Length; i++)
        {
            m_RaysIntervals[i] = currentInterval;
            currentInterval += interval;
        }
    }
    
    public ref readonly CollisionDetectionResult Calculate()
    {
        Bounds colBounds = m_BoxCollider2D.bounds;

        float originY = 0f;
        float originX = 0f;
        float rayDistance = 0f;
        
        if (m_Direction == Vector2.down)
        {
            originY = colBounds.min.y + m_SkinWidth;
            originX = colBounds.min.x;
            
            rayDistance = m_SkinWidth + Mathf.Max(0f, -m_Ctx.ThisFrameVelocity.y * Time.fixedDeltaTime);
        }
        else if (m_Direction == Vector2.up)
        {
            originY = colBounds.max.y - m_SkinWidth;
            originX = colBounds.min.x;
            
            rayDistance = m_SkinWidth + Mathf.Max(0f, m_Ctx.ThisFrameVelocity.y * Time.fixedDeltaTime);
        }
        else if (m_Direction == Vector2.left)
        {
            originY = colBounds.min.y;
            originX = colBounds.min.x + m_SkinWidth;
            
            rayDistance = m_SkinWidth + Mathf.Max(0f, -m_Ctx.ThisFrameVelocity.x * Time.fixedDeltaTime);
        }
        else if (m_Direction == Vector2.right)
        {
            originY = colBounds.min.y;
            originX = colBounds.max.x - m_SkinWidth;
            
            rayDistance = m_SkinWidth + Mathf.Max(0f, m_Ctx.ThisFrameVelocity.x * Time.fixedDeltaTime);
        }
        
        int hitPattern = 0b00000000;
        int overallHitsThisRound = 0;
        
        for (int i = 0; i < m_RaysIntervals.Length; i++)
        {
            if (m_Direction == Vector2.down || m_Direction == Vector2.up)
            {
                originX = colBounds.min.x + m_RaysIntervals[i];
            }
            else if (m_Direction == Vector2.left || m_Direction == Vector2.right)
            {
                originY = colBounds.min.y + m_RaysIntervals[i];
            }
            
            var numHits = Physics2D.RaycastNonAlloc(new Vector2(originX, originY), m_Direction, m_HitsBuffer, rayDistance,m_Filter.layerMask);

            if (numHits == 1)
            {
                m_Result.Collided = true;
                m_Result.CollidedTransform = m_HitsBuffer[0].transform;
                m_Result.Distance = originY - m_HitsBuffer[0].point.y;
                
                hitPattern |= 1 << (m_RaysIntervals.Length - 1 - i);

                overallHitsThisRound++;
            }
            
            if (EnableDebugging)
            {
                Debug.DrawRay(new Vector2(originX, originY), m_Direction * rayDistance, m_HitsBuffer[0].collider != null ? Color.green : Color.red);
            
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
