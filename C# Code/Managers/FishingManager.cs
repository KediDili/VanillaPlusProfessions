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
        
        readonly static string PatcherName = nameof(FishingManager);
        readonly static System.Type PatcherType = typeof(FishingManager);
        
        public void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.getFish",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getFish)),
                postfix: new HarmonyMethod(PatcherType, nameof(getFish_Postfix))
            );
        }

        private static int FailAmount = -1;

        public static void getFish_Postfix(ref Item __result, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, GameLocation __instance)
        {
            try
            {
                if (!string.IsNullOrEmpty(bait) && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Oceanologist, useThisInstead: who))
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
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.getFish", "postfixed", true);
            }
        }
    }
}
