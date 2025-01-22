using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MagicMod
{
    /*
      -----------------------------------------------------------
      PATCH #1: Suppress "You can't use X" messages via Humanoid patches.
                We patch all known Humanoid.Message(...) overloads.
      -----------------------------------------------------------
    */
    public static class HumanoidMessagePatches
    {
        // Overload #1: (MessageHud.MessageType type, string msg)
        [HarmonyPatch(typeof(Humanoid), "Message", 
            new System.Type[] { typeof(MessageHud.MessageType), typeof(string) })]
        [HarmonyPrefix]
        public static bool Message_Overload0_Prefix(MessageHud.MessageType type, string msg)
        {
            return ShouldAllowMessage(msg);
        }

        // Overload #2: (MessageHud.MessageType type, string msg, float time, Sprite icon)
        [HarmonyPatch(typeof(Humanoid), "Message",
            new System.Type[] { typeof(MessageHud.MessageType), typeof(string), typeof(float), typeof(Sprite) })]
        [HarmonyPrefix]
        public static bool Message_Overload1_Prefix(MessageHud.MessageType type, string msg, float time, Sprite icon)
        {
            return ShouldAllowMessage(msg);
        }

        // Overload #3: (MessageHud.MessageType type, string msg, int amount, Sprite icon)
        [HarmonyPatch(typeof(Humanoid), "Message",
            new System.Type[] { typeof(MessageHud.MessageType), typeof(string), typeof(int), typeof(Sprite) })]
        [HarmonyPrefix]
        public static bool Message_Overload2_Prefix(MessageHud.MessageType type, string msg, int amount, Sprite icon)
        {
            return ShouldAllowMessage(msg);
        }

        private static bool ShouldAllowMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return true;

            // If the message references "DragonEgg" or "cantattach", skip it entirely
            if (msg.Contains("DragonEgg") || msg.Contains("$piece_itemstand_cantattach"))
            {
                DebugLogger.LogMessage($"[MagicMod] Suppressed message: \"{msg}\"");
                return false; // message suppressed
            }
            return true;
        }
    }

    /*
      -----------------------------------------------------------
      PATCH #2: Force ItemStand to treat $item_dragonegg as a supported item,
                so the game doesn't force remove it behind our backs.
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.IsUnsupported), new System.Type[] { typeof(ItemDrop.ItemData) })]
    public static class ItemStand_IsUnsupported_Patch
    {
        [HarmonyPrefix]
        public static bool IsUnsupported_Prefix(ItemDrop.ItemData item, ref bool __result)
        {
            // If it's the DragonEgg, always say "not unsupported" => false
            if (item != null && item.m_shared != null && item.m_shared.m_name == "$item_dragonegg")
            {
                __result = false;
                return false; // skip original method
            }
            return true; // run the game’s normal code for other items
        }
    }

    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.IsSupported), new System.Type[] { typeof(ItemDrop.ItemData) })]
    public static class ItemStand_IsSupported_Patch
    {
        [HarmonyPrefix]
        public static bool IsSupported_Prefix(ItemDrop.ItemData item, ref bool __result)
        {
            // If it's the DragonEgg, always say "yes, it's supported"
            if (item != null && item.m_shared != null && item.m_shared.m_name == "$item_dragonegg")
            {
                __result = true;
                return false; // skip original method
            }
            return true; // run normal for other items
        }
    }

    /*
      -----------------------------------------------------------
      PATCH #3: UseItem => 3.8s hotbar hold to place Egg + start bomb countdown
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UseItem))]
    public static class ItemStand_UseItem_Patch
    {
        private static bool isInDelayedUse = false;
        private static bool ignorePrefix = false;
        private static Coroutine currentCoroutine = null;

        private static bool bombIsActive = false;
        private static Coroutine bombCoroutine = null;

        [HarmonyPrefix]
        public static bool UseItem_Prefix(ItemStand __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            if (ignorePrefix) return true;
            if (item == null) return true;
            if (__instance.HaveAttachment()) return true;
            if (item.m_shared.m_name != "$item_dragonegg")
            {
                __result = false;
                return false;
            }

            bool isKeyHeld = HotbarHelper.IsHotbarKeyHeld();
            if (!isKeyHeld)
            {
                __result = false;
                return false;
            }

            if (isInDelayedUse)
            {
                DebugLogger.LogMessage("[MagicMod] ⚠️ Already placing an item, skipping.");
                __result = false;
                return false;
            }

            isInDelayedUse = true;
            DebugLogger.LogMessage($"[MagicMod] ⏳ Starting placement countdown for: {item.m_shared?.m_name}");

            if (currentCoroutine != null)
            {
                CoroutineHandler.Instance.StopCoroutine(currentCoroutine);
            }
            currentCoroutine = CoroutineHandler.Instance.StartCoroutine(DelayedPlant(__instance, user, item));

            __result = false;
            return false;
        }

        private static IEnumerator DelayedPlant(ItemStand stand, Humanoid user, ItemDrop.ItemData item)
        {
            float holdTime = 3.8f;
            float elapsed = 0f;

            ItemStandTimer.ShowPlacementTimer();

            while (elapsed < holdTime)
            {
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

            DebugLogger.LogMessage($"[MagicMod] ✅ Egg planted: {item.m_shared?.m_name}");
            ItemStandTimer.HidePlacementTimer();
            isInDelayedUse = false;

            ignorePrefix = true;
            try
            {
                var originalUseItem = AccessTools.Method(typeof(ItemStand), nameof(ItemStand.UseItem));
                originalUseItem?.Invoke(stand, new object[] { user, item });
            }
            finally
            {
                ignorePrefix = false;
            }

            yield return new WaitForSeconds(0.1f);

            if (!bombIsActive)
            {
                bombIsActive = true;
                bombCoroutine = CoroutineHandler.Instance.StartCoroutine(BombCountdown(stand, 45f));
            }
        }

        private static IEnumerator BombCountdown(ItemStand stand, float totalTime)
        {
            DebugLogger.LogMessage($"[MagicMod] BombCountdown started, totalTime={totalTime}");

            EventZone.BroadcastToZonePlayers("The bomb has been planted!");
            List<Player> zonePlayers = EventZone.GetPlayersInZone();

            DebugLogger.LogMessage($"[MagicMod] zonePlayers count = {zonePlayers.Count}");

            foreach (Player p in zonePlayers)
            {
                DebugLogger.LogMessage($"[MagicMod] Notifying player: {p.GetPlayerName()}");
                if (p == Player.m_localPlayer)
                {
                    DebugLogger.LogMessage("[MagicMod] => ShowBombTimer(45)");
                    BombTimerUI.ShowBombTimer(totalTime);
                }
            }

            float remaining = totalTime;
            while (remaining > 0f)
            {
                yield return new WaitForSeconds(1f);
                remaining--;

                string attachedItem = stand.GetAttachedItem();
                DebugLogger.LogMessage($"[MagicMod] Bomb tick => remaining={remaining}, attachedItem={attachedItem}");

                // 🚀 **Fix: Properly check if the item is still on the stand**
                if (string.IsNullOrEmpty(attachedItem))
                {
                    DebugLogger.LogMessage("[MagicMod] Bomb defused! Countdown stopped.");
                    
                    // Hide the UI properly when defused
                    foreach (Player p in zonePlayers)
                    {
                        if (p == Player.m_localPlayer)
                        {
                            BombTimerUI.HideBombTimer();
                        }
                    }
                    bombIsActive = false;
                    yield break;
                }

                foreach (Player p in zonePlayers)
                {
                    if (p == Player.m_localPlayer)
                    {
                        BombTimerUI.UpdateBombTimer(remaining);
                    }
                }
            }

            DebugLogger.LogMessage("[MagicMod] Bomb exploded => Terrorists Win");
            EventZone.BroadcastToZonePlayers("Terrorists Win!");

            bombCoroutine = null;
            bombIsActive = false;
        }
    }

    /*
      -----------------------------------------------------------
      PATCH #4: Interact => 10s hold to remove the DragonEgg.
                If removed => "Counter-Terrorist Win."
                If attacked or user stops holding => canceled.
      -----------------------------------------------------------
    */
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
    public static class ItemStand_Interact_Patch
    {
        private static bool isRemoving = false;
        private static Coroutine removeCoroutine = null;
        private static float lastInteractTime = 0f;

        [HarmonyPrefix]
        public static bool Interact_Prefix(ItemStand __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            // If no item or can't remove => vanilla
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

            // If user tapped E => block immediate removal
            if (!hold)
            {
                __result = false;
                return false;
            }

            // If we’re already removing, refresh lastInteractTime
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

            bool gotHit = false;
            System.Action<float, Character> onDamaged = (float dmg, Character attacker) =>
            {
                gotHit = true;
            };

            if (player != null)
            {
                player.m_onDamaged -= onDamaged;
                player.m_onDamaged += onDamaged;
            }

            while (elapsed < removeTime)
            {
                yield return null;

                // If stand or item is missing => canceled
                if (!stand || !stand.HaveAttachment())
                {
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                // If user let go => no new Interact(hold==true) => time since last >= 0.2
                if (Time.time - lastInteractTime > 0.2f)
                {
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                // If got hit => canceled
                if (gotHit)
                {
                    DebugLogger.LogMessage("[MagicMod] ❌ Defuse canceled! Player got hit.");
                    CancelDefuseUI(player, onDamaged);
                    yield break;
                }

                elapsed += Time.deltaTime;

                if (player && player == Player.m_localPlayer)
                {
                    BombTimerUI.UpdateDefuseTimer(removeTime - elapsed);
                }
            }

            // Done defusing
            CancelDefuseUI(player, onDamaged);

            // "Counter-Terrorist Win"
            DebugLogger.LogMessage("[MagicMod] => Counter-Terrorist Win (egg removed)");
            EventZone.BroadcastToZonePlayers("Counter-Terrorist Win");

            // Actually remove egg
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
                player.m_onDamaged -= onDamaged;
            }

            isRemoving = false;
        }
    }
}