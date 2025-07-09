using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;
using StardewValley.Events;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using VanillaPlusProfessions.Utilities;
using StardewValley.Characters;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Internal;
using StardewValley.Objects;
using VanillaPlusProfessions.Craftables;
using StardewValley.Locations;
using System;
using StardewModdingAPI;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public class FarmingPatcher
    {
        readonly static string PatcherName = nameof(FarmingPatcher);
        readonly static System.Type PatcherType = typeof(FarmingPatcher);

        internal static void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                PatcherName, "Utility.performLightningUpdate",
                original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
                transpiler: new HarmonyMethod(PatcherType, nameof(performLightningUpdate_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Farm.addCrows",
                original: AccessTools.Method(typeof(Farm), nameof(Farm.addCrows)),
                transpiler: new HarmonyMethod(PatcherType, nameof(addCrows_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "QuestionEvent.setUp",
                original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
                transpiler: new HarmonyMethod(PatcherType, nameof(setUp_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "FairyEvent.ChooseCrop",
                original: AccessTools.Method(typeof(FairyEvent), "ChooseCrop"),
                postfix: new HarmonyMethod(PatcherType, nameof(ChooseCrop_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "FarmAnimal.dayUpdate",
                original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
                transpiler: new HarmonyMethod(PatcherType, nameof(dayUpdate_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Crop.harvest",
                original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
                postfix: new HarmonyMethod(PatcherType, nameof(harvest_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.GetModifiedRadiusForSprinkler",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.GetModifiedRadiusForSprinkler)),
                postfix: new HarmonyMethod(PatcherType, nameof(GetModifiedRadiusForSprinkler_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "HoeDirt.dayUpdate",
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                prefix: new HarmonyMethod(PatcherType, nameof(dayUpdate_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Building.CheckItemConversionRule",
                original: AccessTools.Method(typeof(Building), nameof(Building.CheckItemConversionRule)),
                prefix: new HarmonyMethod(PatcherType, nameof(CheckItemConversionRule_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Utility.pickFarmEvent",
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
                postfix: new HarmonyMethod(PatcherType, nameof(pickFarmEvent_Postfix))
            );
        }
        public static void pickFarmEvent_Postfix(ref FarmEvent __result)
        {
            if (TalentUtility.HostHasTalent("FairysKiss") && Context.IsMainPlayer)
            {
                if (__result is null)
                {
                    bool multiplayerFlag = true;
                    foreach (Farmer farmer in Game1.getOnlineFarmers())
                    {
                        Friendship friendship = farmer.GetSpouseFriendship();
                        if (friendship != null && friendship.IsMarried() && friendship.WeddingDate == Game1.Date)
                        {
                            multiplayerFlag = false;
                            break;
                        }
                    }
                    if (!Game1.weddingToday && multiplayerFlag)
                    {
                        Random random = Utility.CreateDaySaveRandom();

                        int fairyRoseNumber = (from terrainFeature in Game1.getFarm().terrainFeatures.Values
                                               where terrainFeature is HoeDirt hoedirt && hoedirt.crop is Crop cCrop &&
                                               cCrop.indexOfHarvest.Value == "595" && cCrop.currentPhase.Value == cCrop.phaseDays.Count - 1
                                               select terrainFeature).Count();

                        double baseChance = 0.01;

                        if (random.NextBool(baseChance + (fairyRoseNumber * 0.007)))
                            __result = new FairyEvent();
                    }
                }
            }
        }

        public static bool CheckItemConversionRule_Prefix(Building __instance, BuildingItemConversion conversion, ItemQueryContext itemQueryContext)
        {
            try
            {
                if (__instance.buildingType.Value == "Mill" && TalentUtility.CurrentPlayerHasTalent("FineGrind"))
                {
                    int maxDailyConversions = conversion.MaxDailyConversions, requiredCount = 0, chestItemCount = -1;
                    Chest sourceChest = __instance.GetBuildingChest(conversion.SourceChest), destinationChest = __instance.GetBuildingChest(conversion.DestinationChest);
                    List<int> items = new();

                    if (sourceChest is null)
                        return false;

                    foreach (Item item in sourceChest.Items)
                    {
                        chestItemCount++;
                        if (item == null)
                            continue;
                        bool fail = false;
                        foreach (string requiredTag in conversion.RequiredTags)
                        {
                            if (!item.HasContextTag(requiredTag))
                            {
                                fail = true;
                                break;
                            }
                        }
                        if (fail)
                            continue;
                        
                        requiredCount += item.Stack;
                        requiredCount -= requiredCount % conversion.RequiredCount;
                        items.Add(chestItemCount);
                        int outputAmount = 1;
                        if (requiredCount >= conversion.RequiredCount && (maxDailyConversions < conversion.MaxDailyConversions || conversion.MaxDailyConversions == -1))
                        {
                            for (int i = 0; i < conversion.ProducedItems.Count; i++)
                            {
                                var producedItem = conversion.ProducedItems[i];
                                Item item2 = ItemQueryResolver.TryResolveRandomItem(producedItem, itemQueryContext, inputItem: item);
                                if (GameStateQuery.CheckConditions(producedItem.Condition, __instance.GetParentLocation(), targetItem: item2, inputItem: item))
                                {
                                    outputAmount = item2.Stack * (requiredCount / conversion.RequiredCount);
                                    Item ıtem = null;
                                    if (outputAmount > item2.maximumStackSize())
                                    {
                                        var stacks = SplitStacks(outputAmount, item2.maximumStackSize());
                                        for (int h = 0; h < stacks.Count; h++)
                                        {
                                            destinationChest.addItem(ItemRegistry.Create(item2.QualifiedItemId, stacks[h], item2.Quality));
                                        }
                                    }
                                    else
                                    {
                                        ıtem = destinationChest.addItem(ItemRegistry.Create(item2.QualifiedItemId, outputAmount, item2.Quality));
                                    }
                                    if ((ıtem == null || ıtem.Stack != outputAmount) && i == conversion.ProducedItems.Count - 1)
                                    {
                                        if (maxDailyConversions > -1)
                                        {
                                            maxDailyConversions++;
                                        }
                                        foreach (var itemIndex in items)
                                        {
                                            if (sourceChest.Items[itemIndex] is not null)
                                            {
                                                int prevStack = sourceChest.Items[itemIndex].Stack;
                                                sourceChest.Items[itemIndex] = sourceChest.Items[itemIndex].ConsumeStack(requiredCount > sourceChest.Items[itemIndex].Stack ? sourceChest.Items[itemIndex].Stack : requiredCount);
                                                requiredCount -= prevStack;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Building.CheckItemConversionRule", "postfixed", true);
            }
            return true;
        }

        public static List<int> SplitStacks(int number, int maxStack)
        {
            //There's an error here.
            bool extraEntry = number % maxStack != 0;
            int amount = (number / maxStack) + (extraEntry ? 1 : 0);
            List<int> stacks = new();
            //5 - 5 - 5 - 4 => 24
            for (int j = 0; j < amount; j++)
            {
                if (extraEntry && j == amount - 1)
                    stacks.Add(number % maxStack);
                else
                    stacks.Add(number / (number / maxStack));
            }
            return stacks;
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
                CoreUtility.PrintError(e, PatcherName, "HoeDirt.dayUpdate", "prefixed", true);
            }
        }


        public static IEnumerable<CodeInstruction> dayUpdate_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> result = new();
            try
            {
                bool found = false;
                var methodinfo = AccessTools.PropertySetter(typeof(Item), nameof(Item.Stack));
                foreach (var code in codeInstructions)
                {
                    result.Add(code);
                    if (code.opcode.Equals(OpCodes.Callvirt) && code.OperandIs(methodinfo) && !found)
                    {
                        result.Add(new(OpCodes.Ldarg_0));
                        result.Add(new(OpCodes.Ldloc_S, 18));
                        result.Add(new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(SaveFarmAnimalProductData))));
                        found = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FarmAnimal.dayUpdate", "transpiled", true);
            }
            return result;
        }

        public static void harvest_Postfix(Crop __instance, bool __result, JunimoHarvester junimoHarvester)
        {
            try
            {
                if (!string.IsNullOrEmpty(__instance?.netSeedIndex.Value) && !__instance.dead.Value && __result)
                {
                    if (Game1.random.NextBool() && !__instance.RegrowsAfterHarvest() && TalentUtility.CurrentPlayerHasTalent("CycleOfLife"))
                    {
                        if (junimoHarvester is null)
                        {
                            Game1.createObjectDebris(__instance.netSeedIndex.Value, (int)__instance.tilePosition.X, (int)__instance.tilePosition.Y, Game1.player.UniqueMultiplayerID, __instance.currentLocation);
                        }
                        else
                        {
                            junimoHarvester.tryToAddItemToHut(ItemRegistry.Create<StardewValley.Object>(__instance.netSeedIndex.Value));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Crop.harvest", "postfixed", true);
            }
        }

        public static void SaveFarmAnimalProductData(FarmAnimal farmAnimal, StardewValley.Object product)
        {
            try
            {
                var data = farmAnimal.GetAnimalData();
                bool isDeluxe = false;
                bool hasGivenAnyAtAll = false;
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
                CoreUtility.PrintError(e, PatcherName, "FarmAnimal.dayUpdate", "transpiled", true);
            }
        }

        public static void GetModifiedRadiusForSprinkler_Postfix(ref int __result, StardewValley.Object __instance)
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
                CoreUtility.PrintError(e, PatcherName, "StardewValley.Object.GetModifiedRadiusForSprinkler", "postfixed", true);
            }
        }

        public static void ChooseCrop_Postfix(Vector2 __result)
        {
            try
            {
                if (TalentUtility.AllPlayersHaveTalent("FaeBlessings"))
                {
                    Game1.getFarm().modData[TalentCore.Key_FaeBlessings] = $"{__result.X}+{__result.Y}";
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FairyEvent.ChooseCrop", "postfixed", true);
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
                    if (item.opcode == OpCodes.Stloc_S && (item.operand is 14 || (item.operand as LocalBuilder).LocalIndex == 14))
                    {
                        list.Insert(index - 1, new(OpCodes.Ldloc_S, 13));
                        list.Insert(index, new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(TryOverridePhaseGrowth))));
                        list.Insert(index + 1, new(OpCodes.Brfalse_S, list[index - 2].operand));
                        break;
                    }
                    index++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Farm.addCrows", "transpiling");
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
                CoreUtility.PrintError(e, PatcherName, "Farm.addCrows", "transpiled", true);
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> performLightningUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            try
            {
                int expected_ldloc0s = 3;
                int index = 0;
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
                            pair.operand = AccessTools.Method(PatcherType, nameof(ShouldFarmBeProtected));
                            list[index].opcode = OpCodes.Brfalse_S;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.performLightningUpdate", "transpiling");
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
                CoreUtility.PrintError(e, PatcherName, "GameLocation.performLightningUpdate", "transpiled", true);
            }
            //Return this weird calculation that vanilla does
            return Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0;
        }

        public static float ReturnBirthChance() => TalentUtility.CurrentPlayerHasTalent("BrimmingWithLife") ? 0.011f : 0.0055f;
        
        public static IEnumerable<CodeInstruction> setUp_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> insns = new List<CodeInstruction>();
            try
            {
                foreach (var codeInstruction in instructions)
                {
                    if (codeInstruction.OperandIs(0.0055f))
                    {
                        insns.Add(new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(ReturnBirthChance))));
                        continue;
                    }
                    insns.Add(codeInstruction);
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "QuestionEvent.setUp", "transpiled", true);
            }
            return insns;
        }
    }
}