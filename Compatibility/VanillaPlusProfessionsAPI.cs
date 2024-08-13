using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Talents.UI;

namespace VanillaPlusProfessions.Compatibility
{
    public class VanillaPlusProfessionsAPI : IVanillaPlusProfessions
    {

        internal Dictionary<string, SkillTree> CustomTalentTrees = new();

        internal Dictionary<string, Talent> CustomTalents = new();
        public void RegisterCustomSkillTree(string skillID, Func<string> displayTitle, Texture2D texture, Rectangle rectangle, int bundleID = -1, Color? tintColor = null)
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
                var talentList = (from talent in CustomTalents
                                  where talent.Value.Skill == skillID
                                  select talent.Value).ToList();

                CustomTalentTrees.Add(skillID, new(skillID, displayTitle.Invoke(), texture, talentList, rectangle, bundleID, tintColor));
            }
            else
            {
                ModEntry.ModMonitor.Log("There is no such SpaceCore-registered skill with the ID of " + skillID + ". Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);                
            }
        }
        public void RegisterCustomSkillTalent(string skillID, string name, Func<string,string> displayName, Func<string, string> tooltip, Vector2 position, string[] requirements, int amountToBuyFirst = 0)
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
                    Requirements = requirements,
                    AmountToBuyFirst = amountToBuyFirst,
                    Position = position
                };
                if (!CustomTalents.TryAdd(name, talent))
                    CustomTalents[name] = talent;
            }
            else
            {
                ModEntry.ModMonitor.Log("There is no such SpaceCore-registered skill with the ID of " + skillID + ". Please let the custom skill mod author know of this.", StardewModdingAPI.LogLevel.Error);
            }
        }
    }
}
