using System;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Talents
{
    public class Talent
    {
        public string Name = "";

        public string Skill = "";

        public string[] Requirements = Array.Empty<string>();

        internal static string LockedName = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Name");

        internal static string LockedDesc = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Desc");

        internal static string NumberLocked = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Numbered");

        public int AmountToBuyFirst = 0;

        public string MailFlag = "";

        public Branch[] Branches = null;

        internal Func<string, string> DisplayName = null;

        internal Func<string, string> Description = null;

        public Vector2 Position = new();

        public Action<string, bool> OnTalentAdded = null;

        public Action<string, bool> OnTalentRemoved = null;
    }
}