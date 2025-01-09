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
using StardewValley.Menus;

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
                CustomTalentTrees.TryAdd(skillID, new(skillID, displayTitle.Invoke(), treeTexture, talents, sourceRect, bundleID, tintColor));
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

        public string GetPathForAsset(string key)
        {
            if (ContentEditor.ContentPaths.ContainsKey(key))
                return ContentEditor.ContentPaths[key];
            return null;
        }
        public List<int> GetNextTierProfessions(Farmer who, int level, string skill)
        {
            List<int> toReturn = new();
            if (level == 15)
            {
                foreach (var item in ModEntry.Professions)
                {
                    if (Game1.player.professions.Contains(item.Value.Requires) && DisplayHandler.AreSkillConditionsMet(skill, -1) == item.Value.Skill.ToString())
                        toReturn.Add(item.Value.ID);
                }
            }
            else if (level == 20)
            {
                foreach (var item in ModEntry.Professions)
                {
                    if (level == item.Value.LevelRequirement && DisplayHandler.AreSkillConditionsMet(skill, -1) == item.Value.Skill.ToString())
                        toReturn.Add(item.Value.ID);
                }
            }
            return toReturn;
        }
        public void DrawProfessionIcon(SpriteBatch b, Vector2 position, int profession)
        {
            b.Draw(DisplayHandler.ProfessionIcons, position, new((profession - 467830) % 6 * 16, (profession - 467830) / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }
        public ClickableTextureComponent[] CustomSkillBars => DisplayHandler.MyCustomSkillBars.Value;

        public int[] LevelExperiences => ModEntry.levelExperiences;

        public bool MasteryCaveChanges => ModEntry.ModConfig.Value.MasteryCaveChanges;

        public bool ProfessionsOnly => ModEntry.ModConfig.Value.ProfessionsOnly;

        public bool ColorBlindnessChanges => ModEntry.ModConfig.Value.ColorBlindnessChanges;

        public bool StaminaCostAdjustments => ModEntry.ModConfig.Value.StaminaCostAdjustments;
    }
}
