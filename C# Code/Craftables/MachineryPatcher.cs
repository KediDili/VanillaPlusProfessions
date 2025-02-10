using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.BellsAndWhistles;
using HarmonyLib;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryPatcher
    {
        public static void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Bird), nameof(Bird.FlyToNewPoint)),
                prefix: new(AccessTools.Method(typeof(MachineryPatcher), nameof(FlyToNewPoint_Prefix)))
            );
        }
        public static bool FlyToNewPoint_Prefix(Bird __instance)
        {
            if (MachineryEventHandler.BirdsOnFeeders.Count > 0)
            {
                foreach (var item in MachineryEventHandler.BirdsOnFeeders)
                {
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        if (__instance == item.Value[i])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
