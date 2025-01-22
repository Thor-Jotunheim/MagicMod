using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace MagicMod
{
    /// <summary>
    /// PATCH #1: Restrict attachment so ONLY DragonEgg can be placed.
    /// </summary>
    [HarmonyPatch(typeof(ItemStand), "CanAttach", new[] { typeof(ItemDrop.ItemData) })]
    public static class ItemStand_CanAttach_Patch
    {
        [HarmonyPrefix]
        public static bool CanAttach_Prefix(ItemDrop.ItemData item, ref bool __result)
        {
            // If the item is the DragonEgg (its internal shared name is "$item_dragonegg"),
            // allow attachment. Otherwise, disallow.
            if (item != null && item.m_shared != null && item.m_shared.m_name == "$item_dragonegg")
            {
                __result = true;
            }
            else
            {
                __result = false;
            }

            // Return false to skip the original logic, forcing our result.
            return false;
        }
    }

    /// <summary>
    /// PATCH #2: Delay "UseItem" by 3.8 seconds so it doesn't mount instantly.
    /// Only a hotbar key is required (no "E" key).
    /// </summary>
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UseItem))]
    public static class ItemStand_UseItem_Patch
    {
        // If we're in the middle of a delayed usage
        private static bool isInDelayedUse = false;

        // If we call the original ourselves, we need to ignore our prefix once
        private static bool ignorePrefix = false;

        // Track the running coroutine
        private static Coroutine currentCoroutine = null;

        [HarmonyPrefix]
        public static bool UseItem_Prefix(ItemStand __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            // If we're calling UseItem from inside our own patch, allow vanilla to run
            if (ignorePrefix) return true;

            // If item==null, user might be trying to remove an item. Let vanilla handle that.
            if (item == null) return true;

            // Check if a hotbar key is currently held
            bool isKeyHeld = HotbarHelper.IsHotbarKeyHeld();
            if (!isKeyHeld)
            {
                // Quietly block usage (no instant placement).
                __result = false;
                return false;
            }

            // If we're already mid-countdown, block extra calls
            if (isInDelayedUse)
            {
                DebugLogger.LogMessage("[MagicMod] ⚠️ Already placing an item, skipping.");
                __result = false;
                return false;
            }

            // Begin the delayed usage process
            isInDelayedUse = true;
            DebugLogger.LogMessage($"[MagicMod] Hotbar key held: {isKeyHeld}");
            DebugLogger.LogMessage($"[MagicMod] ⏳ Starting placement countdown for: {item.m_shared?.m_name}");

            // Stop any old coroutine first
            if (currentCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(currentCoroutine);
            }

            // Start the countdown
            currentCoroutine = CoroutineHandler.Instance.StartCoroutine(DelayedUseItem(__instance, user, item));

            // Block the vanilla call until the timer completes
            __result = false;
            return false;
        }

        private static IEnumerator DelayedUseItem(ItemStand stand, Humanoid user, ItemDrop.ItemData item)
        {
            float holdTime = 3.8f;
            float elapsedTime = 0f;

            // Show timer UI
            ItemStandTimer.ShowPlacementTimer();

            while (elapsedTime < holdTime)
            {
                // If hotbar key is released early, cancel
                if (HotbarHelper.WasHotbarKeyReleased())
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Placement canceled! Key released early.");
                    ItemStandTimer.HidePlacementTimer();
                    isInDelayedUse = false;
                    yield break;
                }

                // Update the timer display
                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Finished the hold duration, place the item
            DebugLogger.LogMessage($"[MagicMod] ✅ Timer finished! Placing item: {item.m_shared?.m_name}");
            ItemStandTimer.HidePlacementTimer();

            // Temporarily allow the real UseItem to run
            ignorePrefix = true;
            try
            {
                MethodInfo originalUseItem = AccessTools.Method(typeof(ItemStand), nameof(ItemStand.UseItem));
                if (originalUseItem != null)
                {
                    originalUseItem.Invoke(stand, new object[] { user, item });
                }
            }
            finally
            {
                ignorePrefix = false;
                isInDelayedUse = false;
            }
        }
    }
}