using System.Collections;
using UnityEngine;

namespace MagicMod
{
    public class ItemStandInteractions : MonoBehaviour
    {
        // Coroutine for placing item (takes 3.8 seconds)
        public IEnumerator PlaceItemCoroutine(ItemStand itemStand, ItemDrop item)
        {
            yield return new WaitForSeconds(3.8f);  // Wait for 3.8 seconds before placing the item

            // Now perform the placement action
            itemStand.PlaceItem(item);  // Assuming this is how the item is placed on the stand
            UnityEngine.Debug.Log("[MagicMod] Item placed on stand.");
        }

        // Coroutine for picking up item (takes 7.5 seconds)
        public IEnumerator PickupItemCoroutine(ItemStand itemStand)
        {
            yield return new WaitForSeconds(7.5f);  // Wait for 7.5 seconds before picking the item

            // Now perform the pickup action
            itemStand.PickUpItem();  // Assuming this is how the item is picked up from the stand
            UnityEngine.Debug.Log("[MagicMod] Item picked up from stand.");
        }
    }
}