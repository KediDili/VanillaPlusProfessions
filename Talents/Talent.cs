using System;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Talents
{
    public class Talent
    {
        public string Name = "";

        public string Skill = "";

        public string RequiresTalent = "";

        public string RequiresTalent2 = "";

        internal Func<string> DisplayName = null;

        internal Func<string> Description = null;

        public Vector2 Position = new();
    }
}