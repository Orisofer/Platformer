public interface ILateUpdate : IObjectTransform
{
    int LateUpdatePriority { get; set; } // smaller priority values are executed earlier
    public bool EnableLateUpdate { get; }
    void OnLateUpdate();
}