using UnityEngine;

namespace OriGame.Player
{
    public interface ICollisionDetectionStrategy
    {
        public bool EnableDebugging { get; set; }
        public ref readonly CollisionDetectionResult Calculate();
    }
}

