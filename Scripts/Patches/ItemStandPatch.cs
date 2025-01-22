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

            __instance.StartCoroutine(DelayedPlacement(__instance, itemName, variant, quality));
            return false; // **Prevents the original function from executing immediately**
        }

        private static IEnumerator DelayedPlacement(ItemStand itemStand, string itemName, int variant, int quality)
        {
            float holdTime = 3.8f;
            float elapsedTime = 0f;

            while (elapsedTime < holdTime)
            {
                if (!UnityEngine.Input.GetKey(KeyCode.E)) // ✅ Explicitly referencing UnityEngine.Input
                {
                    DebugLogger.LogMessage("[MagicMod] Placement canceled! Key released early.");
                    yield break; // **Cancel placement if key is released**
                }

                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsedTime); // **Update UI Timer**
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            DebugLogger.LogMessage($"[MagicMod] Item {itemName} placed successfully!");

            // **Manually Call Private `SetVisualItem`**
            typeof(ItemStand).GetMethod("SetVisualItem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(itemStand, new object[] { itemName, variant, quality });

            ItemStandTimer.HidePlacementTimer(); // **Hide Timer UI**
        }
    }
}