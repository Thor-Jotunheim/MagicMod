using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MagicMod
{
    [BepInPlugin("com.MagicMod", "MagicMod", "0.0.1")]
    [BepInProcess("valheim")] // Updated for macOS
    public class MagicMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.MagicMod");

        private readonly string requiredGameVersion = "0.219.16"; // Set the required version here

        void Awake()
        {
            // Check the game version before patching
            string gameVersion = GetGameVersion();  // Get the game version as a string
            Logger.LogInfo($"Detected game version: {gameVersion}");

            if (gameVersion != requiredGameVersion)
            {
                Logger.LogError($"Incorrect game version! Expected: {requiredGameVersion}, Found: {gameVersion}");
                return; // Stop the mod from running if the version doesn't match
            }
            else
            {
                Logger.LogInfo($"Correct game version detected: {gameVersion}");
            }

            // Proceed with patching if the version matches
            harmony.PatchAll();
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

            // Ensure the method always returns a value, even if no version is found
            return "Unknown";
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
        class Jump_Patch
        {
            static void Prefix(ref float ___m_jumpForce)
            {
                UnityEngine.Debug.Log($"[Jump_Patch] Original jump force: {___m_jumpForce}");
                
                if (___m_jumpForce == 0)
                {
                    UnityEngine.Debug.LogWarning("[Jump_Patch] Jump force is zero, check character state!");
                }

                ___m_jumpForce = 15; // Modify jump force to 15
                UnityEngine.Debug.Log($"[Jump_Patch] Modified jump force: {___m_jumpForce}");
            }
        }
    }
}