using UnityEngine;

namespace MagicMod
{
    public static class HotbarHelper
    {
        private static KeyCode[] hotbarKeys = 
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8
        };

        private static KeyCode lastPressedKey = KeyCode.None;

        // ✅ Detect if a hotbar key is currently being held
        public static bool IsHotbarKeyHeld()
        {
            foreach (var key in hotbarKeys)
            {
                if (Input.GetKey(key))
                {
                    lastPressedKey = key;
                    return true;
                }
            }
            return false;
        }

        // ✅ Detect when the key was **just released**
        public static bool WasHotbarKeyReleased()
        {
            if (lastPressedKey != KeyCode.None && Input.GetKeyUp(lastPressedKey))
            {
                lastPressedKey = KeyCode.None;
                return true;
            }
            return false;
        }
    }
}