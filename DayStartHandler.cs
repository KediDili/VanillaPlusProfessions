using System;
using System.Collections.Generic;
using System.Linq;
using VanillaPlusProfessions.Managers;
using StardewModdingAPI.Events;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions
{
    internal class DayStartHandler
    {
        internal static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            DisplayHandler.ShouldHandleSkillPage.Value = true;
            ComboManager.StonesBroken = 0;

            if (CoreUtility.CurrentPlayerHasProfession(39))
                FishingRod.maxTackleUses = 40;

            if (CoreUtility.CurrentPlayerHasProfession(40))
                FishingRod.baseChanceForTreasure = 1;

            if (CoreUtility.AnyPlayerHasProfession(74))
            {
                var list = Game1.getOnlineFarmers();

                IList<string> list2 = (from obj in DataLoader.Objects(Game1.content)
                                       where obj.Value.Category is StardewValley.Object.GreensCategory && !obj.Value.ContextTags.Contains("smp_forageThrowGame_banned")
                                       select "(O)" + obj.Key).ToList();

                string chosenNewForage = Game1.random.ChooseFrom(list2);

                foreach (var farmer in list)
                {
                    if (farmer.modData.TryGetValue(ModEntry.Key_DaysLeftForForageGuess, out string vall))
                        farmer.modData[ModEntry.Key_DaysLeftForForageGuess] = vall is "0" ? "4" : (int.Parse(vall) - 1).ToString();

                    else
                        farmer.modData.TryAdd(ModEntry.Key_DaysLeftForForageGuess, "4");

                    if (farmer.modData[ModEntry.Key_DaysLeftForForageGuess] is "4")
                    {
                        if (!farmer.modData.TryAdd(ModEntry.Key_HasFoundForage, "false"))
                            farmer.modData[ModEntry.Key_HasFoundForage] = "false";

                        if (!farmer.modData.TryAdd(ModEntry.Key_ForageGuessItemID, chosenNewForage))
                            farmer.modData[ModEntry.Key_ForageGuessItemID] = chosenNewForage;

                        if (!Game1.doesHUDMessageExist("Forage bubble minigame has been reset."))
                            Game1.addHUDMessage(new("Forage bubble minigame has been reset.", HUDMessage.newQuest_type));
                    }
                }
            }
            if (CoreUtility.CurrentPlayerHasProfession(36))
            {
                var loc = Game1.getLocationFromName("Greenhouse");
                foreach (var item in loc.terrainFeatures.Values)
                {
                    if (item is FruitTree tree)
                    {
                        for (int i = 0; i < tree.fruit.Count; i++)
                            tree.fruit[i].Quality = 4;
                    }
                }
            }
            if (CoreUtility.CurrentPlayerHasProfession(33) || CoreUtility.AnyPlayerHasProfession(78) || CoreUtility.AnyPlayerHasProfession(76))
            {
                Utility.ForEachBuilding<Building>(building =>
                {
                    if (CoreUtility.CurrentPlayerHasProfession(33))
                    {
                        if (building.GetIndoors() is AnimalHouse animalHouse)
                        {
                            foreach (var (id, animal) in animalHouse.Animals.Pairs)
                            {
                                if (!animalHouse.animalsThatLiveHere.Contains(id))
                                    continue;
                                if (Game1.random.NextBool(0.35))
                                    animal.fullness.Value = 255;
                            }
                        }
                    }
                    if (CoreUtility.AnyPlayerHasProfession(78))
                    {
                        if (building.GetIndoors() is SlimeHutch slimeHutch)
                        {
                            foreach (var item in slimeHutch.characters)
                            {
                                if (item is GreenSlime slime)
                                {
                                    IList<Vector2> nullobjs = (from obj in slimeHutch.Objects.Pairs
                                                               where obj.Value is null
                                                               select obj.Key).ToList();
                                    int number = 0;

                                    if (ManagerUtility.IsSlimeWhite(slime) && Game1.random.NextBool(0.15))
                                    {
                                        slime.makePrismatic();
                                    }
                                    else if (slime.prismatic.Value && Game1.random.NextBool(0.15) && number < 3)
                                    {
                                        Vector2 key1 = Game1.random.ChooseFrom(nullobjs);
                                        slimeHutch.Objects[key1] = ItemRegistry.Create<StardewValley.Object>("Kedi.SMP.FakePrismaticJelly");
                                        number++;
                                    }
                                    else if (slime.hasSpecialItem.Value && number < 3)
                                    {
                                        Vector2 key1 = Game1.random.ChooseFrom(nullobjs);
                                        IList<string> randobjs = new List<string>() { "60", "62", "64", "66", "68", "70", "72" };
                                        slimeHutch.Objects[key1] = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(randobjs));
                                        number++;
                                    }
                                    else if (slime.modData.TryGetValue(ModEntry.Key_SlimeWateredDaysSince, out string value))
                                    {
                                        if (int.TryParse(value, out int val) && val > 7 && Game1.random.NextBool(0.15) && number < 3)
                                        {
                                            Vector2 key1 = Game1.random.ChooseFrom(nullobjs);
                                            slimeHutch.Objects[key1] = ManagerUtility.CreateColoredPetrifiedSlime(slime.color.Value);
                                            number++;
                                        }
                                        slime.modData[ModEntry.Key_SlimeWateredDaysSince] = slimeHutch.modData.TryGetValue(ModEntry.Key_IsSlimeHutchWatered, out string wall) && wall == "false" ? (++val).ToString() : "0";
                                    }
                                    else
                                    {
                                        slime.modData.TryAdd(ModEntry.Key_SlimeWateredDaysSince, "0");
                                    }
                                }
                            }
                        }
                    }
                    if (CoreUtility.AnyPlayerHasProfession(76))
                    {
                        if (building is FishPond pond && pond.currentOccupants == pond.maxOccupants)
                        {
                            var data = pond.GetFishPondData();
                            if (data.PopulationGates?.Count! > 0)
                                return true;
                            if (pond.modData.TryGetValue(ModEntry.Key_FishRewardOrQuestDayLeft, out string value) && value is not null)
                            {
                                if (value is not "0" && pond.neededItem.Value is null && pond.neededItemCount.Value is -1)
                                {
                                    if (pond.output is not null)
                                        pond.output.Value.Stack *= 2;
                                    pond.modData[ModEntry.Key_FishRewardOrQuestDayLeft] = (int.Parse(value) - 1).ToString();
                                    return true;
                                }
                                else if (value is "0" && pond.neededItem.Value is not null)
                                    pond.neededItem.Value = null;
                            }
                            else
                            {
                                pond.modData.TryAdd(ModEntry.Key_FishRewardOrQuestDayLeft, "6");
                            }
                            if (pond.neededItem.Value == null)
                            {
                                pond.hasCompletedRequest.Value = false;
                                IList<List<string>> list = data.PopulationGates.Values.ToList();
                                var newQuestlist = Game1.random.ChooseFrom(list);
                                var ActualQuest = ArgUtility.SplitBySpace(Game1.random.ChooseFrom(newQuestlist));
                                pond.neededItem.Value = ItemRegistry.Create(ActualQuest[0]);
                                pond.neededItemCount.Value = int.Parse(ActualQuest[1]);
                            }
                        }
                    }
                    return true;
                });
            }
            if (CoreUtility.AnyPlayerHasProfession(43) || CoreUtility.AnyPlayerHasProfession(45) || CoreUtility.AnyPlayerHasProfession(49) || CoreUtility.AnyPlayerHasProfession(70))
            {
                StardewValley.Utility.ForEachItem(item =>
                {
                    if (item is not null and StardewValley.Object bigcraftable)
                    {
                        if (CoreUtility.AnyPlayerHasProfession(43) || CoreUtility.AnyPlayerHasProfession(45))
                        {
                            if (bigcraftable is CrabPot crabPot && crabPot.heldObject.Value is StardewValley.Object obj)
                            {
                                if (CoreUtility.AnyPlayerHasProfession(43))
                                    obj.Quality = Game1.random.NextBool(0.6) ? 2 : 4;

                                if (CoreUtility.AnyPlayerHasProfession(45))
                                    obj.Stack *= Game1.random.NextBool(0.6) ? 1 : 2;

                                return true;
                            }
                        }
                        if (CoreUtility.AnyPlayerHasProfession(70) || CoreUtility.AnyPlayerHasProfession(49))
                        {
                            if (bigcraftable.IsTapper())
                            {
                                if (CoreUtility.AnyPlayerHasProfession(70) && bigcraftable.modData.TryGetValue(ModEntry.Key_TFTapperDaysLeft, out string value) && value is not "0")
                                {
                                    bigcraftable.modData[ModEntry.Key_TFTapperDaysLeft] = (Convert.ToInt32(value) - 1).ToString();
                                    if (bigcraftable.modData[ModEntry.Key_TFTapperDaysLeft] is "0")
                                        bigcraftable.heldObject.Value = ManagerUtility.CreateFlavoredSyrupOrDust(bigcraftable.lastInputItem.Value as StardewValley.Object);
                                }
                                if (CoreUtility.AnyPlayerHasProfession(49) && bigcraftable.heldObject.Value is not null && bigcraftable.Location.terrainFeatures.TryGetValue(bigcraftable.TileLocation, out TerrainFeature terrainFeature) && terrainFeature is Tree or FruitTree or GiantCrop)
                                {
                                    bigcraftable.heldObject.Value.Stack += Game1.random.Next(1, 3);
                                }
                                return true;
                            }
                        }
                    }
                    return true;
                });
            }
            ModEntry.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
            ModEntry.Helper.GameContent.InvalidateCache("Data/Machines");
            ModEntry.Helper.GameContent.InvalidateCache("Data/FishPondData");
            ModEntry.Helper.GameContent.InvalidateCache("Data/Weapons");
            ModEntry.Helper.GameContent.InvalidateCache("Data/FarmAnimals");
            ModEntry.Helper.GameContent.InvalidateCache("Data/CraftingRecipes");
            ModEntry.Helper.GameContent.InvalidateCache("Data/Locations");
            ModEntry.Helper.GameContent.InvalidateCache("Data/Objects");
        }
    }
}
