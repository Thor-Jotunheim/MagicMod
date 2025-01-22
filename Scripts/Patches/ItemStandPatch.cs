using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace MagicMod
{
    /*
      -----------------------------------------------------------
      PATCH #1: Patch "CanAttach" so:
        - If item is $item_dragonegg, return true (no "You can't use X" message)
        - Otherwise, return false silently, so no error message displays.
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), "CanAttach", new[] { typeof(ItemDrop.ItemData) })]
    public static class ItemStand_CanAttach_Patch
    {
        [HarmonyPrefix]
        public static bool CanAttach_Prefix(ItemDrop.ItemData item, ref bool __result)
        {
            if (item == null || item.m_shared == null)
            {
                __result = false;
                return false; // skip original logic
            }

            // Only allow the DragonEgg
            if (item.m_shared.m_name == "$item_dragonegg")
            {
                // We allow it
                __result = true;
            }
            else
            {
                // Silently block all other items (no message)
                __result = false;
            }

            return false; // skip the original method entirely
        }
    }

    /*
      -----------------------------------------------------------
      PATCH #2: Override "UseItem" so we:
        - require a 3.8s hotbar hold to place the Egg
        - start a 45s bomb timer for all players in 150m
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UseItem))]
    public static class ItemStand_UseItem_Patch
    {
        private static bool isInDelayedUse = false; // Are we in the 3.8s hold?
        private static bool ignorePrefix = false;   // Skip our patch if we're invoking UseItem ourselves
        private static Coroutine currentCoroutine = null;

        // Bomb logic
        private static bool bombIsActive = false;   // Has the bomb been planted and not yet exploded/defused?
        private static Coroutine bombCoroutine = null;

        [HarmonyPrefix]
        public static bool UseItem_Prefix(ItemStand __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            // If we are calling the real UseItem from reflection, let vanilla run
            if (ignorePrefix) return true;

            // If item==null => removing/unmounting
            // Let the Interact patch handle the 10s defuse logic
            if (item == null) return true;

            // If there's already an item => skip
            if (__instance.HaveAttachment()) return true;

            // If the item isn't the DragonEgg, we block quietly
            if (item.m_shared.m_name != "$item_dragonegg")
            {
                __result = false;
                return false;
            }

            // Check if the user is holding a hotbar key
            bool isKeyHeld = HotbarHelper.IsHotbarKeyHeld();
            if (!isKeyHeld)
            {
                __result = false;
                return false;
            }

            // If we’re already placing, skip
            if (isInDelayedUse)
            {
                DebugLogger.LogMessage("[MagicMod] ⚠️ Already placing an item, skipping.");
                __result = false;
                return false;
            }

            // Begin the 3.8s placement countdown
            isInDelayedUse = true;
            DebugLogger.LogMessage($"[MagicMod] Hotbar key held: {isKeyHeld}");
            DebugLogger.LogMessage($"[MagicMod] ⏳ Starting placement countdown for: {item.m_shared?.m_name}");

            // Stop any old coroutine
            if (currentCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(currentCoroutine);
            }
            currentCoroutine = CoroutineHandler.Instance.StartCoroutine(DelayedPlant(__instance, user, item));

            // Block vanilla for now
            __result = false;
            return false;
        }

        private static IEnumerator DelayedPlant(ItemStand stand, Humanoid user, ItemDrop.ItemData item)
        {
            float holdTime = 3.8f;
            float elapsed = 0f;

            // Show the 3.8s "placing" timer
            ItemStandTimer.ShowPlacementTimer();

            while (elapsed < holdTime)
            {
                // If user lets go of the hotbar key, cancel
                if (HotbarHelper.WasHotbarKeyReleased())
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Plant canceled! Key released early.");
                    ItemStandTimer.HidePlacementTimer();
                    isInDelayedUse = false;
                    yield break;
                }

                ItemStandTimer.UpdatePlacementTimer(holdTime - elapsed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Done planting
            DebugLogger.LogMessage($"[MagicMod] ✅ Egg planted: {item.m_shared?.m_name}");
            ItemStandTimer.HidePlacementTimer();
            isInDelayedUse = false;

            // Let vanilla place the Egg
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
            }

            // Start the bomb timer
            if (!bombIsActive)
            {
                bombIsActive = true;
                bombCoroutine = CoroutineHandler.Instance.StartCoroutine(BombCountdown(stand, 45f));
            }
        }

        /// <summary>
        /// Called after the Egg is successfully placed.
        /// We broadcast "The bomb has been planted" & show a shared 45s countdown UI
        /// to every local player within 150m. If it reaches 0 and Egg is still on stand => "Terrorists Win".
        /// </summary>
        private static IEnumerator BombCountdown(ItemStand stand, float totalTime)
        {
            BroadcastMessageToNearbyPlayers(stand.transform.position, 150f, "The bomb has been planted!");
            List<Player> inRangePlayers = GetNearbyPlayers(stand.transform.position, 150f);

            // Each local player in range gets a 45s UI
            foreach (Player p in inRangePlayers)
            {
                if (p == Player.m_localPlayer)
                {
                    BombTimerUI.ShowBombTimer(totalTime);
                }
            }

            float remaining = totalTime;
            while (remaining > 0f && stand.HaveAttachment())
            {
                yield return new WaitForSeconds(1f);
                remaining--;

                // Update each local player's UI
                foreach (Player p in inRangePlayers)
                {
                    if (p == Player.m_localPlayer)
                    {
                        BombTimerUI.UpdateBombTimer(remaining);
                    }
                }
            }

            // Hide the UI
            foreach (Player p in inRangePlayers)
            {
                if (p == Player.m_localPlayer)
                {
                    BombTimerUI.HideBombTimer();
                }
            }

            // If the Egg is still there => "Terrorists Win"
            if (stand.HaveAttachment())
            {
                BroadcastMessageToNearbyPlayers(stand.transform.position, 150f, "Terrorists Win!");
            }

            bombCoroutine = null;
            bombIsActive = false;
        }

        private static void BroadcastMessageToNearbyPlayers(Vector3 center, float radius, string msg)
        {
            var allPlayers = Player.GetAllPlayers();
            foreach (var p in allPlayers)
            {
                float distance = Vector3.Distance(p.transform.position, center);
                if (distance <= radius)
                {
                    p.Message(MessageHud.MessageType.Center, msg);
                }
            }
        }

        private static List<Player> GetNearbyPlayers(Vector3 center, float radius)
        {
            var results = new List<Player>();
            var allPlayers = Player.GetAllPlayers();
            foreach (var p in allPlayers)
            {
                float distance = Vector3.Distance(p.transform.position, center);
                if (distance <= radius)
                {
                    results.Add(p);
                }
            }
            return results;
        }
    }

    /*
      -----------------------------------------------------------
      PATCH #3: Interact => 10s hold to remove the DragonEgg.
                If removed => "Counter-Terrorist Win."
                If user is attacked or lets go of E => canceled.
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
    public static class ItemStand_Interact_Patch
    {
        private static bool isRemoving = false;
        private static Coroutine removeCoroutine = null;

        // Track the last time we saw "Interact" with hold==true
        private static float lastInteractTime = 0f;

        [HarmonyPrefix]
        public static bool Interact_Prefix(ItemStand __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            // If there's no item or can't be removed => run vanilla
            if (!__instance.HaveAttachment() || !__instance.m_canBeRemoved)
            {
                return true;
            }

            // Must be the Egg
            string attached = __instance.GetAttachedItem();
            if (!attached.Contains("DragonEgg"))
            {
                return true;
            }

            // If user just tapped E, block immediate removal
            if (!hold)
            {
                __result = false;
                return false;
            }

            // If we’re already removing, just update lastInteractTime
            if (isRemoving)
            {
                lastInteractTime = Time.time;
                __result = false;
                return false;
            }

            // Start the defuse countdown
            isRemoving = true;
            lastInteractTime = Time.time;

            if (removeCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(removeCoroutine);
            }
            removeCoroutine = CoroutineHandler.Instance.StartCoroutine(DefuseCountdown(__instance, user));

            __result = false;
            return false;
        }

        private static IEnumerator DefuseCountdown(ItemStand stand, Humanoid user)
        {
            float removeTime = 10f;
            float elapsed = 0f;

            Player player = user as Player;
            if (player && player == Player.m_localPlayer)
            {
                BombTimerUI.ShowDefuseTimer(removeTime);
            }

            // Subscribe to the player's damage event so if they're hit, we cancel
            bool gotHit = false;

            // NOTICE the signature now has (float dmg, Character attacker)
            System.Action<float, Character> onDamaged = (float dmg, Character attacker) =>
            {
                gotHit = true; // we’ll break out
            };

            // Hook into the player's "OnDamaged" if we can
            if (player != null)
            {
                // Unsubscribe first to avoid duplicates, then subscribe
                player.m_onDamaged -= onDamaged; 
                player.m_onDamaged += onDamaged;
            }

            while (elapsed < removeTime)
            {
                yield return null; // check every frame

                // Check if stand or item is missing => canceled
                if (!stand || !stand.HaveAttachment())
                {
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                // If the user has let go of E => the game won't call Interact with hold==true
                // for a new frame, so if Time.time - lastInteractTime > ~0.2f => user let go
                if (Time.time - lastInteractTime > 0.2f)
                {
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                // If the user got attacked, cancel
                if (gotHit)
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Defuse canceled! Player got hit.");
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                elapsed += Time.deltaTime;

                // Update UI for local defuser
                if (player && player == Player.m_localPlayer)
                {
                    BombTimerUI.UpdateDefuseTimer(removeTime - elapsed);
                }
            }

            // Completed the defuse
            CancelDefuseUI(player, onDamaged);

            // Broadcast message
            BroadcastMessageToNearbyPlayers(stand.transform.position, 150f, "Counter-Terrorist Win");

            // Actually remove egg by calling "DropItem"
            var nviewField = AccessTools.Field(typeof(ItemStand), "m_nview");
            var nview = nviewField.GetValue(stand) as ZNetView;
            nview?.InvokeRPC("DropItem", (long)0);

            isRemoving = false;
        }

        private static void CancelDefuseUI(Player player, System.Action<float, Character> onDamaged)
        {
            if (player && player == Player.m_localPlayer)
            {
                BombTimerUI.HideDefuseTimer();
            }

            if (player != null)
            {
                // Unsubscribe from damage event
                player.m_onDamaged -= onDamaged;
            }

            isRemoving = false;
        }

        private static void BroadcastMessageToNearbyPlayers(Vector3 center, float radius, string msg)
        {
            var allPlayers = Player.GetAllPlayers();
            foreach (var p in allPlayers)
            {
                float distance = Vector3.Distance(p.transform.position, center);
                if (distance <= radius)
                {
                    p.Message(MessageHud.MessageType.Center, msg);
                }
            }
        }
    }
}