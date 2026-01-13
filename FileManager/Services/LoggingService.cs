using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace FileManager.Services
{
    /// <summary>
    /// Provides centralized logging functionality using NLog.
    /// Supports structured logging with properties for better log analysis.
    /// </summary>
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
            logEventInfo.Properties["Method"] = method;
            logger.Log(logEventInfo);
        }

        public void LogError(string message, Exception ex)
        {
            logger.Error(ex, message);
        }

        public void LogWarning(string message, string source)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Warn,
                Message = message
            };
            logEventInfo.Properties["Source"] = source;
            logger.Log(logEventInfo);
        }

        public void LogWarning(string message)
        {
            logger.Warn(message);
        }

        public void LogSecurityEvent(string eventType, string message, Dictionary<string, object> properties)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Warn,
                Message = $"[SECURITY] {eventType}: {message}"
            };

            logEventInfo.Properties["EventType"] = eventType;
            logEventInfo.Properties["SecurityEvent"] = true;

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    logEventInfo.Properties[prop.Key] = prop.Value;
                }
            }

            logger.Log(logEventInfo);
        }

        public void LogSecurityEvent(string message)
        {
            LogSecurityEvent("PathTraversal", message, null);
        }

        public void LogFileOperation(string operation, string path, bool success, string errorMessage = null)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = success ? LogLevel.Info : LogLevel.Error,
                Message = $"File operation: {operation} on '{path}' - {(success ? "Success" : "Failed")}"
            };

            logEventInfo.Properties["Operation"] = operation;
            logEventInfo.Properties["Path"] = path;
            logEventInfo.Properties["Success"] = success;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                logEventInfo.Properties["ErrorMessage"] = errorMessage;
            }

            logger.Log(logEventInfo);
        }

        public void LogFileOperation(string operation, string path)
        {
            LogFileOperation(operation, path, true, null);
        }

        public void LogValidationFailure(string validationType, string input, string errorMessage)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Warn,
                Message = $"Validation failed: {validationType} - {errorMessage}"
            };

            logEventInfo.Properties["ValidationType"] = validationType;
            logEventInfo.Properties["Input"] = SanitizeForLogging(input);
            logEventInfo.Properties["ErrorMessage"] = errorMessage;

            logger.Log(logEventInfo);
        }

        public void LogInfo(string message, string source)
        {
            var logEventInfo = new LogEventInfo
            {
                Level = LogLevel.Info,
                Message = message
            };
            logEventInfo.Properties["Source"] = source;
            logger.Log(logEventInfo);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        /// <summary>
        /// Sanitizes input data for logging to prevent sensitive information exposure.
        /// </summary>
        private string SanitizeForLogging(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "[empty]";
            }

            // Limit length for logging
            if (input.Length > 100)
            {
                return input.Substring(0, 97) + "...";
            }

            return input;
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
