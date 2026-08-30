using UnityEngine;

namespace OriGame.Core
{
    public class UnityGameLogger : IGameLogger
    {
        public bool Enabled { get; set; }

        public UnityGameLogger()
        {
            Enabled = true;
        }

        public void Log(string message)
        {
            if (Enabled)
            {
                Debug.Log(message);
            }
        }

        public void LogWarning(string message)
        {
            if (Enabled)
            {
                Debug.LogWarning(message);
            }
        }

        public void LogError(string message)
        {
            if (Enabled)
            {
                Debug.LogError(message);
            }
        }
    }
}