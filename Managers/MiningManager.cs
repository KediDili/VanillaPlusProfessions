using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Enchantments;
using System.Collections.Generic;
using StardewValley;
using System.Linq;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;
using StardewValley.Extensions;
using System;

namespace VanillaPlusProfessions.Managers
{
    public class MiningManager : IProfessionManager
    {
        public int SkillValue => 3;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.receiveLeftClick)),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), nameof(ForgeMenu.receiveLeftClick), "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(ForgeMenu), "_UpdateDescriptionText"),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'ForgeMenu._UpdateDescriptionText'", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.draw), new System.Type[] { typeof(SpriteBatch) }),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), nameof(ForgeMenu.draw), "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.update)),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'ForgeMenu.update'", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method("StardewValley.GameLocation:breakStone"),
                    prefix: new HarmonyMethod(typeof(MiningManager), nameof(breakStone_Prefix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'GameLocation.breakStone'", "prefixing");
            }
            TypeBeingPatched = typeof(BaseEnchantment);
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.GetEnchantmentFromItem)),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'BaseEnchantment.GetEnchantmentFromItem'", "transpiling");
            }
            TypeBeingPatched = null;
        }

        private static System.Type TypeBeingPatched = null; 

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilgen)
        {
            var found = false;
            var skip = 0;
            var index = 0;
            List<CodeInstruction> list = codeInstructions.ToList();

            foreach (var item in codeInstructions)
            {
                if (item.opcode.Equals(OpCodes.Ldstr) && item.operand.Equals("(O)74") && !found)
                {
                    Label lbl = ilgen.DefineLabel();
                    list[index + 3].labels.Add(lbl);

                    yield return item;
                    yield return list[index + 1];
                    yield return new(OpCodes.Brtrue_S, lbl);
                    if (TypeBeingPatched is null)
                    {
                        yield return new(OpCodes.Ldarg_0);
                        yield return new(OpCodes.Ldfld, AccessTools.Field(typeof(ForgeMenu), "rightIngredientSpot"));
                        yield return new(OpCodes.Ldfld, AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.item)));
                        yield return new(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        yield return new(OpCodes.Ldarg_1);
                        yield return new(OpCodes.Ldc_I4_0);
                    }
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(MiningManager), "Conditions"));
                    found = true;
                    skip = 1;
                }
                else if (skip > 0)
                {
                    skip--;
                    continue;
                }
                else
                {
                    index++;
                    yield return item;
                }
            }
        }
        public static void breakStone_Prefix(string stoneId, int x, int y, Farmer who, Random r)
        {
            if (who == Game1.player)
            {
                if (CoreUtility.CurrentPlayerHasProfession("Appraiser", useThisInstead: who) && stoneId is "843" or "844")
                {
                    int number = Game1.random.Next(0, 3);
                    for (int i = 0; i < number; i++)
                    {
                        Game1.createObjectDebris("(O)848", x, y);
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("Volatility", who: who) && r.NextBool(0.005))
                {
                    if (stoneId == "751")
                    {
                        Game1.createObjectDebris("(O)380", x, y);
                    }
                    else if (stoneId == "850" || stoneId == "290")
                    {
                        Game1.createObjectDebris("(O)384", x, y);
                    }
                    else if (stoneId == "764" || stoneId == "VolcanoGoldNode")
                    {
                        Game1.createObjectDebris("(O)386", x, y);
                    }
                    else if (stoneId == "765")
                    {
                        Game1.createObjectDebris("(O)909", x, y);
                    }
                }
            }
        }
        public static bool Conditions(Item item, bool doesStackMatter = true)
        {
            return doesStackMatter
                ? item?.QualifiedItemId == "(O)82" && item?.Stack >= 5 && CoreUtility.CurrentPlayerHasProfession("Enchanter")
                : item?.QualifiedItemId == "(O)82" && CoreUtility.CurrentPlayerHasProfession("Enchanter");
        }
    }
}
