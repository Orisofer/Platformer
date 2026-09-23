using OriGame.Player;
using UnityEngine;

public class AbilityResolverBasic : IAbilityResolver
{
    private ResolvedMovement m_ResolvedMovement;
    
    public ref readonly ResolvedMovement ResolveMovement(PlayerMovementRequest[] requests)
    {
        int topPriorityX = int.MinValue;
        int topPriorityY = int.MinValue;

        Vector2 finalVelocity = Vector2.zero;
        float gravityScale = 1.0f;
        
        for (int i = 0; i < requests.Length; i++)
        {
            if (requests[i].PriorityOnX >= topPriorityX)
            {
                topPriorityX =  requests[i].PriorityOnX;
                finalVelocity.x = requests[i].Target.x;
            }
            
            if (requests[i].PriorityOnY >= topPriorityY)
            {
                topPriorityY =  requests[i].PriorityOnY;
                finalVelocity.y = requests[i].Target.y;
                gravityScale =  requests[i].GravityScale;
            }
        }
        
        m_ResolvedMovement.Clear();
        
        m_ResolvedMovement.Target = finalVelocity;
        m_ResolvedMovement.GravityScale = gravityScale;

        return ref m_ResolvedMovement;
    }
}
