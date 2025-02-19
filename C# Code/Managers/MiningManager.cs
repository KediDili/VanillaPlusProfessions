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
using System.Reflection;

namespace VanillaPlusProfessions.Managers
{
    public class MiningManager : IProfessionManager
    {
        public int SkillValue => 3;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        readonly static string PatcherName = nameof(MiningManager);
        readonly static System.Type PatcherType = typeof(MiningManager);

        public void ApplyPatches()
        {
            MethodBase[] basemethods = {
                AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.receiveLeftClick)),
                AccessTools.Method(typeof(ForgeMenu), "_UpdateDescriptionText"),
                AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.draw), new System.Type[] { typeof(SpriteBatch) }),
                AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.update)),
                AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.GetEnchantmentFromItem))
            };
            for (int i = 0; i < basemethods.Length; i++)
            {
                if (i == basemethods.Length - 1)
                {
                    TypeBeingPatched = typeof(BaseEnchantment);
                }
                CoreUtility.PatchMethod(
                    PatcherName, $"{basemethods[i].DeclaringType.Name}.{basemethods[i].Name}",
                    original: basemethods[i],
                    transpiler: new HarmonyMethod(PatcherType, nameof(Transpiler))
                );
                TypeBeingPatched = null;
            }
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.breakStone",
                original: AccessTools.Method("StardewValley.GameLocation:breakStone"),
                prefix: new HarmonyMethod(PatcherType, nameof(breakStone_Prefix))
            );
        }

        private static System.Type TypeBeingPatched = null; 

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilgen)
        {
            List<CodeInstruction> list = codeInstructions.ToList();
            List<CodeInstruction> toReturn = codeInstructions.ToList();
            try
            {
                var found = false;
                var skip = 0;
                var index = 0;
                foreach (var item in codeInstructions)
                {
                    if (item.opcode.Equals(OpCodes.Ldstr) && item.operand.Equals("(O)74") && !found)
                    {
                        Label lbl = ilgen.DefineLabel();
                        list[index + 3].labels.Add(lbl);

                        toReturn.Add(item);
                        toReturn.Add(list[index + 1]);
                        toReturn.Add(new(OpCodes.Brtrue_S, lbl));
                        if (TypeBeingPatched is null)
                        {
                            toReturn.Add(new(OpCodes.Ldarg_0));
                            toReturn.Add(new(OpCodes.Ldfld, AccessTools.Field(typeof(ForgeMenu), "rightIngredientSpot")));
                            toReturn.Add(new(OpCodes.Ldfld, AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.item))));
                            toReturn.Add(new(OpCodes.Ldc_I4_1));
                        }
                        else
                        {
                            toReturn.Add(new(OpCodes.Ldarg_1));
                            toReturn.Add(new(OpCodes.Ldc_I4_0));
                        }
                        toReturn.Add(new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(Conditions))));
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
                        toReturn.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "<Enchanter profession as a whole: unfortunately this transpiler is called by many parts of VPP>", "transpiled", true);
            }
            return list;
        }
        public static void breakStone_Prefix(string stoneId, int x, int y, Farmer who, Random r)
        {
            try
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
                    if (TalentUtility.CurrentPlayerHasTalent("Volatility", who: who) && r.NextBool(0.05))
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.breakStone", "transpiled", true);
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
