using System;

namespace FileManager.Services
{
    public interface ILoggingService
    {
        void Log(string message, Exception exception = null);
        void LogError(string method, Exception ex, string message);
    }
}
