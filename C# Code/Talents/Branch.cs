using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Talents
{
    public class Branch
    {
        public string Name;

        public Func<string, string> DisplayName;

        public Func<string, string> Desc;

        public string Flag;

        public Vector2 Position;
    }
}
