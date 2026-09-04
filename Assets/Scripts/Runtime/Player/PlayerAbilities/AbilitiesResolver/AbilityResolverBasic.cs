using OriGame.Player;
using UnityEngine;

public class AbilityResolverBasic : IAbilityResolver
{
    public Vector2 ResolveMovement(ref PlayerMovementRequest[] requests, PlayerContext playerContext)
    {
        int topPriorityX = int.MinValue;
        int topPriorityY = int.MinValue;

        Vector2 finalVelocity = Vector2.zero;
        
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
            }
        }

        return finalVelocity;
    }
}
