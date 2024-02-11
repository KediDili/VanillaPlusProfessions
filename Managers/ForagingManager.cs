using HarmonyLib;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace VanillaPlusProfessions.Managers
{
    public class ForagingManager : IProfessionManager
    {
        public int SkillValue => 2;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Tree), "TryGetTapperOutput"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(ForagingManager), nameof(ForagingManager.TryGetTapperOutput_Prefix)))
            );
            
        }
        public static void TryGetTapperOutput_Prefix(ref float timeMultiplier)
        {
            if (CoreUtility.CurrentPlayerHasProfession(48))
            {
                timeMultiplier *= 3 / 4;
            }
        }        
    }
}
