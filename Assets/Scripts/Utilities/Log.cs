using UnityEngine;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System;

namespace CardDuel.Utils
{
    public enum LogCategory { UI, Application, Gameplay, Network, System, Error }

    public static class Log
    {
        // ===== GLOBAL SWITCH =====
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public const bool ENABLED = true;
#else
        public const bool ENABLED = false;
#endif

        // ===== FILE SETTINGS =====
        private static readonly string FilePath;
        private const string FileName = "GameLog.txt";

        // ===== CATEGORY SWITCHES =====
        public static bool Ui = true;
        public static bool Application = true;
        public static bool Gameplay = true;
        public static bool Network = true;
        public static bool System = true;

        static Log()
        {
            // Set path to: C:/Users/Name/AppData/LocalLow/Company/Product/GameLog.txt (Windows)
            // Or /storage/emulated/0/Android/data/com.Company.Product/files/GameLog.txt (Android)
            FilePath = Path.Combine(UnityEngine.Application.persistentDataPath, FileName);
            UnityEngine.Debug.Log(" FilePath :"+ FilePath);

            // Clear previous log on startup
            if (ENABLED)
            {
                File.WriteAllText(FilePath, $"--- Log Started: {DateTime.Now} ---\n");
            }
        }

        // ===== PUBLIC API =====

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(LogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!IsCategoryEnabled(category)) return;

            string formatted = Format(category, message);
            UnityEngine.Debug.Log(formatted, context);
            WriteToFile(formatted);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warn(LogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!IsCategoryEnabled(category)) return;

            string formatted = Format(category, message, "WARN");
            UnityEngine.Debug.LogWarning(formatted, context);
            WriteToFile(formatted);
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            string formatted = Format(LogCategory.Error, message, "ERROR");
            UnityEngine.Debug.LogError(formatted, context);

            if (ENABLED) WriteToFile(formatted);
        }

        // ===== FAST HELPERS =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UI(string message, UnityEngine.Object ctx = null) => Info(LogCategory.UI, message, ctx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void App(string message, UnityEngine.Object ctx = null) => Info(LogCategory.Application, message, ctx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Game(string message, UnityEngine.Object ctx = null) => Info(LogCategory.Gameplay, message, ctx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Net(string message, UnityEngine.Object ctx = null) => Info(LogCategory.Network, message, ctx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sys(string message, UnityEngine.Object ctx = null) => Info(LogCategory.System, message, ctx);

        // ===== INTERNAL =====

        private static void WriteToFile(string logLine)
        {
            try
            {
                // Use File.AppendAllText for simple thread-safety in this static context
                File.AppendAllText(FilePath, logLine + Environment.NewLine);
            }
            catch (Exception) { /* Avoid infinite recursion if file writing fails */ }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCategoryEnabled(LogCategory category)
        {
            return category switch
            {
                LogCategory.UI => Ui,
                LogCategory.Application => Application,
                LogCategory.Gameplay => Gameplay,
                LogCategory.Network => Network,
                LogCategory.System => System,
                _ => true
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Format(LogCategory category, string message, string prefix = "INFO")
        {
            // Cache DateTime string to avoid repeated formatting
            var timeString = DateTime.Now.ToString("HH:mm:ss");
            return "[" + timeString + "] [" + Time.frameCount + "] [" + prefix + "] [" + category + "] " + message;
        }
    }
}