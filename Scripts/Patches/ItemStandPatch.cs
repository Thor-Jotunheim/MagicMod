using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace MagicMod
{
    [HarmonyPatch(typeof(ItemStand), "SetVisualItem")]
    public static class ItemStandPatch
    {
        private static bool isPlacingItem = false;
        private static bool ignorePrefix = false;
        private static Coroutine currentPlacementCoroutine = null;

        [HarmonyPrefix]
        public static bool BeforeSetVisualItem(ItemStand __instance, ref string itemName, int variant, int quality)
        {
            // If we're ignoring the prefix (i.e. calling this from PlaceItem), let the original run
            if (ignorePrefix) return true;

            // If the game calls SetVisualItem with an empty name or if logging is off, let the original run
            if (!DebugLogger.IsDebugEnabled || string.IsNullOrEmpty(itemName)) return true;

            // Check if the hotbar key is held
            bool isKeyHeld = HotbarHelper.IsHotbarKeyHeld();

            // If the key is NOT held, just block it quietly (no spammy log)
            if (!isKeyHeld)
            {
                // Return false to block immediate placement
                return false;
            }

            // Otherwise, now we log because the user is actively trying to place something
            DebugLogger.LogMessage($"[MagicMod] Hotbar key held: {isKeyHeld}");
            DebugLogger.LogMessage($"[MagicMod] 🔹 Attempting to place item: {itemName}");

            // Prevent multiple placement attempts
            if (isPlacingItem)
            {
                DebugLogger.LogMessage("[MagicMod] ⚠️ Already placing an item, skipping.");
                return false;
            }

            // Flag that we're starting our delayed placement process
            isPlacingItem = true;

            // Ensure only one coroutine is running
            if (currentPlacementCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(currentPlacementCoroutine);
            }
            currentPlacementCoroutine = CoroutineHandler.Instance.StartCoroutine(DelayedPlacement(__instance, itemName, variant, quality));

            // Skip the original call for now
            return false;
        }

        private static IEnumerator DelayedPlacement(ItemStand itemStand, string itemName, int variant, int quality)
        {
            float holdTime = 3.8f;
            float elapsedTime = 0f;

            DebugLogger.LogMessage("[MagicMod] ⏳ Placement countdown started...");
            ItemStandTimer.ShowPlacementTimer();

            while (elapsedTime < holdTime)
            {
                // If the user releases the key early, cancel
                if (HotbarHelper.WasHotbarKeyReleased())
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Placement canceled! Key released early.");
                    ItemStandTimer.HidePlacementTimer();
                    isPlacingItem = false;
                    yield break;
                }

                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            DebugLogger.LogMessage($"[MagicMod] ✅ Timer finished! Placing item: {itemName}");

            // Let the original method run by unblocking the prefix
            isPlacingItem = false;
            PlaceItem(itemStand, itemName, variant, quality);
        }

        private static void PlaceItem(ItemStand itemStand, string itemName, int variant, int quality)
        {
            // Temporarily allow original SetVisualItem by ignoring our prefix
            ignorePrefix = true;

            MethodInfo method = typeof(ItemStand).GetMethod("SetVisualItem", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(itemStand, new object[] { itemName, variant, quality });

                MethodInfo updateVisual = typeof(ItemStand).GetMethod("UpdateVisual", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (updateVisual != null)
                {
                    updateVisual.Invoke(itemStand, null);
                    DebugLogger.LogMessage("[MagicMod] ✅ Forced UpdateVisual to ensure correct item appearance.");
                }
            }

            ignorePrefix = false;

            ItemStandTimer.HidePlacementTimer();
        }
    }
}