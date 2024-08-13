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
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.Transpiler))
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
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.Transpiler))
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
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.Transpiler))
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
                transpiler: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.Transpiler))
            );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'ForgeMenu.update'", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "createMultipleObjectDebris", new System.Type[] { typeof(string), typeof(int), typeof(int), typeof(int), typeof(long), typeof(GameLocation) }),
                prefix: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.createMultipleObjectDebris_Prefix))
            );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiningManager), "'Game1.createMultipleObjectDebris'", "prefixing");
            }
            TypeBeingPatched = typeof(BaseEnchantment);
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.GetEnchantmentFromItem)),
                    transpiler: new HarmonyMethod(typeof(MiningManager), nameof(MiningManager.Transpiler))
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
        public static void createMultipleObjectDebris_Prefix(string id, int xTile, int yTile, ref int number, long who)
        {
            Farmer farmer = Game1.getFarmer(who);
            if (CoreUtility.CurrentPlayerHasProfession(60, useThisInstead: farmer) && id == "(O)848")
            {
                number = 0;

                if (farmer.professions.Contains(18))
                    number++;
                if (farmer.hasBuff("dwarfStatue_0"))
                    number++;
                number += Game1.random.Next(2, 5);
            }
            if (TalentUtility.AnyPlayerHasTalent("Mining_Volatile") && Game1.random.NextBool())
            {
                if (id == "(O)378")
                {
                    Game1.createObjectDebris("(O)380", xTile, yTile);
                }
                else if (id == "(O)380")
                {
                    Game1.createObjectDebris("(O)384", xTile, yTile);
                }
                else if (id == "(O)384")
                {
                    Game1.createObjectDebris("(O)386", xTile, yTile);
                }
                else if (id == "(O)386")
                {
                    Game1.createObjectDebris("(O)909", xTile, yTile);
                }
            }
        }
        public static bool Conditions(Item item, bool doesStackMatter = true)
        {
            return doesStackMatter
                ? item?.QualifiedItemId == "(O)82" && item?.Stack >= 5 && CoreUtility.CurrentPlayerHasProfession(61)
                : item?.QualifiedItemId == "(O)82" && CoreUtility.CurrentPlayerHasProfession(61);
        }
    }
}
