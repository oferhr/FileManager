using System;
using System.IO;
using NLog;

namespace FileManager.Services
{
    public class LoggingService : ILoggingService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Log(string message, Exception exception = null)
        {
            if (exception != null)
            {
                logger.Error(exception, message);
            }
            else
            {
                logger.Info(message);
            }
        }

        public void LogError(string method, Exception ex, string message)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Error,
                Exception = ex,
                Message = message
            };
            logger.Log(logEventInfo);
        }

        public static void LogToFile(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
    }
}
