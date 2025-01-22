using HarmonyLib;
using UnityEngine;

namespace MagicMod
{
    // This class handles the jump modification
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