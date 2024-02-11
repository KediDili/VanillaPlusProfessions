using StardewValley;
using StardewModdingAPI;
using System.Linq;
using System.Collections.Generic;
using VanillaPlusProfessions.Talents;

namespace VanillaPlusProfessions
{
    public static class CoreUtility
    {
        public static bool IsOverlayValid() => Game1.player.FarmingLevel > 10 || Game1.player.FishingLevel > 10 || Game1.player.ForagingLevel > 10 || Game1.player.MiningLevel > 10 || Game1.player.CombatLevel > 10;

        public static bool AnyPlayerHasProfession(int profession)
        {
            if (!Context.IsWorldReady)
                return ModEntry.ModConfig.Value.DeveloperOrTestingMode;

            var team = Game1.getOnlineFarmers();
            foreach (var farmer in team)
                if (farmer.isActive() && farmer.professions.Contains(profession))
                    return true;
            return ModEntry.ModConfig.Value.DeveloperOrTestingMode;
        }

        internal static void remove(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                Utility.ForEachBuilding(Building =>
                {
                    Building.modData.Remove(ModEntry.Key_FishRewardOrQuestDayLeft);
                    Building.modData.Remove(ModEntry.Key_IsSlimeHutchWatered);
                    return true;
                });
                foreach (var farmer in Game1.getAllFarmers())
                {
                    foreach (var item in ModEntry.Professions.Values)
                        farmer.professions.Remove(item.ID);
                }
            }
            else
            {
                ModEntry.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static void recalculate(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                int number = 0;
                number += Game1.player.farmingLevel.Value;
                number += Game1.player.fishingLevel.Value;
                number += Game1.player.miningLevel.Value;
                number += Game1.player.combatLevel.Value;
                number += Game1.player.foragingLevel.Value;
                number += Game1.player.achievements.Count;
                TalentCore.TalentPointCount.Value += number;
            }
            else
            {
                ModEntry.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static bool DoesDictHaveID(string value, out KeyValuePair<string, Profession> result)
        {
            var list = from profession in ModEntry.Professions
                       where profession.Value.ID.ToString() == value
                       select profession;
            result = list.FirstOrDefault();

            return list.Any();
        }

        public static bool CurrentPlayerHasProfession(int profession, Farmer useThisInstead = null, bool ignoreMode = false)
        {
            useThisInstead ??= Game1.player;

            if (useThisInstead is null)
                return false;

            return useThisInstead.professions.Contains(profession) is true || (ModEntry.ModConfig.Value.DeveloperOrTestingMode && !ignoreMode);
        }        
    }
}
