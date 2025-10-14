// ReSharper disable All
public static class Logger
{
    private static ILoggerBackend s_LoggerBackend = new LoggerBackendUnityConsole();
    
    public static void Log(ILogable logObject, string message)
    {
        if (!logObject.EnableLogging) return;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        s_LoggerBackend.Log($"{logObject.transform.name}:: {message}");
#endif
    }
    
    public static void LogWarning(ILogable logObject, string message)
    {
        if (!logObject.EnableLogging) return;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        s_LoggerBackend.LogWarning($"{logObject.transform.name}:: {message}");
#endif
    }
    
    public static void LogError(ILogable logObject, string message)
    {
        if (!logObject.EnableLogging) return;
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        s_LoggerBackend.LogError($"{logObject.transform.name}:: {message}");
#endif
    }
}