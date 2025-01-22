using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace MagicMod
{
    [BepInPlugin("com.MagicMod", "MagicMod", "0.0.1")]
    [BepInProcess("valheim")]
    public class MagicMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.MagicMod");
        private readonly string requiredGameVersion = "0.219.16"; // Set the required version here

        void Awake()
        {
            // Enable Debug Logging
            DebugLogger.IsDebugEnabled = true;  // Enable logging
            DebugLogger.LogMessage("[MagicMod] Debug logger is active!");

            // Check the game version before patching
            string gameVersion = GetGameVersion();
            Logger.LogInfo($"Detected game version: {gameVersion}");

            if (gameVersion != requiredGameVersion)
            {
                Logger.LogError($"Incorrect game version! Expected: {requiredGameVersion}, Found: {gameVersion}");
                return; // Stop the mod from running if the version doesn't match
            }

            Logger.LogInfo($"Correct game version detected: {gameVersion}");

            // Apply all Harmony patches (logs all patched methods)
            harmony.PatchAll();

            // Debugging: List all patched methods
            foreach (var method in harmony.GetPatchedMethods())
            {
                DebugLogger.LogMessage($"[MagicMod] ✅ Patched: {method.DeclaringType?.FullName}.{method.Name}");
            }
        }

        private string GetGameVersion()
        {
            try
            {
                string homeDir = Environment.GetEnvironmentVariable("HOME");
                string assemblyPath = Path.Combine(homeDir, "Library/Application Support/Steam/steamapps/common/Valheim/Valheim.app/Contents/Resources/Data/Managed/assembly_valheim.dll");

                if (File.Exists(assemblyPath))
                {
                    var assembly = Assembly.LoadFile(assemblyPath);
                    Logger.LogInfo($"Assembly loaded: {assembly.FullName}");

                    // Look for the 'Version' class
                    var versionType = assembly.GetType("Version");

                    if (versionType != null)
                    {
                        Logger.LogInfo($"Found 'Version' class in assembly");

                        // Look for the CurrentVersion property inside the 'Version' class
                        var currentVersionProperty = versionType.GetProperty("CurrentVersion", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

                        if (currentVersionProperty != null)
                        {
                            Logger.LogInfo("Found CurrentVersion property");

                            // Get the value of CurrentVersion
                            var currentVersion = currentVersionProperty.GetValue(null);

                            // Log the value of CurrentVersion
                            Logger.LogInfo($"CurrentVersion value: {currentVersion}");

                            // Return the version as a string directly from the GameVersion
                            return currentVersion.ToString();
                        }
                        else
                        {
                            Logger.LogError("CurrentVersion property not found in Version class");
                        }
                    }
                    else
                    {
                        Logger.LogError("'Version' class not found in assembly");
                    }
                }
                else
                {
                    Logger.LogError($"assembly_valheim.dll not found at {assemblyPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error retrieving version from assembly: {ex.Message}");
            }

            return "Unknown";
        }
    }
}