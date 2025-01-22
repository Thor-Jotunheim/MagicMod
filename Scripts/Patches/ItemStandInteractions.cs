/*
using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace MagicMod
{
    public class ItemStandInteractions : MonoBehaviour
    {
        // Singleton instance for ItemStandInteractions
        public static ItemStandInteractions Instance { get; private set; }

        void Awake()
        {
            // Ensure this instance is the one being used
            Instance = this;
        }

        // Harmony patch for Item Stand placement (placing an item takes 3.8 seconds)
        [HarmonyPatch(typeof(ItemStand), "Interact")]
        public class ItemStandInteractPatch
        {
            static bool Prefix(ItemStand __instance, Humanoid user, bool hold, bool alt, ref bool __result)
            {
                // Log the state of the ItemStand and User
                Debug.Log("[MagicMod] ItemStand Interact method called.");
                Debug.Log($"[MagicMod] ItemStand attachment status: {__instance.HaveAttachment()}");
                Debug.Log($"[MagicMod] Number of supported items: {__instance.m_supportedItems.Count}");
                
                // Log the user and item information
                Debug.Log($"[MagicMod] User: {user.name}");
                Debug.Log($"[MagicMod] Is user holding: {hold}, alt: {alt}");

                // If the player is holding the item and trying to place it
                if (!hold && !__instance.HaveAttachment() && __instance.m_supportedItems.Count == 1)
                {
                    // Get the item to place
                    ItemDrop.ItemData item = user.GetInventory().GetItem(__instance.m_supportedItems[0].m_itemData.m_shared.m_name);
                    if (item != null)
                    {
                        // Log the item to be placed
                        Debug.Log($"[MagicMod] Found item: {item.m_shared.m_name}. Placing on the stand.");
                        
                        // Call the coroutine for placing the item (this will delay the placement for 3.8 seconds)
                        __instance.StartCoroutine(ItemStandInteractions.Instance.PlaceItemCoroutine(__instance, item));
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
                    // Log when the item is being picked up
                    Debug.Log("[MagicMod] Picking up the item from the stand.");

                    // Call the coroutine for picking up the item (this will delay the pickup for 7.5 seconds)
                    __instance.StartCoroutine(ItemStandInteractions.Instance.PickupItemCoroutine(__instance));
                    __result = false;  // Prevent the default pickup logic while waiting
                    return false;  // Skip the original method
                }

                // Allow the default interaction logic if no item is being placed or picked up
                return true;
            }
        }

        // Coroutine for placing item (takes 3.8 seconds)
        public IEnumerator PlaceItemCoroutine(ItemStand itemStand, ItemDrop.ItemData item)
        {
            Debug.Log("[MagicMod] Placing item with delay...");
            yield return new WaitForSeconds(3.8f);  // Wait for 3.8 seconds before placing the item

            // Now perform the placement action (this could involve custom logic to attach the item to the stand)
            AttachItem(itemStand, item);
            Debug.Log("[MagicMod] Item placed on stand.");
        }

        // Custom method to attach an item to the stand (replace with actual logic for your mod)
        private void AttachItem(ItemStand itemStand, ItemDrop.ItemData item)
        {
            // Custom logic to attach the item to the item stand (based on the mod setup)
            Debug.Log($"[MagicMod] Attaching item: {item.m_shared.m_name} to the stand.");
        }

        // Coroutine for picking up item (takes 7.5 seconds)
        public IEnumerator PickupItemCoroutine(ItemStand itemStand)
        {
            Debug.Log("[MagicMod] Starting item pickup with delay...");
            yield return new WaitForSeconds(7.5f);  // Wait for 7.5 seconds before picking the item

            // Check if an item is attached to the stand
            string attachedItem = itemStand.GetAttachedItem();
            if (!string.IsNullOrEmpty(attachedItem))
            {
                // Log the item to be picked up
                Debug.Log($"[MagicMod] Attached item: {attachedItem}. Proceeding to pick it up.");
                
                // If there is an attached item, we will call the existing pickup method
                PickupItem(itemStand);
                Debug.Log("[MagicMod] Item picked up from stand.");
            }
            else
            {
                Debug.LogWarning("[MagicMod] No item to pick up.");
            }
        }

        // Method to pick up the item (uses the existing method in the game)
        private void PickupItem(ItemStand itemStand)
        {
            // Check if there's an item attached
            if (itemStand.HaveAttachment())
            {
                // Log the removal action
                Debug.Log("[MagicMod] Removing item from the stand.");
                itemStand.DestroyAttachment();  // Remove the attached item
                Debug.Log("[MagicMod] Item removed from the stand.");
            }
            else
            {
                Debug.LogWarning("[MagicMod] No item to remove from the stand.");
            }
        }

        // Initialize item stand interactions (apply the Harmony patches)
        public void InitializeItemStandInteractions(Harmony harmony)
        {
            // Apply all Harmony patches for item stand interactions
            harmony.PatchAll();
        }
    }
}
*/