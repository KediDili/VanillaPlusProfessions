using HarmonyLib;
using StardewValley.BellsAndWhistles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryPatcher
    {
        public static void ApplyPatches()
        {
            CoreUtility.PatchMethod("MachineryPatcher", "Critter.draw",
                original: AccessTools.Method(typeof(Critter), nameof(Critter.draw)),
                transpiler: new(typeof(MachineryPatcher), nameof(draw_Transpiler))
            );
        }

        public static IEnumerable<CodeInstruction> draw_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> list = new();
            try
            {
                foreach (var item in insns)
                {
                    if (item.Is(OpCodes.Ldc_R4, 1000000f))
                    {
                        list.Add(new(OpCodes.Ldc_R4, 2000000f));
                        continue;
                    }
                    list.Add(item);
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, "MachineryPatcher", "Critter.draw", "transpiler");
            }
            return list;
        }
    }
}
