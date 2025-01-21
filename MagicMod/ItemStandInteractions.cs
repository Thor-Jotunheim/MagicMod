using HarmonyLib;
using UnityEngine;
using System.Collections;

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
            static bool Prefix(ItemStand __instance, Humanoid user, bool hold, bool alt, ref bool __result)
            {
                // If the player is holding the item and trying to place it
                if (!hold && !__instance.HaveAttachment() && __instance.m_supportedItems.Count == 1)
                {
                    ItemDrop.ItemData item = user.GetInventory().GetItem(__instance.m_supportedItems[0].m_itemData.m_shared.m_name);
                    if (item != null)
                    {
                        // Call the coroutine for placing the item (this will delay the placement for 3.8 seconds)
                        __instance.StartCoroutine(PlaceItemCoroutine(__instance, item, user));
                        __result = false;  // Prevent the default placement logic while waiting
                        return false;  // Skip the original method
                    }
                    user.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem");
                    __result = false;
                    return false;
                }

                // If the player is picking up the item
                if (__instance.HaveAttachment() && __instance.m_canBeRemoved)
                {
                    // Call the coroutine for picking up the item (this will delay the pickup for 7.5 seconds)
                    __instance.StartCoroutine(PickupItemCoroutine(__instance, user));
                    __result = false;  // Prevent the default pickup logic while waiting
                    return false;  // Skip the original method
                }

                // Allow the default interaction logic if no item is being placed or picked up
                return true;
            }
        }

        // Coroutine for placing item (takes 3.8 seconds)
        public static IEnumerator PlaceItemCoroutine(ItemStand itemStand, ItemDrop.ItemData item, Humanoid user)
        {
            yield return new WaitForSeconds(3.8f);  // Wait for 3.8 seconds before placing the item

            // Perform the placement action using the interact method with the correct Humanoid
            itemStand.Interact(user, false, false);  // Pass the player’s humanoid to the interact method
            Debug.Log("[MagicMod] Item placed on stand.");
        }

        // Coroutine for picking up item (takes 7.5 seconds)
        public static IEnumerator PickupItemCoroutine(ItemStand itemStand, Humanoid user)
        {
            yield return new WaitForSeconds(7.5f);  // Wait for 7.5 seconds before picking the item

            // Perform the pickup action using the interact method with the correct Humanoid
            itemStand.Interact(user, false, true);  // Pass the player’s humanoid to the interact method
            Debug.Log("[MagicMod] Item picked up from stand.");
        }
    }
}