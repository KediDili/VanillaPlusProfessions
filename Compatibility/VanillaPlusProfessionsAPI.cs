using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Talents.UI;
using StardewModdingAPI;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Compatibility
{
    public class VanillaPlusProfessionsAPI : IVanillaPlusProfessions
    {
        internal Dictionary<string, SkillTree> CustomTalentTrees = new();
        internal List<CustomMonsterData> CustomMonsters = new();

        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, List<Talent> talents, Texture2D treeTexture, Rectangle sourceRect, int bundleID = -1, Color? tintColor = null)
        {
            string[] skills = ModEntry.SpaceCoreAPI.Value.GetCustomSkills();
            if (skills.Contains(skillID))
            {
                if (bundleID is <= (-1) or > 6)
                {
                    if (!tintColor.HasValue)
                    {
                        ModEntry.ModMonitor.Log("SpaceCore-registered skill with the ID of " + skillID + " has chosen an invalid color option. They haven't specified neither of bundleID and tintColor. Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);
                        return;
                    }
                }
                else
                {
                    if (tintColor.HasValue)
                    {
                        ModEntry.ModMonitor.Log("SpaceCore-registered skill with the ID of " + skillID + " has chosen an invalid color option. They can't specify both bundleID and tintColor at once. Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);
                        return;
                    }
                }
                if (true)
                {
                    //wtf was i gonna do here
                }
                CustomTalentTrees.Add(skillID, new(skillID, displayTitle.Invoke(), treeTexture, talents, sourceRect, bundleID, tintColor));
            }
            else
            {
                ModEntry.ModMonitor.Log("There is no such SpaceCore-registered skill with the ID of " + skillID + ". Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);                
            }
        }

        public IEnumerable<string> GetProfessionsForPlayer(Farmer who = null)
        {
            if (who is null)
            {
                who = Game1.player;
            }
            List<string> professions = new();
            if (!Context.IsWorldReady)
            {
                return professions;
            }
            foreach (var item in ModEntry.Professions)
            {
                if (CoreUtility.CurrentPlayerHasProfession(item.Key, useThisInstead: who, ignoreMode: true))
                {
                    professions.Add(item.Key);
                }
            };
            return professions;
        }
        public void RegisterCustomMonster(Type monsterType, bool isSlimy, IVanillaPlusProfessions.MonsterType type)
        {
            var newData = new CustomMonsterData()
            {
                Type = monsterType,
                MonsterType = type,
                isSlimy = isSlimy,
            };
            CustomMonsters.Add(newData);
        }

        public bool MasteryCaveChanges 
        {
            get
            {
                return ModEntry.ModConfig.Value.MasteryCaveChanges;
            }    
        }
        public bool ProfessionsOnly
        {
            get
            {
                return ModEntry.ModConfig.Value.ProfessionsOnly;
            }
        }

        public bool ColorBlindnessChanges
        {
            get
            {
                return ModEntry.ModConfig.Value.ColorBlindnessChanges;
            }
        }
    }
}
