using HarmonyLib;
using StardewValley;
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

        }
    }
}
