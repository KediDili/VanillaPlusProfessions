using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Compatibility
{
    public class HasTalents
    {
        public bool AllowsInput() => false;
        public bool RequiresInput() => false;
        public bool CanHaveMultipleValues(string input = null) => false;
        public bool IsReady() => Context.IsWorldReady;
        public bool UpdateContext() => true;
        public IEnumerable<string> GetValues(string input)
        {
            if (ModEntry.ModConfig.Value.ProfessionsOnly)
            {
                yield return null;
                yield break;
            }
            foreach (var item in TalentCore.Talents)
            {
                if (TalentUtility.CurrentPlayerHasTalent(item.Value.MailFlag, ignoreDisabledTalents: true))
                {
                    yield return item.Value.Name;
                }
            }
        }
    }
}
