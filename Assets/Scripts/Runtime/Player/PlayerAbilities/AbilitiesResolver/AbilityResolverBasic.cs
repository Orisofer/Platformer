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
            ref readonly PlayerMovementRequest request = ref requests[i];
            
            if (request.PriorityOnX >= topPriorityX)
            {
                topPriorityX =  request.PriorityOnX;
                finalVelocity.x = request.Target.x;
            }
            
            if (request.PriorityOnY >= topPriorityY)
            {
                topPriorityY =  request.PriorityOnY;
                finalVelocity.y = request.Target.y;
                gravityScale =  request.GravityScale;
            }
        }
        
        m_ResolvedMovement.Clear();
        
        m_ResolvedMovement.Target = finalVelocity;
        m_ResolvedMovement.GravityScale = gravityScale;

        return ref m_ResolvedMovement;
    }
}
