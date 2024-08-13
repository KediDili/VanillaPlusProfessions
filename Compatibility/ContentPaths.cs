using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace VanillaPlusProfessions.Compatibility
{
    public class ContentPaths
    {
        public bool AllowsInput() => true;
        public bool RequiresInput() => true;
        public bool CanHaveMultipleValues(string input = null) => false;
        public bool IsReady() => true;
        public bool UpdateContext() => true;
        public IEnumerable<string> GetValues(string input)
        {
            if (ContentEditor.ContentPaths.TryGetValue(input, out string output) && !string.IsNullOrEmpty(output))
            {
                yield return output;
                yield break;
            }
            yield return "Place/Holder";
        }
    }
}
