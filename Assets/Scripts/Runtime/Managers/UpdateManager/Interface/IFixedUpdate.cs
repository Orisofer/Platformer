using OriGame.Core;
using UnityEngine;

public interface IFixedUpdate : IObjectTransform
{
    int FixedUpdatePriority { get; set; } // smaller priority values are executed earlier
    public bool EnableFixedUpdate { get; set; }
    public void OnFixedUpdate(float fixedDeltaTime);
}
