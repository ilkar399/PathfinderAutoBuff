using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathfinderAutoBuff.Utility.SettingsWrapper;

namespace PathfinderAutoBuff.Utility
{
    public static class VersionCompatibility
    {
        public static void SettingsCompatibility()
        {
            //0.16 -> 0.20 favorite queues compatibility
            if (FavoriteQueues.Count > 0)
            {
                // collect all existing values of the property bar
                var existingValues = new HashSet<string>(from x in FavoriteQueues2 select x);
                // pick items that have a property bar that doesn't exist yet
                var newItems = FavoriteQueues.Values.Where(x => !existingValues.Contains(x));
                // Add them
                foreach (string item in newItems)
                {
                    if (item.Length> 0)
                        FavoriteQueues2.Add(item);
                }
                FavoriteQueues.Clear();
            }
        }
    }
}
