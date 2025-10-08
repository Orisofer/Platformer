using System;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    [SerializeField] private BoxCollider2D m_Collider;
    [SerializeField] private Transform m_Graphic;
    [SerializeField] private bool m_IsTrigger;
    [SerializeField] private bool m_DrawGizmos;

    public void Initialize()
    {
        m_Collider.size = new Vector2(m_Graphic.localScale.x, m_Graphic.localScale.y);
        m_Collider.offset = Vector2.zero;
        m_Collider.isTrigger = m_IsTrigger;
    }
    
#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        if (m_DrawGizmos)
        {
            Gizmos.color = Color.yellow;
            Vector2 center = m_Collider.bounds.center;
            Gizmos.DrawWireCube(center, m_Collider.bounds.size);
        }
    }
    
#endif
}
