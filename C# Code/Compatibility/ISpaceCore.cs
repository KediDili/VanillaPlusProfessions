using StardewValley;
using System;

namespace VanillaPlusProfessions.Compatibility
{
    public interface ISpaceCore
    {
        string[] GetCustomSkills();
        
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        
        int GetExperienceForCustomSkill(Farmer farmer, string skill);

        int GetBuffLevelForCustomSkill(Farmer farmer, string skill);

        int GetProfessionId(string skill, string profession);

        /// Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);
    }
}
