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
        internal Dictionary<IEnumerable<string>, Action<Dictionary<string, string>>> RunBeforeTalentMenuCloses = new();
        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, List<Talent> talents, Texture2D treeTexture, Rectangle sourceRect, int bundleID = -1, Color? tintColor = null)
        {
            string[] skills = ModEntry.SpaceCoreAPI.GetCustomSkills();
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
                CustomTalentTrees.TryAdd(skillID, new(null, skillID, displayTitle.Invoke(), treeTexture, talents, sourceRect, bundleID, tintColor));
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

        public IEnumerable<string> GetTalentsForPlayer(Farmer who = null)
        {
            if (who is null)
            {
                who = Game1.player;
            }
            List<string> talents = new();
            if (!Context.IsWorldReady)
            {
                return talents;
            }

            foreach (var item in TalentCore.Talents)
            {
                if (TalentUtility.CurrentPlayerHasTalent(item.Value.MailFlag, ignoreDisabledTalents: true))
                {
                    talents.Add(item.Value.Name);
                }
            }

            return talents;
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

        public bool RegisterTalentStatusAction(IEnumerable<string> talents, Action<Dictionary<string, string>> action)
        {
            if (talents is not null && action is not null)
            {
                RunBeforeTalentMenuCloses.Add(talents, action);
                return true;
            }
            else
            {
                ModEntry.ModMonitor.Log("Invalid values were provided to the VPP API's RegisterTalentStatusAction method: ", LogLevel.Warn);
                if (talents is null && action is not null)
                {
                    ModEntry.ModMonitor.Log($"The 'talents' parameter is null but the 'action' parameter is not. The method is declared in the {action.Method.DeclaringType.Assembly.FullName ?? "<could not be found>"} assembly, {action.Method.DeclaringType.FullName ?? "<could not be found>"} type.", LogLevel.Warn);
                }
                else if (action is null && talents is not null)
                {
                    ModEntry.ModMonitor.Log($"The 'action' parameter is null but the 'talents' parameter is not. The talent values are: { string.Join('/', talents)}", LogLevel.Warn);
                }
                else
                {
                    ModEntry.ModMonitor.Log($"Both of the 'talents' and 'action' parameters are null.", LogLevel.Warn);
                }
                ModEntry.ModMonitor.Log("This should NOT be reported on VPP's page, you're seeing this warning because another mod used the API incorrectly.", LogLevel.Warn);
                return false;
            }
        }
        public ClickableTextureComponent[] CustomSkillBars => DisplayHandler.MyCustomSkillBars.Value;

        public int[] LevelExperiences => ModEntry.levelExperiences;

        public int MasteryCaveChanges => ModEntry.ModConfig.Value.MasteryCaveChanges;

        public bool ProfessionsOnly => ModEntry.ModConfig.Value.ProfessionsOnly;

        public bool ColorBlindnessChanges => ModEntry.ModConfig.Value.ColorBlindnessChanges;

        public bool StaminaCostAdjustments => ModEntry.ModConfig.Value.StaminaCostAdjustments;
    }
}
