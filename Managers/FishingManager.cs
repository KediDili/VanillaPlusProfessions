using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Managers
{
    public class FishingManager : IProfessionManager
    {
        public int SkillValue => 1;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                postfix: new HarmonyMethod(typeof(FishingManager), nameof(FishingManager.getFish_Postfix))
            );
        }

        private static int FailAmount = -1;

        public static void getFish_Postfix(ref Item __result, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, GameLocation __instance)
        {
            if (!string.IsNullOrEmpty(bait) && CoreUtility.CurrentPlayerHasProfession(38, who))
            {
                if (FailAmount < 3 && (__result is null || __result?.HasContextTag("trash_item") == true))
                {
                    ModEntry.Helper.Reflection.GetMethod(__instance, "getFish").Invoke(new object[] { millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile });
                    FailAmount++;
                }
                else if (FailAmount >= 3)
                    FailAmount = -1;
            }
        }
    }
}
