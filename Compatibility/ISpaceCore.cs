using StardewValley;

namespace VanillaPlusProfessions.Compatibility
{
    public interface ISpaceCore
    {
        string[] GetCustomSkills();
        
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
        
        int GetProfessionId(string skill, string profession);
    }
}
