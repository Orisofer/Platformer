using UnityEngine;

public interface ICollisionDetectionStrategy
{
    public bool EnableDebugging { get; set; }
    public CollisionDetectionResult Calculate();
}
