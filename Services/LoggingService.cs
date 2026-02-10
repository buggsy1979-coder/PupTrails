using System;
using System.IO;

namespace PupTrailsV3.Services
{
    public static class LoggingService
    {
        private static readonly string LogFilePath;

        static LoggingService()
        {
            var logDir = PathManager.LogsDirectory;
            Directory.CreateDirectory(logDir);
            LogFilePath = Path.Combine(logDir, $"PupTrail_{DateTime.Now:yyyyMMdd}.log");
        }

        public static void LogError(string message, Exception? exception = null)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                if (exception != null)
                {
                    logEntry += $"\nException: {exception.GetType().Name}: {exception.Message}\nStackTrace: {exception.StackTrace}";
                }
                File.AppendAllText(LogFilePath, logEntry + "\n\n");
            }
            catch
            {
                // Silently fail if logging fails
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
                File.AppendAllText(LogFilePath, logEntry + "\n");
            }
            catch
            {
                // Silently fail if logging fails
            }
        }

        public static void LogWarning(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WARNING: {message}";
                File.AppendAllText(LogFilePath, logEntry + "\n");
            }
            catch
            {
                // Silently fail if logging fails
            }
        }

        public static string GetLogFilePath() => LogFilePath;
    }
}
