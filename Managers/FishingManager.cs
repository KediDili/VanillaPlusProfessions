using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Managers
{
    public class FishingManager : IProfessionManager
    {
        public int SkillValue => 1;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                    postfix: new HarmonyMethod(typeof(FishingManager), nameof(FishingManager.getFish_Postfix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingManager), nameof(GameLocation.getFish), "postfixing");
            }
        }

        private static int FailAmount = -1;

        public static void getFish_Postfix(ref Item __result, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, GameLocation __instance)
        {
            if (!string.IsNullOrEmpty(bait) && CoreUtility.CurrentPlayerHasProfession(38, useThisInstead: who))
            {
                if (FailAmount < 100 && (__result is null || __result?.HasContextTag("trash_item") == true))
                {
                    try
                    {
                        __result = (Item)AccessTools.Method(typeof(GameLocation), "getFish").Invoke(__instance, new object[] { millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile });
                    }
                    catch (System.Exception)
                    { }
                    FailAmount++;
                }
                else if (FailAmount >= 100)
                    FailAmount = -1;
            }
        }
    }
}
