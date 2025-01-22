using UnityEngine;
using System.Collections.Generic;

namespace MagicMod
{
    public static class EventZone
    {
        // Adjust these to set where your event zone is on the map.
        public static Vector3 Center = new Vector3(313f, 34f, -256f);
        public static float Radius = 150f;

        /// Is 'pos' inside the event zone radius?
        public static bool IsInZone(Vector3 pos)
        {
            return Vector3.Distance(pos, Center) <= Radius;
        }

        /// Get a list of all players currently inside the event zone.
        public static List<Player> GetPlayersInZone()
        {
            var all = Player.GetAllPlayers();
            var results = new List<Player>();
            foreach (var p in all)
            {
                if (IsInZone(p.transform.position))
                {
                    results.Add(p);
                }
            }
            return results;
        }

        /// Broadcast a message to all players in the event zone.
        public static void BroadcastToZonePlayers(string msg)
        {
            var all = Player.GetAllPlayers();
            foreach (var p in all)
            {
                if (IsInZone(p.transform.position))
                {
                    p.Message(MessageHud.MessageType.Center, msg);
                }
            }
        }
    }
}