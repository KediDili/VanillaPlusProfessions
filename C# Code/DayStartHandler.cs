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
using VanillaPlusProfessions.Talents;
using StardewValley.GameData.GiantCrops;
using VanillaPlusProfessions.Utilities;
using xTile.Dimensions;
using VanillaPlusProfessions.Craftables;
using StardewValley.GameData.FruitTrees;

namespace VanillaPlusProfessions
{
    internal class DayStartHandler
    {
        internal static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            DisplayHandler.CoreDisplayHandler.Value.ShouldHandleSkillPage = true;
            ComboManager.StonesBroken.Value = 0;
            DisplayHandler.CoreDisplayHandler.Value.WasSkillMenuRaised = false;
            TalentCore.IsDayStartOrEnd = true;

            CraftableHandler.OnDayStarted();
            foreach (var feederLoc in MachineryEventHandler.BirdsOnFeeders.Keys)
            {
                var location = Game1.getLocationFromName(feederLoc);
                if (location.modData.TryGetValue(Constants.Key_WasRainingHere, out string val) && val.ToLower() == "true")
                    continue;
                foreach (var item in location.Objects.Values.Select( obj => { return obj.QualifiedItemId == Constants.Id_BirdFeeder && obj.lastInputItem.Value != null ? obj : null; }))
                {
                    if (item is not null)
                    {
                        int count = item.modData.ContainsKey(Constants.Key_BirdFeederTime) ? int.Parse(item.modData[Constants.Key_BirdFeederTime]) : 0;
                        (++count).ToString();
                        if (count >= 7)
                        {
                            item.lastInputItem.Value = null;
                            item.showNextIndex.Value = false;
                            count = 0;
                        }
                        item.modData[Constants.Key_BirdFeederTime] = count.ToString();
                    }
                }
            }
            MachineryEventHandler.BirdsOnFeeders.Clear();
            ModEntry.EmptyCritterRoom ??= Game1.getLocationFromNameInLocationsList("KediDili.VPPData.CP_EmptyCritterRoom");
            TalentCore.VoidButterflyLocation = Game1.random.ChooseFrom(Constants.VoidButterfly_Locations);

            bool RefreshingWaters = TalentUtility.CurrentPlayerHasTalent(Constants.Talent_RefreshingWaters),
            Caretaker = CoreUtility.AnyPlayerHasProfession(Constants.Profession_Caretaker), 
            WildGrowth = TalentUtility.AnyPlayerHasTalent(Constants.Talent_WildGrowth),
            Trawler = CoreUtility.AnyPlayerHasProfession(Constants.Profession_Trawler),
            Hydrologist = CoreUtility.AnyPlayerHasProfession(Constants.Profession_Hydrologist),
            FishTrap = TalentUtility.AnyPlayerHasTalent(Constants.Talent_FishTrap),
            Diversification = TalentUtility.AnyPlayerHasTalent(Constants.Talent_Diversification),
            DeadMansChest = TalentUtility.AnyPlayerHasTalent(Constants.Talent_DeadMansChest),
            FarmForage = CoreUtility.AnyPlayerHasProfession(Constants.Profession_FarmForage),
            CombatFarm = CoreUtility.AnyPlayerHasProfession(Constants.Profession_CombatFarm),
            FishFarm = CoreUtility.AnyPlayerHasProfession(Constants.Profession_FishFarm),
            MiniFridgeBigSpace = TalentUtility.AnyPlayerHasTalent(Constants.Talent_MiniFridgeBigSpace),
            HarmoniousBlooming = TalentUtility.AnyPlayerHasTalent(Constants.Talent_HarmoniousBlooming),
            CrabRave = TalentUtility.HostHasTalent(Constants.Talent_CrabRave) && Game1.player.isWearingRing("810"),
            GoodSoaking = TalentUtility.AnyPlayerHasTalent(Constants.Talent_GoodSoaking);

            foreach (var item in Game1.player.Items)
            {
                if (item is WateringCan can && !can.IsBottomless)
                {
                    if (RefreshingWaters)
                    {
                        can.WaterLeft = can.waterCanMax;
                    }
                    TalentCore.TalentCoreEntry.Value.HasWaterCan = true;
                    can.modData[Constants.Key_Resurgence] = "0";
                    break;
                }
            }
        
            if (GoodSoaking)
            {
                Utility.ForEachLocation(loc =>
                {
                    if (loc.modData.TryGetValue(Constants.Key_WasRainingHere, out string value) && value is "true")
                    {
                        foreach (var item in loc.terrainFeatures.Pairs)
                        {
                            if (item.Value is HoeDirt dirt)
                            {
                                dirt.state.Value = 1;
                                dirt.updateNeighbors();
                            }
                        }
                    }
                    return true;
                }, false, false);
            }
            MachineryEventHandler.DrillLocations = new();
            MachineryEventHandler.ThermalReactorLocations = new();
            MachineryEventHandler.NodeMakerLocations = new();

            Utility.ForEachLocation(location => HandleForEachLocation(location, GoodSoaking), true, false);

            if (Game1.getFarm().modData.TryGetValue(Constants.Key_FaeBlessings, out string value))
            {
                string[] strings = value.Split('+');
                Vector2 vector = new(int.Parse(strings[0]), int.Parse(strings[1]));
                if (Game1.getFarm().terrainFeatures.TryGetValue(vector, out TerrainFeature terrainFeature) && terrainFeature is HoeDirt dirt && dirt.crop is Crop crop)
                {
                    Game1.getFarm().modData[Constants.Key_FaeBlessings] = "0+0";
                    HandleCropFairy(crop);
                }
            }
            if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_HiddenBenefits))
            {
                Utility.ForEachCrop(crop =>
                {
                    if (crop.modData.TryGetValue(Constants.Key_HiddenBenefit_Crop, out string val) && val == "true")
                    {
                        crop.growCompletely();
                        crop.modData[Constants.Key_HiddenBenefit_Crop] = "false";
                    }
                    return true;
                });
            }
            if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Artificer))
                FishingRod.maxTackleUses = 40;

            if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Plunderer))
                FishingRod.baseChanceForTreasure = 1;

            if (CoreUtility.AnyPlayerHasProfession(Constants.Profession_ForageFish))
            {
                var list = Game1.getOnlineFarmers();

                IList<string> list2 = (from obj in Game1.objectData
                                       where TalentUtility.EligibleForForagePerks(obj.Key, Constants.Profession_ForageFish)
                                       select obj.Key).ToList();

                string chosenNewForage = Game1.random.ChooseFrom(list2);

                foreach (var farmer in list)
                {
                    if (farmer.modData.TryGetValue(Constants.Key_DaysLeftForForageGuess, out string vall))
                        farmer.modData[Constants.Key_DaysLeftForForageGuess] = vall is "0" ? "4" : (int.Parse(vall) - 1).ToString();

                    else
                        farmer.modData.TryAdd(Constants.Key_DaysLeftForForageGuess, "4");

                    if (farmer.modData[Constants.Key_DaysLeftForForageGuess] is "4")
                    {
                        if (!farmer.modData.TryAdd(Constants.Key_HasFoundForage, "false"))
                            farmer.modData[Constants.Key_HasFoundForage] = "false";

                        if (!farmer.modData.TryAdd(Constants.Key_ForageGuessItemID, chosenNewForage))
                            farmer.modData[Constants.Key_ForageGuessItemID] = chosenNewForage;

                        if (!Game1.doesHUDMessageExist(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Message.ForageBubbleReset")))
                            Game1.addHUDMessage(new(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Message.ForageBubbleReset"), HUDMessage.newQuest_type));
                    }
                }
            }
            if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Horticulturist))
            {
                foreach (var location in Game1.locations)
                {
                    if (!location.IsGreenhouse)
                        continue;
                    foreach (var item in location.terrainFeatures.Values)
                    {
                        if (item is FruitTree tree) 
                        {
                            for (int i = 0; i < tree.fruit.Count; i++)
                                tree.fruit[i].Quality = 4;
                        }
                    }
                }
            }
            Utility.ForEachBuilding<Building>(building => HandleForEachBuilding(building, Caretaker, WildGrowth, CombatFarm, FishFarm));

            Utility.ForEachItem(item => HandleForEachItem(item, Trawler, Hydrologist, FishTrap, Diversification, DeadMansChest, CrabRave, FarmForage, HarmoniousBlooming, MiniFridgeBigSpace)); 
        }
        public static bool HandleForEachLocation(GameLocation location, bool GoodSoaking)
        {
            if (GoodSoaking && location.IsOutdoors)
            {
                if (location.modData.TryGetValue(Constants.Key_WasRainingHere, out string value) && value is "true")
                {
                    foreach (var item in location.terrainFeatures.Pairs)
                    {
                        if (item.Value is HoeDirt dirt)
                        {
                            dirt.state.Value = 1;
                            dirt.updateNeighbors();
                        }
                    }
                }
            }
            if (GameStateQuery.CheckConditions("KediDili.VanillaPlusProfessions_IsConsistentMineLocation " + location.NameOrUniqueName))
            {
                List<Vector2> TileLocations = new();
                foreach (var item in location.Objects.Values)
                {
                    if (item.ItemId == Constants.Id_ProgrammableDrill)
                    {
                        TileLocations.Add(item.TileLocation);
                    }
                }
                if (TileLocations.Count > 0)
                {
                    MachineryEventHandler.DrillLocations.Add(location.NameOrUniqueName, TileLocations);
                }
            }
            if (GameStateQuery.CheckConditions("KediDili.VanillaPlusProfessions_IsLavaLocation " + location.NameOrUniqueName))
            {
                List<Vector2> TileLocations = new();
                foreach (var item in location.Objects.Values)
                {
                    if (item.ItemId == Constants.Id_ThermalReactor)
                    {
                        TileLocations.Add(item.TileLocation);
                    }
                }
                if (TileLocations.Count > 0)
                {
                    MachineryEventHandler.ThermalReactorLocations.Add(location.NameOrUniqueName, TileLocations);
                }
            }
            List<Vector2> TileLocations2 = new();
            foreach (var item in location.Objects.Values)
            {
                if (item.ItemId == Constants.Id_NodeMaker)
                {
                    TileLocations2.Add(item.TileLocation);
                }
            }
            if (TileLocations2.Count > 0)
            {
                MachineryEventHandler.NodeMakerLocations.Add(location.NameOrUniqueName, TileLocations2);
            }

            return true;
        }

        public static bool HandleForEachBuilding(Building building, bool Caretaker, bool WildGrowth, bool CombatFarm, bool FishFarm)
        {
            if (Caretaker || WildGrowth)
            {
                if (building.GetIndoors() is AnimalHouse animalHouse)
                {
                    bool shouldUseAutoGrabber = false;
                    StardewValley.Object autoGrabber = null;

                    foreach (var (id, animal) in animalHouse.Animals.Pairs)
                    {
                        if (!animalHouse.animalsThatLiveHere.Contains(id))
                            continue;

                        if (Caretaker && Game1.random.NextBool(0.35))
                        {
                            animal.fullness.Value = 255;
                        }

                        if (WildGrowth && Game1.random.NextBool(ModEntry.CoreModEntry.Value.ModConfig.WildGrowth_Chance))
                        {
                            foreach (var item in animalHouse.Objects.Pairs)
                            {
                                if (item.Value is not null and StardewValley.Object obj_Autograbber && obj_Autograbber.QualifiedItemId == "(BC)165")
                                {
                                    shouldUseAutoGrabber = true;
                                    autoGrabber = item.Value;
                                    break;
                                }
                            }
                            if (animal.modData.TryGetValue(Constants.Key_WildGrowth, out string value) && value is not null && animal.GetHarvestType() == StardewValley.GameData.FarmAnimals.FarmAnimalHarvestType.DropOvernight)
                            {
                                var data = animal.GetAnimalData();
                                if (data is null)
                                    continue;

                                if (value is "true" && data.ProduceItemIds?.Any() is true)
                                {
                                    var obj = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(data.ProduceItemIds).ItemId);
                                    obj.CanBeSetDown = false;
                                    obj.Quality = animal.produceQuality.Value;
                                    if (animal.hasEatenAnimalCracker.Value)
                                        obj.Stack = 2;
                                    if (shouldUseAutoGrabber && autoGrabber is not null)
                                    {
                                        if (autoGrabber.heldObject.Value is Chest chest && chest?.addItem(obj) is null)
                                            autoGrabber.showNextIndex.Value = true;
                                    }
                                    else
                                    {
                                        var tile = animal.Tile;
                                        Utility.spawnObjectAround(tile, obj, animalHouse);
                                    }
                                }
                                else if (value is "false" && data.DeluxeProduceItemIds?.Any() is true)
                                {
                                    var obj = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(data.DeluxeProduceItemIds).ItemId);
                                    obj.CanBeSetDown = false;
                                    obj.Quality = animal.produceQuality.Value;
                                    if (animal.hasEatenAnimalCracker.Value)
                                        obj.Stack = 2;
                                    if (shouldUseAutoGrabber && autoGrabber is not null)
                                    {
                                        if (autoGrabber.heldObject.Value is Chest chest && chest.addItem(obj) is null)
                                            autoGrabber.showNextIndex.Value = true;
                                    }
                                    else
                                    {
                                        var tile = animal.Tile;
                                        Utility.spawnObjectAround(tile, obj, animalHouse);
                                    }
                                }
                                animal.modData[Constants.Key_WildGrowth] = "none";
                            }
                        }
                    }
                    if (shouldUseAutoGrabber)
                    {
                        foreach (var item in animalHouse.Objects.Pairs)
                        {
                            if (item.Value is not null and StardewValley.Object obj_Autograbber && obj_Autograbber.QualifiedItemId == "(BC)165")
                                item.Value.DayUpdate();
                        }
                    }
                }
            }
            if (CombatFarm)
            {
                if (building.GetIndoors() is SlimeHutch slimeHutch)
                {
                    int number = 0;
                    List<Vector2> nullobjs = new();
                    for (int XX = 0; XX < slimeHutch.Map.Layers[0].LayerWidth; XX++)
                    {
                        for (int YY = 0; YY < slimeHutch.Map.Layers[0].LayerHeight; YY++)
                        {
                            if (slimeHutch.isTilePlaceable(new(XX, YY), false) && !slimeHutch.Objects.ContainsKey(new(XX, YY)) && !slimeHutch.isTileOnWall(XX, YY) && slimeHutch.isTileLocationOpen(new Location(XX, YY)))
                                nullobjs.Add(new(XX, YY));
                        }
                    }
                    foreach (var item in slimeHutch.characters)
                    {
                        if (item is GreenSlime slime)
                        {
                            Random r = new();
                            if (ManagerUtility.IsSlimeWhite(slime) && r.NextBool(0.01))
                            {
                                slime.makePrismatic();
                            }
                            else if (slime.prismatic.Value && r.NextBool(0.05) && number < 3)
                            {
                                Vector2 key1 = Game1.random.ChooseFrom(nullobjs);
                                nullobjs.Remove(key1);
                                slimeHutch.Objects[key1] = ItemRegistry.Create<StardewValley.Object>("Kedi.VPP.FakePrismaticJelly");
                                slimeHutch.Objects[key1].CanBeGrabbed = true;
                                slimeHutch.Objects[key1].IsSpawnedObject = true;
                                number++;
                            }
                            else if (slime.hasSpecialItem.Value && number < 3)
                            {
                                Vector2 key1 = r.ChooseFrom(nullobjs);
                                IList<string> randobjs = new List<string>() { "60", "62", "64", "66", "68", "70", "72" };
                                nullobjs.Remove(key1);
                                slimeHutch.Objects[key1] = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(randobjs));
                                slimeHutch.Objects[key1].CanBeGrabbed = true;
                                slimeHutch.Objects[key1].IsSpawnedObject = true;
                                number++;
                            }
                            else if (slime.modData.TryGetValue(Constants.Key_SlimeWateredDaysSince, out string value) && !slime.prismatic.Value)
                            {
                                if (int.TryParse(value, out int val) && val > 7 && Game1.random.NextBool(0.15) && number < 3)
                                {
                                    Vector2 key1 = r.ChooseFrom(nullobjs);
                                    nullobjs.Remove(key1);
                                    slimeHutch.Objects[key1] = ManagerUtility.CreateColoredPetrifiedSlime(slime.color.Value);
                                    slimeHutch.Objects[key1].CanBeGrabbed = true; number++;
                                    slimeHutch.Objects[key1].IsSpawnedObject = true;
                                }
                                slime.modData[Constants.Key_SlimeWateredDaysSince] = slimeHutch.modData.TryGetValue(Constants.Key_IsSlimeHutchWatered, out string wall) && wall == "false" ? (++val).ToString() : "0";
                            }
                            else
                            {
                                slime.modData.TryAdd(Constants.Key_SlimeWateredDaysSince, "0");
                            }
                        }
                    }
                }
            }
            if (FishFarm)
            {
                if (building is FishPond pond && pond.currentOccupants.Value == pond.maxOccupants.Value)
                {
                    var data = pond.GetFishPondData();
                    if (data?.PopulationGates?.Count! > 0)
                        return true;
                    if (pond.modData.TryGetValue(Constants.Key_FishRewardOrQuestDayLeft, out string value) && value is not null)
                    {
                        if (value is not "0" && pond.neededItem.Value is null && pond.neededItemCount.Value is -1)
                        {
                            if (pond.output.Value is not null)
                                pond.output.Value.Stack *= 2;
                            pond.modData[Constants.Key_FishRewardOrQuestDayLeft] = (int.Parse(value) - 1).ToString();
                            return true;
                        }
                        else if (value is "0" && pond.neededItem.Value is not null)
                            pond.neededItem.Value = null;
                    }
                    else
                    {
                        pond.modData.TryAdd(Constants.Key_FishRewardOrQuestDayLeft, "6");
                    }
                    if (pond.neededItem.Value == null)
                    {
                        pond.hasCompletedRequest.Value = false;
                        List<List<string>> list = new();
                        if (pond.GetFishPondData()?.PopulationGates?.Values is null)
                            return true;

                        foreach (var pondData in pond.GetFishPondData()?.PopulationGates?.Values)
                            list.Add(pondData);
                        if (list.Count > 0)
                        {
                            var newQuestlist = Game1.random.ChooseFrom(list);
                            var ActualQuest = ArgUtility.SplitBySpace(Game1.random.ChooseFrom(newQuestlist));
                            pond.neededItem.Value = ItemRegistry.Create(ActualQuest[0]);
                            pond.neededItemCount.Value = int.Parse(ActualQuest[1]);
                        }
                    }
                }
            }
            BuildingHandler.OnDayStarted(building);
            return true;
        }

        public static bool HandleForEachItem(Item item, bool Trawler, bool Hydrologist, bool FishTrap, bool Diversification, bool DeadMansChest, bool CrabRave, bool FarmForage, bool HarmoniousBlooming, bool MiniFridgeBigSpace)
        {
            if (item is StardewValley.Object bigcraftable)
            {
                if (Trawler || Hydrologist || FishTrap || Diversification || DeadMansChest || CrabRave)
                {
                    if (bigcraftable is CrabPot crabPot)
                    {
                        Random r = new();
                        string bait = crabPot.bait.Value?.ItemId ?? "685";
                        Farmer who = Game1.GetPlayer(crabPot.owner.Value, true) ?? Game1.MasterPlayer;
                        StardewValley.Object @object = (StardewValley.Object)crabPot.Location.getFish(1f, bait, r.Next(1, 5), who, 5, crabPot.TileLocation);
                        do
                        {
                            @object = (StardewValley.Object)crabPot.Location.getFish(1f, bait, r.Next(1, 5), who, 5, crabPot.TileLocation);
                        } while (@object?.HasContextTag("fish_legendary") is true || @object?.HasContextTag("trash_item") is true); //so that you dont get legendaries in crabpots 

                        if (crabPot.heldObject.Value is null || crabPot.heldObject.Value?.HasContextTag("trash_item") is true)
                        {
                            if (FishTrap && r.NextBool(0.20))
                                crabPot.heldObject.Value = @object;
                            if (DeadMansChest && r.NextBool(0.1))
                                crabPot.heldObject.Value = ItemRegistry.Create("(O)275") as StardewValley.Object;
                            if (CrabRave && r.NextBool(0.1) && crabPot.owner.Value == Game1.player.UniqueMultiplayerID)
                                crabPot.heldObject.Value = ItemRegistry.Create("(O)" + Game1.random.ChooseFrom(new string[] { "715", "716", "717" })) as StardewValley.Object;
                        }
                        if (crabPot.heldObject.Value is not null && !crabPot.heldObject.Value.HasContextTag("trash_item") && crabPot.heldObject.Value.Category == StardewValley.Object.FishCategory)
                        {
                            if (Trawler && crabPot.bait.Value is not null && crabPot.heldObject.Value.Quality < 2)
                                crabPot.heldObject.Value.Quality = r.NextBool(0.7) ? 2 : 4;
                            if (crabPot.heldObject.Value.Stack < 5)
                            {
                                if (Diversification && crabPot.bait.Value?.QualifiedItemId is "(O)774")
                                    crabPot.heldObject.Value.Stack += 1;
                                if (Hydrologist)
                                    crabPot.heldObject.Value.Stack += r.NextBool(0.85) ? 0 : 1;
                            }
                        }

                        return true;
                    }
                }

                if (bigcraftable is not null && bigcraftable.Location is not null && bigcraftable.IsTapper() is true && FarmForage)
                {
                    TerrainFeature? TreeOrGiantCrop = null;
                    if (bigcraftable.Location.terrainFeatures.TryGetValue(bigcraftable.TileLocation, out TerrainFeature terrainFeature) && terrainFeature is FruitTree tree)
                    {
                        TreeOrGiantCrop = terrainFeature;
                    }
                    else if (bigcraftable.Location.resourceClumps?.Count > 0 is true)
                    {
                        foreach (var resourceClump in bigcraftable.Location.resourceClumps)
                        {
                            if (resourceClump is GiantCrop crop && crop.getBoundingBox().Contains(bigcraftable.TileLocation * 64))
                            {
                                TreeOrGiantCrop = crop;
                                break;
                            }
                        }
                    }
                    if (TreeOrGiantCrop is null) return true;
                    FruitTreeData fruitTreeData = (TreeOrGiantCrop as FruitTree)?.GetData();
                    GiantCropData giantCropData = (TreeOrGiantCrop as GiantCrop)?.GetData();
                    StardewValley.Object? ingredient = null;
                    if (fruitTreeData?.CustomFields?.TryGetValue(Constants.Key_FruitTreeOrGiantCrop, out string value) is true && !string.IsNullOrEmpty(value))
                    {
                        string[] splitted = value.Split(' ');
                        ingredient = ItemRegistry.Create<StardewValley.Object>(splitted[0], splitted.Length == 2 ? int.Parse(splitted[1]) : 1);
                        ingredient.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                    }
                    else if (giantCropData?.CustomFields?.TryGetValue(Constants.Key_FruitTreeOrGiantCrop, out value) is true && !string.IsNullOrEmpty(value))
                    {
                        ingredient = ItemRegistry.Create<StardewValley.Object>(value);
                        ingredient.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                    }
                    else
                    {
                        ManagerUtility.GetProduceTimeBasedOnPrice(TreeOrGiantCrop, out StardewValley.Object produce);
                        ingredient = produce;
                        ingredient?.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Kedi.VPP.FruitSyrup");
                    }
                    if (bigcraftable.heldObject.Value is null && ingredient is not null)
                    {
                        bigcraftable.heldObject.Value = ManagerUtility.CreateFlavoredSyrupOrDust(ingredient);
                        // subtract 1 since we're doing this on day start instead of on collection/placement
                        var days = 6;
                        days = ingredient.Price / 20 - 1;
                        bigcraftable.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, days);
                    }
                    else if (bigcraftable.readyForHarvest.Value)
                    {
                        if (bigcraftable.modData.TryGetValue(Constants.Key_TFTapperDaysLeft, out string value3))
                        {
                            if (Convert.ToInt32(value3) - 1 < 1)
                            {
                                bigcraftable.heldObject.Value.Stack++;
                                bigcraftable.heldObject.Value.FixStackSize();
                                bigcraftable.modData[Constants.Key_TFTapperDaysLeft] = ManagerUtility.GetProduceTimeBasedOnPrice(TreeOrGiantCrop, out StardewValley.Object _);
                            }
                            else
                            {
                                bigcraftable.modData[Constants.Key_TFTapperDaysLeft] = (Convert.ToInt32(value3) - 1).ToString();
                            }
                        }
                        else
                        {
                            bigcraftable.modData[Constants.Key_TFTapperDaysLeft] = ManagerUtility.GetProduceTimeBasedOnPrice(TreeOrGiantCrop, out StardewValley.Object _);
                        }

                    }
                    return true;
                }

                if (HarmoniousBlooming && bigcraftable.QualifiedItemId == Constants.Id_BeeHouse && bigcraftable.heldObject.Value is not null)
                {
                    int tiles = TalentUtility.FlowersInBeeHouseRange(bigcraftable.Location, bigcraftable.TileLocation);
                    if (Game1.random.NextBool(tiles * 0.05) && bigcraftable.heldObject.Value.Stack < 5)
                    {
                        bigcraftable.heldObject.Value.Stack += tiles / 6;
                        bigcraftable.heldObject.Value.FixStackSize();
                    }
                    return true;
                }
                if (bigcraftable is Chest chest && chest.QualifiedItemId == "(BC)217")
                {
                    chest.SpecialChestType = MiniFridgeBigSpace
                        ? Chest.SpecialChestTypes.BigChest
                        : chest.SpecialChestType;
                }
            }
            return true;
        }

        public static void HandleCropFairy(Crop crop)
        {
            if (crop.currentPhase.Value == crop.phaseDays.Count - 1 && crop.TryGetGiantCrops(out var possibleGiantCrops))
            {
                Farm farm = Game1.getFarm();
                foreach (KeyValuePair<string, GiantCropData> pair in possibleGiantCrops)
                {
                    string giantCropId = pair.Key;
                    GiantCropData giantCrop = pair.Value;
                    if (!GameStateQuery.CheckConditions(giantCrop.Condition, farm))
                    {
                        continue;
                    }
                    bool valid = true;
                    for (int y2 = (int)crop.Dirt.Tile.Y; y2 < (int)crop.Dirt.Tile.Y + giantCrop.TileSize.Y; y2++)
                    {
                        int x2 = (int)crop.Dirt.Tile.X;
                        while (x2 < (int)crop.Dirt.Tile.X + giantCrop.TileSize.X)
                        {
                            Vector2 v2 = new(x2, y2);
                            if (farm.terrainFeatures.TryGetValue(v2, out var terrainFeature2))
                            {
                                if (terrainFeature2 is HoeDirt dirt2)
                                {
                                    Crop crop2 = dirt2.crop;
                                    if (crop2?.indexOfHarvest.Value == crop.indexOfHarvest.Value)
                                    {
                                        x2++;
                                        continue;
                                    }
                                }
                            }
                            valid = false;
                            break;
                        }
                        if (!valid)
                        {
                            break;
                        }
                    }
                    if (!valid)
                    {
                        continue;
                    }
                    for (int y = (int)crop.Dirt.Tile.Y; y < (int)crop.Dirt.Tile.Y + giantCrop.TileSize.Y; y++)
                    {
                        for (int x = (int)crop.Dirt.Tile.X; x < (int)crop.Dirt.Tile.X + giantCrop.TileSize.X; x++)
                        {
                            Vector2 v = new(x, y);
                            ((HoeDirt)farm.terrainFeatures[v]).crop = null;
                        }
                    }
                    farm.resourceClumps.Add(new GiantCrop(giantCropId, crop.Dirt.Tile));
                    break;
                }
            }
        }
    }
}
