using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    public class CraftablePatcher
    {
        public static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                    transpiler: new(AccessTools.Method(typeof(CraftablePatcher), nameof(newDay_Transpiler)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crop.newDay'", "transpiling");
            }
        }
        public static IEnumerable<CodeInstruction> newDay_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            var method = AccessTools.Method(typeof(Crop), nameof(Crop.isWildSeedCrop));
            int index = 0, count = 1;
            try
            {
                foreach (var ins in list)
                {
                    if (ins.operand is MethodInfo info && info == method)
                    {
                        if (count != 0)
                        {
                            count--;
                        }
                        else
                        {
                            object instruction = list[index + 1].operand;
                            list.Insert(index + 2, new(OpCodes.Ldarg_0));
                            list.Insert(index + 3, new(OpCodes.Call, AccessTools.Method(typeof(CraftablePatcher), nameof(IsVPPForageCrop))));
                            list.Insert(index + 4, new(OpCodes.Brtrue_S, instruction));
                            break;
                        }
                    }
                    index++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crop.newDay'", "transpiling");
            }
            return list;
        }

        public static bool IsVPPForageCrop(Crop crop)
        {
            return crop.netSeedIndex.Value is "(O)sdsd" or "(O)ssdsdsd" or "sdsdawwd" or "ndjfhbgjske";
        }
    }
}
