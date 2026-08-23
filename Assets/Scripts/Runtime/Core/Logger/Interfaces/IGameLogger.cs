namespace OriGame.Core
{
    public interface IGameLogger
    {
        public bool Enabled { get; set; }
        public void Log(string message);
        public void LogWarning(string message);
        public void LogError(string message);
    }
}
