using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace MagicMod
{
    /// <summary>
    /// This patch covers two things:
    /// 1) Forces "CanAttach" to always return true, so you don't get the "You cannot use X on Item Stand" error.
    /// 2) Delays "UseItem" by 3.8s, requiring you to hold a hotbar key to place items (no "E" key needed).
    /// </summary>

    // --------------------------------------------------
    //  PATCH #1: Always allow attaching the item
    //            (bypasses "Can't attach" checks).
    // --------------------------------------------------
    [HarmonyPatch(typeof(ItemStand), "CanAttach", new[] { typeof(ItemDrop.ItemData) })]
    public static class ItemStand_CanAttach_Patch
    {
        [HarmonyPrefix]
        public static bool CanAttach_Prefix(ref bool __result)
        {
            // Always allow. This prevents "You cannot use X on Item Stand."
            __result = true;
            return false; // Skip the original logic
        }
    }

    // --------------------------------------------------
    //  PATCH #2: Delay "UseItem" so it doesn't mount
    //            instantly. Requires holding hotbar key
    //            for 3.8 seconds.
    // --------------------------------------------------
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UseItem))]
    public static class ItemStand_UseItem_Patch
    {
        // If we're in the middle of a delayed usage
        private static bool isInDelayedUse = false;

        // If we call the original ourselves, we need to ignore our prefix once
        private static bool ignorePrefix = false;

        // Track the running coroutine
        private static Coroutine currentCoroutine = null;

        /// <summary>
        /// Prefix on "ItemStand.UseItem"
        /// Returns FALSE to block vanilla instantly. Then we do a coroutine
        /// that waits 3.8s of holding the hotbar key. After that, we manually
        /// call the real UseItem with "ignorePrefix" so we don’t re-block.
        /// </summary>
        [HarmonyPrefix]
        public static bool UseItem_Prefix(ItemStand __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            // If we're the ones calling UseItem via reflection, allow vanilla.
            if (ignorePrefix) return true;

            // If the stand already has an item and the user is trying to remove it (item==null),
            // or the user is trying to do something else, let vanilla handle it.
            // But: The user might want to remove an item that is already on the stand.
            // The game passes "UseItem" with item==null to remove. If item==null, let vanilla do it.
            if (item == null)
            {
                return true;
            }

            // Check if a hotbar key is currently held.
            bool isKeyHeld = HotbarHelper.IsHotbarKeyHeld();
            if (!isKeyHeld)
            {
                // Quietly block usage (don’t place instantly).
                __result = false;
                return false;
            }

            // If we're already in the middle of a delayed usage, skip starting another
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

            // Stop any previous coroutine, just in case
            if (currentCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(currentCoroutine);
            }

            // Launch the coroutine that waits for 3.8 seconds of holding
            currentCoroutine = CoroutineHandler.Instance.StartCoroutine(
                DelayedUseItem(__instance, user, item)
            );

            // Return false to block the normal usage right now
            __result = false;
            return false;
        }

        /// <summary>
        /// The coroutine that waits 3.8s. If the user is still holding
        /// the hotbar key at the end, we call the real UseItem.
        /// </summary>
        private static IEnumerator DelayedUseItem(ItemStand stand, Humanoid user, ItemDrop.ItemData item)
        {
            float holdTime = 3.8f;
            float elapsedTime = 0f;

            // Show the timer UI
            ItemStandTimer.ShowPlacementTimer();

            while (elapsedTime < holdTime)
            {
                // If the hotbar key is released early, cancel
                if (HotbarHelper.WasHotbarKeyReleased())
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Placement canceled! Key released early.");
                    ItemStandTimer.HidePlacementTimer();
                    isInDelayedUse = false;
                    yield break;
                }

                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Time’s up, place the item
            DebugLogger.LogMessage($"[MagicMod] ✅ Timer finished! Placing item: {item.m_shared?.m_name}");
            ItemStandTimer.HidePlacementTimer();

            // Temporarily allow the real "UseItem" to run
            ignorePrefix = true;
            try
            {
                MethodInfo originalUseItem = AccessTools.Method(typeof(ItemStand), nameof(ItemStand.UseItem));
                if (originalUseItem != null)
                {
                    // This calls the real UseItem so the item is finally attached
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