using System;
using System.Collections.Generic;

namespace FileManager.Services
{
    /// <summary>
    /// Provides centralized logging functionality for the FileManager application.
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs a general message or error.
        /// </summary>
        void Log(string message, Exception exception = null);

        /// <summary>
        /// Logs an error with method context.
        /// </summary>
        void LogError(string method, Exception ex, string message);

        /// <summary>
        /// Logs an error with message and exception.
        /// </summary>
        void LogError(string message, Exception ex);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        void LogWarning(string message, string source);

        /// <summary>
        /// Logs a warning message without source.
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs a security-related event with structured properties.
        /// </summary>
        void LogSecurityEvent(string eventType, string message, Dictionary<string, object> properties);

        /// <summary>
        /// Logs a security event with simple message (eventType defaults to "PathTraversal").
        /// </summary>
        void LogSecurityEvent(string message);

        /// <summary>
        /// Logs a file operation (create, delete, move, copy).
        /// </summary>
        void LogFileOperation(string operation, string path, bool success, string errorMessage = null);

        /// <summary>
        /// Logs a successful file operation.
        /// </summary>
        void LogFileOperation(string operation, string path);

        /// <summary>
        /// Logs a validation failure with details.
        /// </summary>
        void LogValidationFailure(string validationType, string input, string errorMessage);

        /// <summary>
        /// Logs an information message with source context.
        /// </summary>
        void LogInfo(string message, string source);

        /// <summary>
        /// Logs an information message without source.
        /// </summary>
        void LogInfo(string message);
    }
}
