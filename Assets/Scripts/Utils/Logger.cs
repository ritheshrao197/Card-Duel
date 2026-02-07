using UnityEngine;

namespace CardDuel.Utils
{
    public enum LogCategory
    {
        Network,
        Gameplay,
        UI,
        State,
        Error
    }

    public static class Logger
    {
        public static bool EnableLogs = true;

        public static void Log(LogCategory category, string message)
        {
            if (!EnableLogs) return;
            Debug.Log(Format(category, message));
        }

        public static void Warn(LogCategory category, string message)
        {
            if (!EnableLogs) return;
            Debug.LogWarning(Format(category, message));
        }

        public static void Error(LogCategory category, string message)
        {
            Debug.LogError(Format(category, message));
        }

        private static string Format(LogCategory category, string message)
        {
            return $"[CardDuel][{category}] {message}";
        }
    }
}
