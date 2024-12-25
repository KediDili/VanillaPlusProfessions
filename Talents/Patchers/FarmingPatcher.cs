using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;
using StardewValley.Events;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using VanillaPlusProfessions.Utilities;
using System.Reflection;
using StardewValley.Characters;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public class FarmingPatcher
    {
        internal static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
                    transpiler: new HarmonyMethod(typeof(FarmingPatcher), nameof(performLightningUpdate_Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(Utility.performLightningUpdate), "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farm), nameof(Farm.addCrows)),
                    transpiler: new HarmonyMethod(typeof(FarmingPatcher), nameof(addCrows_Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(Farm.addCrows), "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
                    transpiler: new HarmonyMethod(typeof(FarmingPatcher), nameof(setUp_Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(QuestionEvent.setUp), "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FairyEvent), "ChooseCrop"),
                    postfix: new HarmonyMethod(typeof(FarmingPatcher), nameof(ChooseCrop_Postfix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "FairyEvent.ChooseCrop", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
                    transpiler: new HarmonyMethod(typeof(FarmingPatcher), nameof(dayUpdate_Transpiler))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "FarmAnimal.dayUpdate", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
                    postfix: new HarmonyMethod(typeof(FarmingPatcher), nameof(harvest_Postfix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(Crop.harvest), "postfixing");
            }
                        
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Object), nameof(Object.GetModifiedRadiusForSprinkler)),
                    postfix: new HarmonyMethod(typeof(FarmingPatcher), nameof(GetModifiedRadiusForSprinkler_Postfix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(Object.GetModifiedRadiusForSprinkler), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                    prefix: new HarmonyMethod(typeof(FarmingPatcher), nameof(dayUpdate_Prefix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), nameof(Object.GetModifiedRadiusForSprinkler), "prefixing");
            }
        }
        public static void dayUpdate_Prefix(HoeDirt __instance)
        {
            try
            {
                if (__instance.crop is not null and Crop crop && !crop.dead.Value && __instance.isWatered())
                {
                    if (TalentUtility.ShouldCropGrowByOneDay(__instance, crop))
                    {
                        crop.newDay(__instance.state.Value);
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "HoeDirt.dayUpdate", "prefixed", true);
            }
        }


        public static IEnumerable<CodeInstruction> dayUpdate_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool found = false;
            var methodinfo = AccessTools.PropertySetter(typeof(Item), nameof(Item.Stack));
            foreach (var code in codeInstructions)
            {
                yield return code;
                if (code.opcode.Equals(OpCodes.Callvirt) && code.OperandIs(methodinfo) && !found)
                {
                    yield return new(OpCodes.Ldarg_0);
                    yield return new(OpCodes.Ldloc_S, 18);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(FarmingPatcher), nameof(SaveFarmAnimalProductData)));
                    found = true;
                }
            }
        }

        public static void harvest_Postfix(Crop __instance, bool __result, JunimoHarvester junimoHarvester)
        {
            try
            {
                if (!string.IsNullOrEmpty(__instance?.netSeedIndex.Value) && !__instance.dead.Value && TalentUtility.CurrentPlayerHasTalent("CycleOfLife"))
                {
                    if (__result && Game1.random.NextBool() && !__instance.RegrowsAfterHarvest())
                    {
                        if (junimoHarvester is null)
                        {
                            Game1.createObjectDebris(__instance.netSeedIndex.Value, (int)__instance.tilePosition.X, (int)__instance.tilePosition.Y, Game1.player.UniqueMultiplayerID, __instance.currentLocation);
                        }
                        else
                        {
                            junimoHarvester.home.GetBuildingChest("Output").addItem(ItemRegistry.Create<Object>(__instance.netSeedIndex.Value));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "Crop.harvest", "postfixed", true);
            }
        }

        public static void SaveFarmAnimalProductData(FarmAnimal farmAnimal, StardewValley.Object product)
        {
            var data = farmAnimal.GetAnimalData();
            bool isDeluxe = false;
            bool hasGivenAnyAtAll = false;
            try
            {
                foreach (var item in data.DeluxeProduceItemIds)
                {
                    if (item.ItemId == product.ItemId)
                    {
                        isDeluxe = true;
                        break;
                    }
                }
                foreach (var item in data.ProduceItemIds)
                {
                    if (item.ItemId == product.ItemId)
                    {
                        hasGivenAnyAtAll = true;
                        break;
                    }
                }
                if (!hasGivenAnyAtAll)
                {
                    if (!farmAnimal.modData.TryAdd(TalentCore.Key_WildGrowth, "none"))
                    {
                        farmAnimal.modData[TalentCore.Key_WildGrowth] = "none";
                    }
                }
                else
                {
                    if (!farmAnimal.modData.TryAdd(TalentCore.Key_WildGrowth, isDeluxe.ToString().ToLower()))
                    {
                        farmAnimal.modData[TalentCore.Key_WildGrowth] = isDeluxe.ToString().ToLower();
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "FarmAnimal.dayUpdate", "transpiled", true);
            }
        }

        public static void GetModifiedRadiusForSprinkler_Postfix(ref int __result, Object __instance)
        {
            try
            {
                if (__instance is not null && __result is not -1 && __instance.heldObject.Value is not null)
                {
                    if (__instance.heldObject.Value.QualifiedItemId == "(O)Kedi.VPP.Fertigator")
                        __result++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "StardewValley.Object.GetModifiedRadiusForSprinkler", "postfixed", true);
            }
        }

        public static void ChooseCrop_Postfix(Vector2 __result)
        {
            try
            {
                if (TalentUtility.AllPlayersHaveTalent("Farming_Fae_Blessings"))
                {
                    Game1.getFarm().modData[TalentCore.Key_FaeBlessings] = $"{__result.X}+{__result.Y}";
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "FairyEvent.ChooseCrop", "postfixed", true);
            }
        }

        public static IEnumerable<CodeInstruction> addCrows_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            try
            {
                int index = 0;
                foreach (var item in list)
                {
                    if (item.opcode == OpCodes.Ldloc_S && item.operand is 14)
                    {
                        list.Insert(index, new(OpCodes.Call, AccessTools.Method(typeof(FarmingPatcher), nameof(TryOverridePhaseGrowth))));
                        list.Insert(index, new(OpCodes.Ldloc_S, 13));
                    }
                    index++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "Farm.addCrows", "transpiling");
            }
            return list;
        }

        public static bool TryOverridePhaseGrowth(HoeDirt dirt)
        {
            try
            {
                if (TalentUtility.HostHasTalent("Trickster"))
                {
                    if (dirt.crop is not null and Crop crop && crop.currentPhase.Value == crop.phaseDays.Count - 1)
                    {
                        return false;
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "Farm.addCrows", "transpiled", true);
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> performLightningUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int expected_ldloc0s = 3;
            int index = 0;
            List<CodeInstruction> list = instructions.ToList();
            try
            {
                foreach (var pair in list)
                {
                    index++;
                    if (expected_ldloc0s > 0 && pair.opcode.Equals(OpCodes.Ldloc_0))
                    {
                        expected_ldloc0s--;
                        if (expected_ldloc0s == 0)
                        {
                            list.RemoveRange(index, 14);
                            pair.opcode = OpCodes.Call;
                            pair.operand = AccessTools.Method(typeof(FarmingPatcher), nameof(ShouldFarmBeProtected));
                            list[index].opcode = OpCodes.Brfalse_S;
                            break;
                        }
                    }
                }

            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "GameLocation.performLightningUpdate", "transpiling");
            }
            
            return list;
        }

        //return false if yes, true if no
        public static bool ShouldFarmBeProtected()
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("StormSurge") || TalentUtility.CurrentPlayerHasTalent("StaticCharge"))
                {
                    Farm farm = Game1.getFarm();
                    var LightningRods = from obj in farm.Objects.Pairs
                                        where obj.Value is not null && obj.Value.QualifiedItemId == "(BC)9"
                                        select obj.Key;
                    if (TalentUtility.CurrentPlayerHasTalent("StaticCharge"))
                    {
                        foreach (var tile in LightningRods)
                        {
                            if (farm.Objects[tile].heldObject.Value is not null)
                            {
                                if (farm.Objects[tile].modData.ContainsKey(TalentCore.Key_StaticCharge) && farm.Objects[tile].heldObject.Value.Stack < 2)
                                    farm.Objects[tile].heldObject.Value.Stack++;
                                else
                                    farm.Objects[tile].modData.TryAdd(TalentCore.Key_StaticCharge, "placeholder");
                            }
                        }
                    }
                    if (TalentUtility.CurrentPlayerHasTalent("StormSurge"))
                    {
                        int CropsAndTrees = (from TerrainFeature terrainFeature in farm.terrainFeatures.Values
                                             where (terrainFeature is FruitTree) || (terrainFeature is HoeDirt dirt && dirt.crop is not null and Crop crop && !crop.dead.Value)
                                             select terrainFeature).Count();

                        return LightningRods.Count() * 30 < CropsAndTrees;
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingPatcher), "GameLocation.performLightningUpdate", "transpiled", true);
            }
            //Return this weird calculation that vanilla does
            return Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0;
        }

        public static float ReturnBirthChance() => TalentUtility.CurrentPlayerHasTalent("Farming_Brimming_With_Life") ? 0.011f : 0.0055f;
        
        public static IEnumerable<CodeInstruction> setUp_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var codeInstruction in instructions)
            {
                if (codeInstruction.OperandIs(0.0055f))
                {
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(FarmingPatcher), nameof(ReturnBirthChance)));
                    continue;
                }
                yield return codeInstruction;
            }
        }
    }
}