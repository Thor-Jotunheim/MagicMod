using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MagicMod
{
    public class DebugLogger
    {
        private static string logFilePath;

        // **Master Logging Toggle**
        public static bool IsDebugEnabled = true;

        static DebugLogger()
        {
            string pluginPath = BepInEx.Paths.PluginPath;
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);

            logFilePath = Path.Combine(pluginPath, "MagicModDebug.log");

            // **Force-create the log file**
            if (!File.Exists(logFilePath))
            {
                File.WriteAllText(logFilePath, "=== MagicMod Debug Log ===\n");
            }
        }

        public static void LogMessage(string message)
        {
            if (!IsDebugEnabled) return;

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            Debug.Log(logEntry);

            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MagicMod] Failed to write log: {ex.Message}");
            }
        }
    }

    // **🔥 Universal Debug Hook - Logs All Method Calls from Targeted Classes**
    [HarmonyPatch]
    public static class UniversalMethodLogger
    {
        static bool Prepare(MethodBase original)
        {
            if (original == null || original.DeclaringType == null)
                return false;

            string className = original.DeclaringType.FullName;

            // **Log which classes are being patched**
            DebugLogger.LogMessage($"[MagicMod] Checking class: {className}");

            return className.StartsWith("Player") ||
                   className.StartsWith("ItemStand") ||
                   className.StartsWith("Inventory") ||
                   className.StartsWith("Container") ||
                   className.StartsWith("Smelter");
        }

        static void Prefix(MethodBase __originalMethod, object[] __args)
        {
            if (!DebugLogger.IsDebugEnabled) return;

            string methodName = __originalMethod.Name;
            string className = __originalMethod.DeclaringType?.FullName ?? "Unknown";
            string args = __args != null && __args.Length > 0 ? string.Join(", ", __args) : "No Args";

            DebugLogger.LogMessage($"Called: {className}.{methodName}({args})");
        }
    }
}