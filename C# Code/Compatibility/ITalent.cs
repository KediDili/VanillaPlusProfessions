using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Compatibility
{
    public interface ITalent
    {
        public string Name { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public string MailFlag { get; }

        public string[] Requirements { get; }

        public Branch[] Branches { get; }

        public class Branch
        {
            //Internal name of the branch
            public string Name;

            //Translated name of the branch
            public Func<string, string> DisplayName;

            //Translated description of the branch
            public Func<string, string> Desc;

            //The mail flag immediately added upon choosing this branch
            public string Flag;

            //The position on the menu (No multiplying with 4)
            public Vector2 Position;
        }
    }
}
