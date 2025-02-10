using System.Collections.Generic;

namespace VanillaPlusProfessions
{
    public interface IProfessionManager
    {
        public int SkillValue { get; }

        public Dictionary<string, Profession> RelatedProfessions { get; internal set; }

        public void ApplyPatches();
    }
}
