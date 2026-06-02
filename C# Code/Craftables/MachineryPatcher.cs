using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;
using StardewValley;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryPatcher
    {
        public static void ApplyPatches()
        {
            CoreUtility.PatchMethod("MachineryPatcher", "Critter.draw",
                original: AccessTools.Method(typeof(Critter), nameof(Critter.draw)),
                transpiler: new(typeof(MachineryPatcher), nameof(draw_Critter_Transpiler))
            );
            CoreUtility.PatchMethod("MachineryPatcher", "StardewValley.Object.draw_3",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) }),
                transpiler: new(typeof(MachineryPatcher), nameof(draw_SObject_Transpiler_3))
            );
            CoreUtility.PatchMethod("MachineryPatcher", "StardewValley.Object.draw_4",
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               transpiler: new(typeof(MachineryPatcher), nameof(draw_SObject_Transpiler_4))
            );
        }

        public static IEnumerable<CodeInstruction> draw_Critter_Transpiler(IEnumerable<CodeInstruction> insns)
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

        public static IEnumerable<CodeInstruction> draw_SObject_Transpiler_4(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> list = new();
            try
            {
                int count = 5;
                var method = AccessTools.PropertyGetter(typeof(Color), "White");
                var getColorMethod = AccessTools.Method(typeof(MachineryPatcher), nameof(MaybeGetLightSourceColor));
                foreach (var item in insns)
                {
                    if (item.Is(OpCodes.Call, method))
                    {
                        count--;
                        if (count == 0)
                        {
                            list.Add(new(OpCodes.Ldarg_0));
                            list.Add(new(OpCodes.Call, getColorMethod));
                            continue;
                        }
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
        public static IEnumerable<CodeInstruction> draw_SObject_Transpiler_3(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> list = new();
            try
            {
                int count = 3;
                var method = AccessTools.PropertyGetter(typeof(Color), "White");
                var getColorMethod = AccessTools.Method(typeof(MachineryPatcher), nameof(MaybeGetLightSourceColor));
                foreach (var item in insns)
                {
                    if (item.Is(OpCodes.Call, method))
                    {
                        count--;
                        if (count == 0)
                        {
                            list.Add(new(OpCodes.Ldarg_0));
                            list.Add(new(OpCodes.Call, getColorMethod));
                            continue;
                        }
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

        static Color MaybeGetLightSourceColor(StardewValley.Object obj)
        {
            return obj.lightSource?.color.Value ?? Color.White;
        }
    }
}
