using HarmonyLib;
using UnityEngine;

namespace MagicMod
{
    public class ItemStandInteractions : MonoBehaviour
    {
        // Initialize coroutines or any logic for item stand interactions
        public void InitializeItemStandInteractions(Harmony harmony)
        {
            harmony.PatchAll();
        }

        // Harmony patch for Item Stand placement (placing an item takes 3.8 seconds)
        [HarmonyPatch(typeof(ItemStand), "Interact")]
        public class ItemStandInteractPatch
        {
            static bool Prefix(ItemStand __instance, Player player, ref bool __result)
            {
                // If the item is being placed
                if (__instance.IsPlacingItem(player)) 
                {
                    __instance.StartCoroutine(PlaceItemCoroutine(__instance, player.GetPlacedItem()));
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Harmony patch for Item Stand pickup (picking up an item takes 7.5 seconds)
        [HarmonyPatch(typeof(ItemStand), "Interact")]
        public class ItemStandPickupPatch
        {
            static bool Prefix(ItemStand __instance, Player player, ref bool __result)
            {
                if (__instance.IsPickingUpItem(player))
                {
                    __instance.StartCoroutine(PickupItemCoroutine(__instance));
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        // Coroutine for placing item (takes 3.8 seconds)
        private static IEnumerator PlaceItemCoroutine(ItemStand itemStand, ItemDrop item)
        {
            yield return new WaitForSeconds(3.8f);
            itemStand.PlaceItem(item);
            Debug.Log("[MagicMod] Item placed on stand.");
        }

        // Coroutine for picking up item (takes 7.5 seconds)
        private static IEnumerator PickupItemCoroutine(ItemStand itemStand)
        {
            yield return new WaitForSeconds(7.5f);
            itemStand.PickUpItem();
            Debug.Log("[MagicMod] Item picked up from stand.");
        }
    }
}