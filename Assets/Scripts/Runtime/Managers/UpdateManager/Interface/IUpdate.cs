using OriGame.Core;

public interface IUpdate : IObjectTransform
{
    int UpdatePriority { get; set; } // smaller priority values are executed earlier
    public bool EnableUpdate { get; set; }
    public void OnUpdate(float deltaTime);
}
