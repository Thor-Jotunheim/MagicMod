using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace MagicMod
{
    [HarmonyPatch(typeof(ItemStand), "SetVisualItem")]
    public static class ItemStandPatch
    {
        [HarmonyPrefix]
        public static bool BeforeSetVisualItem(ItemStand __instance, ref string itemName, int variant, int quality)
        {
            if (!DebugLogger.IsDebugEnabled) return true;

            DebugLogger.LogMessage($"[MagicMod] Attempting to place item: {itemName}");

            // ✅ Use CoroutineHandler to properly run the delay
            CoroutineHandler.Instance.RunCoroutine(DelayedPlacement(__instance, itemName, variant, quality));
            return false; // **Prevents the original function from executing immediately**
        }

        private static IEnumerator DelayedPlacement(ItemStand itemStand, string itemName, int variant, int quality)
        {
            float holdTime = 3.8f;
            float elapsedTime = 0f;

            ItemStandTimer.ShowPlacementTimer(); // **Show Timer UI**

            while (elapsedTime < holdTime)
            {
                if (!IsHotbarKeyHeld()) // ✅ Checks hotbar keys instead of `E`
                {
                    DebugLogger.LogMessage("[MagicMod] Placement canceled! Key released early.");
                    ItemStandTimer.HidePlacementTimer();
                    yield break; // **Cancel placement if key is released**
                }

                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsedTime); // **Update UI Timer**
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            DebugLogger.LogMessage($"[MagicMod] Item {itemName} placed successfully!");

            // ✅ Use Reflection to call SetVisualItem AFTER the delay
            MethodInfo method = typeof(ItemStand).GetMethod("SetVisualItem", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(itemStand, new object[] { itemName, variant, quality });
            }
            else
            {
                DebugLogger.LogMessage("[MagicMod] ERROR: Could not find SetVisualItem method via reflection.");
            }

            ItemStandTimer.HidePlacementTimer(); // **Hide Timer UI**
        }

        private static bool IsHotbarKeyHeld()
        {
            return UnityEngine.Input.GetKey(KeyCode.Alpha1) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha2) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha3) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha4) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha5) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha6) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha7) ||
                   UnityEngine.Input.GetKey(KeyCode.Alpha8);
        }
    }
}