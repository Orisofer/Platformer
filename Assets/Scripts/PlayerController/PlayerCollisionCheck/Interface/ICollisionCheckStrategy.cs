using UnityEngine;

public interface ICollisionCheckStrategy
{
    public bool CheckCollision(Transform source);
}
