using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using VanillaPlusProfessions.Talents;

namespace VanillaPlusProfessions.Compatibility
{
    public class VanillaPlusProfessionsAPI : IVanillaPlusProfessions
    {

        internal Dictionary<string, SkillTree> CustomTalentTrees = new();

        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, int bundleID, Texture2D texture, Rectangle rectangle, int lockedID)
        {
            string[] skills = ModEntry.SpaceCoreAPI.Value.GetCustomSkills();
            if (skills.Contains(skillID))
            {
                var talentList = (from talent in TalentCore.Talents
                                  where talent.Skill == skillID
                                  select talent).ToList();

                CustomTalentTrees.Add(skillID, new(skillID, bundleID, displayTitle.Invoke(), texture, talentList, rectangle, lockedID));
            }
            else
            {
                ModEntry.ModMonitor.Log("There is no such SpaceCore-registered skill with the ID of " + skillID + ". Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);
            }
        }
        public void RegisterCustomSkillTalent(string skillID, string name, Func<string> displayName, Func<string> tooltip, Vector2 position, string requiresTalent1 = "", string requiresTalent2 = "")
        {
            string[] skills = ModEntry.SpaceCoreAPI.Value.GetCustomSkills();
            if (skills.Contains(skillID))
            {
                Talent talent = new()
                {
                    Skill = skillID,
                    Name = name,
                    DisplayName = displayName,
                    Description = tooltip,
                    RequiresTalent = requiresTalent1,
                    RequiresTalent2 = requiresTalent2,
                    Position = position
                };

                TalentCore.Talents.Add(talent);
            }
            else
            {
                ModEntry.ModMonitor.Log("There is no such SpaceCore-registered skill with the ID of " + skillID + ". Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);
            }
        }
        public bool IsTalentGained(string name, long farmerID = -1, Farmer who = null)
        {
            if (who is null)
            {
                if (farmerID is -1)
                {
                    return TalentCore.GainedTalents.Value.Contains(name);
                }
                else
                {
                    who = Game1.getFarmer(farmerID);
                    if (who.modData.TryGetValue(TalentCore.Key_GainedTalents, out string talents))
                    {
                        if (talents is not null)
                        {
                            string[] strings = talents.Split(TalentCore.Seperator_GainedTalents, StringSplitOptions.RemoveEmptyEntries);
                            return strings.Contains(name);
                        }
                    }
                    return false;
                }
            }
            else
            {
                if (who.modData.TryGetValue(TalentCore.Key_GainedTalents, out string talents))
                {
                    if (talents is not null)
                    {
                        string[] strings = talents.Split(TalentCore.Seperator_GainedTalents, StringSplitOptions.RemoveEmptyEntries);
                        return strings.Contains(name);
                    }
                }
                return false;
            }
        }
    }
}
