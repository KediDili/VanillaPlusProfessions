using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.Triggers;
using VanillaPlusProfessions.Compatibility;
using VanillaPlusProfessions.Craftables;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Talents.Patchers;

namespace VanillaPlusProfessions.Utilities
{
    public static class TalentUtility
    {
        private readonly static string[] GemNodes = new string[] { "2", "4", "6", "8", "10", "12", "14" };
        private readonly static string[] GeodeNodes = new string[] { "75", "76", "77" };
        private readonly static string[] OreNodes = new string[] { "290", "751", "764", "765", "95"};
        public readonly static string[] BlandStones = new string[] { "32", "668", "670", "34", "36", "38", "40", "42", "48", "50", "52", "54", "56", "58", "343", "450", "760", "845", "846", "844" };
        
        public readonly static string[] ValidTalentStatuses = new string[] { "Enabled", "Refunded", "Disabled" };
        
        public static List<string> ItemExtensions_GeodeNodeList = new();
        public static List<string> ItemExtensions_GemNodeList = new();

        public static void DataUpdates(Dictionary<string, string> talentStatuses)
        {
            //Alchemic Reversal/Over The Rainbow/Survival Cooking/Drift Fencing
            //Take It Slow/Upcycling/Camp Spirit/Spring Thaw/Accessorise/Hidden Benefits
            string[] recipeUpdaters = new string[] { "AlchemicReversal", "OverTheRainbow", "SurvivalCooking", "DriftFencing", "TakeItSlow",
            "Upcycling", "CampSpirit", "SpringThaw", "Accessorise", "HiddenBenefits"};
            bool updateRecipes = recipeUpdaters.Intersect(talentStatuses.Keys).Any();

            //Essence Infusion/Double Hook/Cold Press
            bool updateMachines = talentStatuses.ContainsKey("DoubleHook") || talentStatuses.ContainsKey("ColdPress");

            //Survival Cooking/Sugar Rush/Sap Sipper
            bool updateObjects = talentStatuses.ContainsKey("SurvivalCooking") || talentStatuses.ContainsKey("SugarRush") || talentStatuses.ContainsKey("SapSipper") ;

            //Trashed Treasure / Eye Spy
            bool updateGarbageCans = talentStatuses.ContainsKey("TrashedTreasure") || talentStatuses.ContainsKey("EyeSpy");

            //Fishery Grant/Monumental Discount
            bool updateBuildings = talentStatuses.ContainsKey("FisheryGrant") || talentStatuses.ContainsKey("MonumentalDiscount") || talentStatuses.ContainsKey("Overcrowding");

            //In The Weeds / Legendary Variety
            bool updateFishPonds = talentStatuses.ContainsKey("InTheWeeds") || talentStatuses.ContainsKey("LegendaryVariety");

            //Everyone's Best Friend
            if (talentStatuses.ContainsKey("EveryonesBestFriend"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data/NPCGiftTastes");
            }

            //Bookclub Bargains
            if (talentStatuses.ContainsKey("BookclubBargains"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data/Shops");
            }

            //Welcome To The Jungle
            if (talentStatuses.ContainsKey("WelcomeToTheJungle"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data/WildTrees");
            }

            //Vast Domain
            if (talentStatuses.ContainsKey("VastDomain"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data/Locations");
            }

            //Hidden Benefits
            if (talentStatuses.ContainsKey("HiddenBenefits"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data/TailoringRecipes");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/AnimalShop");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/SebastianRoom");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/HaleyHouse");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/SeedShop");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/AdventureGuild");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/JoshHouse");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/WizardHouseBasement");
            }

            //Sleep Under The Stars
            if (talentStatuses.ContainsKey("SleepUnderTheStars"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Maps/Beach");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/Mountain");
                ModEntry.Helper.GameContent.InvalidateCache("Maps/Forest");
            }

            if (updateObjects)
            {
                //Survival Cooking/Sugar Rush
                ModEntry.Helper.GameContent.InvalidateCache("Data/Objects");
            }
            if (updateGarbageCans)
            {
                //Trashed Treasure / Eye Spy
                ModEntry.Helper.GameContent.InvalidateCache("Data/GarbageCans");
            }

            if (updateBuildings)
            {
                //Fishery Grant/Monumental Discount/Overcrowding
                ModEntry.Helper.GameContent.InvalidateCache("Data/Buildings");
                if (talentStatuses.TryGetValue("Overcrowding", out string val))
                {
                    if (val == ValidTalentStatuses[0] || val == ValidTalentStatuses[2])
                    {
                        Utility.ForEachBuilding(building =>
                        {
                            if (building.GetIndoors() is AnimalHouse animalHouse && building.buildingType.Value is "Coop" or "Big Coop" or "Deluxe Coop" or "Barn" or "Big Barn" or "Deluxe Barn")
                            {
                                building.maxOccupants.Value = building.maxOccupants.Value switch
                                {
                                    4 => 5,
                                    8 => 10,
                                    12 => 15,
                                    _ => building.maxOccupants.Value,
                                };
                                animalHouse.animalLimit.Value = building.maxOccupants.Value;
                            }
                            return true;
                        });
                    }
                    else if (val == ValidTalentStatuses[1])
                    {
                        Utility.ForEachBuilding(building =>
                        {
                            if (building.GetIndoors() is AnimalHouse animalHouse && building.buildingType.Value is "Coop" or "Big Coop" or "Deluxe Coop" or "Barn" or "Big Barn" or "Deluxe Barn")
                            {
                                building.maxOccupants.Value = building.maxOccupants.Value switch
                                {
                                    5 => 4,
                                    10 => 8,
                                    15 => 12,
                                    _ => building.maxOccupants.Value,
                                };
                                animalHouse.animalLimit.Value = building.maxOccupants.Value;
                            }
                            return true;
                        });
                    }
                }
            }

            if (updateFishPonds)
            {
                //In The Weeds
                ModEntry.Helper.GameContent.InvalidateCache("Data/FishPondData");
            }

            if (updateMachines)
            {
                //Double Hook/Cold Press
                ModEntry.Helper.GameContent.InvalidateCache("Data/Machines");
            }

            if (updateRecipes)
            {
                TriggerActionManager.Raise("KediDili.VanillaPlusProfessions_UpdateRecipes");
            }
        }

        public static void ApplyExtraDamage(Monster monster, Farmer who, int damage)
        {
            if (monster is not BigSlime && monster.Health - damage >= 0)
            {
                monster.takeDamage(damage, 0, 0, false, 1, who);
                if (monster is not BigSlime && monster.Health < 0)
                {
                    monster.deathAnimation();
                    ModEntry.Helper.Reflection.GetMethod(who.currentLocation, "onMonsterKilled").Invoke(new object[] { who, monster, monster.GetBoundingBox(), false });
                }
            }
            else if (monster is not BigSlime && monster.Health - damage < 0)
            {
                // It has to NOT be one of mummy, armored bug, pupating grub, or a shelled rock crab using its shell OR has to have Warrior profession.
                if ((!(monster is Mummy || monster is Bug { isArmoredBug.Value: true } || monster is Grub { pupating.Value: true } || (monster is RockCrab crab && !crab.shellGone.Value && crab.Sprite.currentFrame % 4 == 0)) || CoreUtility.CurrentPlayerHasProfession("Warrior", useThisInstead: who)))
                {
                    monster.Health -= damage;
                    monster.deathAnimation();
                    ModEntry.Helper.Reflection.GetMethod(who.currentLocation, "onMonsterKilled").Invoke(new object[] { who, monster, monster.GetBoundingBox(), false });
                }
            }
            else
            {
                monster.Health -= damage;
                if (monster.Health <= 0 && monster is BigSlime)
                {
                    who.stats.SlimesKilled++;
                    monster.deathAnimation();
                    if (Game1.gameMode == 3 && Game1.random.NextDouble() < 0.75)
                    {
                        int toCreate = Game1.random.Next(2, 5);
                        for (int i = 0; i < toCreate; i++)
                        {
                            var slime = new GreenSlime(monster.Position, Game1.CurrentMineLevel);
                            slime.setTrajectory(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3));
                            slime.willDestroyObjectsUnderfoot = false;
                            slime.moveTowardPlayer(4);
                            slime.Scale = 0.75f + Game1.random.Next(-5, 10) / 100f;
                            slime.currentLocation = monster.currentLocation;
                            monster.currentLocation.characters.Add(slime);
                        }
                    }
                }
            }
        }

        public static void MakeFarmerInvincible(Farmer player)
        {
            player.temporarilyInvincible = true;
            player.temporaryInvincibilityTimer = 0;
            player.currentTemporaryInvincibilityDuration = 1200;
            player.flashDuringThisTemporaryInvincibility = true;
        }
        public static int FlowersInBeeHouseRange(GameLocation location, Vector2 startTileLocation)
        {
            List<Vector2> validCrops = new();
            List<Vector2> CheckTiles = GetTilesAroundBeeHouse(startTileLocation.X, startTileLocation.Y);
            if (location is not null)
            {
                for (int i = 0; i < CheckTiles.Count; i++)
                {
                    if (location.terrainFeatures.TryGetValue(startTileLocation, out var terrainFeature) && terrainFeature is HoeDirt dirt && dirt.crop != null)
                    {
                        ParsedItemData data = ItemRegistry.GetData(dirt.crop.indexOfHarvest.Value);
                        if (data != null && data.Category == StardewValley.Object.flowersCategory && dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && !dirt.crop.dead.Value)
                            validCrops.Add(dirt.crop.tilePosition);
                    }
                }
            }
            return validCrops.Count;
        }

        public static bool CurrentPlayerHasTalent(string flag, long farmerID = -1, Farmer who = null, bool ignoreDisabledTalents = true)
        {
            if (ModEntry.ModConfig.Value.ProfessionsOnly)
                return false;

            if (farmerID is not -1)
                who = Game1.GetPlayer(farmerID) ?? Game1.MasterPlayer;

            who ??= Game1.player;
            if (who is null)
                return false;

            if (!Context.IsWorldReady && who.mailReceived?.Count == 0)
                return false;

            flag = GetFlag(flag);
            bool returnValue = false;
            if (!ignoreDisabledTalents)
                returnValue = who.mailReceived.Contains(flag);
            else 
                returnValue = who.mailReceived.Contains(flag) && !who.mailReceived.Contains(flag + "_disabled");

            if (ModEntry.ModMonitor.IsVerbose || ModEntry.ModConfig.Value.DeveloperOrTestingMode)
                ModEntry.ModMonitor.Log($"Checked talent {flag}: {returnValue}", LogLevel.Warn);

            return returnValue;
        }

        internal static string GetFlag(string name) => TalentCore.Talents.TryGetValue(name, out Talent talent) ? talent.MailFlag : name;

        public static int GetStoneHealth(string ID)
        {
            return ID switch
            {
                "2" or "4" or "6" or "8" or "10" or "12" or "14" => 10,
                "95" => 25,
                _ => 1
            };
        }

        public static bool AllPlayersHaveTalent(string flag)
        {
            if (!Context.IsWorldReady || ModEntry.ModConfig.Value.ProfessionsOnly)
            {
                return false;
            }

            List<bool> bools = new();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (flag is not null)
                {
                    bools.Add(CurrentPlayerHasTalent(GetFlag(flag), who: farmer));
                }
            }
            return bools.Count > 0 && !bools.Contains(false);
        }
        public static bool ShouldCropGrowByOneDay(HoeDirt dirt, Crop crop)
        {
            bool Efflorescence = HostHasTalent("Efflorescence") && ItemRegistry.GetData(crop.GetData().HarvestItemId).IsErrorItem && ItemRegistry.GetData(crop.GetData().HarvestItemId).Category == StardewValley.Object.flowersCategory;
            bool Nourishing_Rain = HostHasTalent("NourishingRain") && dirt.Location.modData.TryGetValue(TalentCore.Key_WasRainingHere, out string value2) && value2 is "true";
            bool Tropical_Bliss = HostHasTalent("TropicalBliss") && dirt.Location.InIslandContext() && (crop.GetData()?.Seasons.Contains(Season.Summer) is true || crop.GetData()?.Seasons.Count > 1);
            bool Deluxe_Wild_Seeds = !CraftablePatcher.IsVPPForageCrop(crop, false) && crop.currentLocation.GetData()?.CustomFields?.ContainsKey("Kedi.VPP.ForestLocation") is true || crop.currentLocation is Forest or Woods;

            return Efflorescence || Nourishing_Rain || Tropical_Bliss || Deluxe_Wild_Seeds;
        }
        public static bool AnyPlayerHasTalent(string flag)
        {
            if (!Context.IsWorldReady || ModEntry.ModConfig.Value.ProfessionsOnly)
            {
                return false;
            }

            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (flag is not null)
                {
                    return CurrentPlayerHasTalent(GetFlag(flag), who: farmer);
                }
            }
            return false;
        }
        public static bool HostHasTalent(string flag) => CurrentPlayerHasTalent(GetFlag(flag), who: Game1.MasterPlayer);

        public static List<Vector2> GetTilesAroundBeeHouse(float xStart, float yStart)
        {
            return new()
            {
                new(xStart, yStart - 2),
                new(xStart - 1, yStart - 1),
                new(xStart, yStart - 1),
                new(xStart + 1, yStart - 1),
                new(xStart - 2, yStart),
                new(xStart - 1, yStart),
                new(xStart + 1, yStart),
                new(xStart + 2, yStart),
                new(xStart - 1, yStart + 1),
                new(xStart, yStart + 1),
                new(xStart + 1, yStart + 1),
                new(xStart, yStart + 2)
            };
        }

        public static void DetermineGeodeDrop(Item geode, bool update = true) //remove the update parameter
        {
            if (Utility.IsGeode(geode, true))
            {
                MiningPatcher.IsUpdating = true;
                Item drop = Utility.getTreasureFromGeode(geode);
                MiningPatcher.IsUpdating = false;
                geode.modData[TalentCore.Key_XrayDrop] = drop.QualifiedItemId;
            }
        }

        public static bool isFavoredMonster(Monster monster, Farmer who)
        {
            CustomMonsterData monsterData = null;
            foreach (var item in ModEntry.VanillaPlusProfessionsAPI.CustomMonsters)
            {
                if (item.Type.Equals(monster.GetType()))
                {
                    monsterData = item;
                    break;
                }
            }
            if (CurrentPlayerHasTalent("MonsterSpecialist", who: who))
            {
                if (CurrentPlayerHasTalent("Combat_Monster_Specialist_Ground", who: who))
                {
                    if (monsterData is not null)
                        return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Ground;
                    else
                        return monster is GreenSlime or BigSlime or Grub or Duggy or LavaLurk or Leaper;
                }
                else if (CurrentPlayerHasTalent("Combat_Monster_Specialist_Humanoid", who: who))
                {
                    if (monsterData is not null)
                        return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Humanoid;
                    else
                        return monster is RockGolem or Skeleton or Mummy or ShadowBrute or Shooter or ShadowShaman;
                }
                else if (CurrentPlayerHasTalent("Combat_Monster_Specialist_Flying", who: who))
                {
                    if (monsterData is not null)
                        return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Flying;
                    else
                        return monster is Bat or Ghost or AngryRoger or Serpent or BlueSquid or SquidKid or Fly;
                }
                else if (CurrentPlayerHasTalent("Combat_Monster_Specialist_Armoured", who: who))
                {
                    if (monsterData is not null)
                        return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Armoured;
                    else
                        return monster is DwarvishSentry or RockCrab or HotHead or MetalHead or DinoMonster;
                }
            }

            return false;
        }
        public static void GiftOfTheTalented_ApplyOrUnApply(Dictionary<string, string> talentStatuses)
        {
            if (talentStatuses["GiftOfTheTalented"] == ValidTalentStatuses[0])
            {
                TalentCore.GiveOrTakeStardropEffects = true;
            }
            else if (talentStatuses["GiftOfTheTalented"] == ValidTalentStatuses[1] || talentStatuses["GiftOfTheTalented"] == ValidTalentStatuses[2])
            {
                
                TalentCore.GiveOrTakeStardropEffects = false;
            }
            else
            {
                TalentCore.GiveOrTakeStardropEffects = null;
            }
        }

        internal static void OnItemBasedTalentBoughtOrRefunded(Dictionary<string, string> talentStatuses)
        {
            bool switchTrinketRings = false;
            bool switchSapEdibility = false;
            bool markContextTagsDirty_SugarRush = false;
            bool hasSurvivalCooking = CurrentPlayerHasTalent("SurvivalCooking");

            if (talentStatuses.TryGetValue("Accessorise", out string val) && val == ValidTalentStatuses[1])
                switchTrinketRings = true;
            
            if (talentStatuses.TryGetValue("SapSipper", out val))
                switchSapEdibility = true;
            
            if (talentStatuses.TryGetValue("SugarRush", out val) && (val == ValidTalentStatuses[0] || val == ValidTalentStatuses[2]))
                markContextTagsDirty_SugarRush = true;

            if (markContextTagsDirty_SugarRush || switchSapEdibility || switchTrinketRings)
            {
                Utility.ForEachItem(item =>
                {
                    if (item is StardewValley.Object obj)
                    {
                        if (obj.Category is StardewValley.Object.CookingCategory && markContextTagsDirty_SugarRush)
                        {
                            obj.MarkContextTagsDirty();
                        }
                        else if (obj.QualifiedItemId == "(O)92" && switchSapEdibility)
                        {
                            obj.Edibility = talentStatuses["SapSipper"] == ValidTalentStatuses[0] || talentStatuses["SapSipper"] == ValidTalentStatuses[2] ? 3 : -1;
                        }
                    }
                    else if (item is TrinketRing ring && switchTrinketRings)
                    {
                        Game1.player.team.returnedDonations.Add(ring.GetRingTrinket(true));
                        Game1.player.team.newLostAndFoundItems.Value = true;
                        item = item.ConsumeStack(1);
                    }
                    return true;
                });
                if (switchTrinketRings)
                {
                    foreach (var trinketRing in GetAllTrinketRings(Game1.player))
                    {
                        Game1.player.team.returnedDonations.Add(trinketRing.GetRingTrinket(true));
                        Game1.player.team.newLostAndFoundItems.Value = true;
                    }
                }
            }
        }

        public static bool IsBlandStone(this StardewValley.Object obj)
        {
            if (ModEntry.ItemExtensionsAPI is not null && ModEntry.ItemExtensionsAPI.IsResource(obj.ItemId, out int? _, out string itemDropped))
            {
                return itemDropped is "390" or "(O)390";
            }
            return BlandStones.Contains(obj.ItemId) || obj.HasContextTag(TalentCore.ContextTag_BlandStone);
        }

        public static string GetNodeForRoomAndPillar()
        {
            return Game1.random.ChooseFrom(OreNodes);
        }

        public static string GetIDOfRing(Trinket trinket)
        {
            TrinketData data = trinket.GetTrinketData();

            if (data.CustomFields?.TryGetValue(TalentCore.Key_AccessoriseRing, out string value) is true && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            ModEntry.ModMonitor.Log($"Cannot find ring ID for trinket {trinket.ItemId}. This should be reported to the author of the mod its coming from and not to KediDili.", LogLevel.Warn);
            return null;
        }

        public static int BuffDescriptionLength(string name)
        {
            int minimum_size = 272;
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                minimum_size = 384;
            }
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
            {
                minimum_size = 336;
            }
            return Math.Max(minimum_size, (int)Game1.dialogueFont.MeasureString(name ?? "").X);
        }
        public static List<TrinketRing> GetAllTrinketRings(Farmer who)
        {
            List<TrinketRing> result = new();
            if (ModEntry.WearMoreRingsAPI is not null)
            {
                for (int i = 0; i < ModEntry.WearMoreRingsAPI?.RingSlotCount(); i++)
                {
                    try
                    {
                        if (ModEntry.WearMoreRingsAPI?.GetRing(i) is not null)
                        {
                            if (ModEntry.WearMoreRingsAPI?.GetRing(i) is CombinedRing combinedRing && combinedRing is not null)
                            {
                                if (combinedRing.combinedRings[0] is TrinketRing)
                                    result.Add(combinedRing.combinedRings[0] as TrinketRing);

                                if (combinedRing.combinedRings[1] is TrinketRing)
                                    result.Add(combinedRing.combinedRings[1] as TrinketRing);
                            }
                            if (ModEntry.WearMoreRingsAPI?.GetRing(i) is TrinketRing trinketRing && trinketRing != null)
                                result.Add(trinketRing);
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
            else
            {
                if (who.rightRing.Value is TrinketRing ring1)
                {
                    result.Add(ring1);
                }
                else if (who.rightRing.Value is CombinedRing combinedRing1)
                {
                    if (combinedRing1.combinedRings[0] is TrinketRing)
                        result.Add(combinedRing1.combinedRings[0] as TrinketRing);

                    if (combinedRing1.combinedRings[1] is TrinketRing)
                        result.Add(combinedRing1.combinedRings[1] as TrinketRing);
                }
                if (who.leftRing.Value is TrinketRing ring2)
                {
                    result.Add(ring2);
                }
                else if (who.leftRing.Value is CombinedRing combinedRing2)
                {
                    if (combinedRing2.combinedRings[0] is TrinketRing)
                        result.Add(combinedRing2.combinedRings[0] as TrinketRing);

                    if (combinedRing2.combinedRings[1] is TrinketRing)
                        result.Add(combinedRing2.combinedRings[1] as TrinketRing);
                }
            }
            return result;
        }

        public static bool AccessoriseMachineRule(StardewValley.Object machine, GameLocation location, Farmer player)
        {
            Item inputItem = player.ActiveItem;
            if (inputItem is Trinket trinket)
            {
                if (!ModEntry.Helper.ModRegistry.IsLoaded("mushymato.TrinketTinker") || !GameStateQuery.CheckConditions($"mushymato.TrinketTinker_DIRECT_EQUIP_ONLY {trinket.ItemId}"))
                {
                    //get the new guid and give it to the trinket's moddata
                    trinket.modData.TryAdd(ModEntry.Key_RingTrinkets, Guid.NewGuid().ToString());

                    //Add the trinket to the global inventory
                    player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryID_RingTrinkets).Add(trinket);

                    //Create the trinket ring and give it the same guid.
                    TrinketRing output = new(trinket);
                    output.modData.Add(ModEntry.Key_RingTrinkets, trinket.modData[ModEntry.Key_RingTrinkets]);

                    //"Destroy" the input and give the ring
                    player.ActiveItem = null;
                    player.addItemToInventory(output);
                }
            }
            else if (inputItem is TrinketRing ring)
            {
                Trinket trinket1 = ring.GetRingTrinket(true);

                player.ActiveItem = null;
                player.addItemToInventory(trinket1);
            }
            return true;
        }

        public static void GemAndGeodeNodes(bool flag, List<Vector2> list, GameLocation mine)
        {
            bool success = false;
            Dictionary<Vector2, string> CoordsForMP = new();
            for (int i = 0; i < list.Count; i++)
            {
                string str = nodeID(flag, mine, out int? health);
                if (str is not "-1")
                {
                    var obj = ItemRegistry.Create<StardewValley.Object>(str);
                    //Do something proper for the stones
                    obj.setHealth(health.Value);
                    obj.MinutesUntilReady = health.Value;
                    obj.Flipped = Game1.random.NextBool();
                    mine.Objects[list[i]] = obj;
                    CoordsForMP.Add(list[i], obj.ItemId);
                    success = true;
                }
            }
            if (success && Context.IsMultiplayer && Context.HasRemotePlayers)
            {
                ModEntry.Helper.Multiplayer.SendMessage(CoordsForMP, ModEntry.Manifest.UniqueID + "/SwitchMineStones", new string[] { ModEntry.Manifest.UniqueID });
            }
        }

        private static bool ShouldAddToThePool(string node, bool flag, MineShaft mineShaft)
        {
            if (ModEntry.ItemExtensionsAPI.GetResourceData(node, false, out object resourceData))
            {
                string SpawnOnFloors = ModEntry.Helper.Reflection.GetProperty<string>(resourceData, "SpawnOnFloors").GetValue();
                double SpawnFrequency = ModEntry.Helper.Reflection.GetProperty<double>(resourceData, "SpawnFrequency").GetValue();
                
                if (SpawnOnFloors is not null)
                {
                    string[] strings = SpawnOnFloors.Split('-');
                    if (int.TryParse(strings[0], out int minFloor) && int.TryParse(strings[1], out int maxFloor))
                    {
                        if (mineShaft.mineLevel < minFloor || mineShaft.mineLevel > maxFloor)
                        {
                            return false;
                        }
                    }
                }
                if (Game1.random.NextBool(SpawnFrequency))
                {
                    return true;
                }
            }
            return false;
        }

        private static string nodeID(bool flag, GameLocation mine, out int? health)
        {
            List<string> GemNodeList = new();
            
            List<string> GeodeNodeList = new();

            MineShaft mineShaft = mine as MineShaft;

            health = null;
            string result = "-1";

            if (flag)
            {
                GemNodeList.AddRange(GemNodes);
                foreach (string gemNode in ItemExtensions_GemNodeList)
                {
                    if (ShouldAddToThePool(gemNode, flag, mineShaft))
                    {
                        GemNodeList.Add(gemNode);
                    }
                }
            }
            else
            {
                GeodeNodeList.AddRange(GeodeNodes);
                foreach (string geodeNode in ItemExtensions_GeodeNodeList)
                {
                    if (ShouldAddToThePool(geodeNode, flag, mineShaft))
                    {
                        GeodeNodeList.Add(geodeNode);
                    }
                }
            }

            if (mineShaft is not null)
            {
                result = mineShaft.getMineArea(mineShaft.mineLevel) switch
                {
                    10 or 0 => flag ? Game1.random.ChooseFrom(GemNodeList) : GeodeNodeList[0],
                    40 => flag ? Game1.random.ChooseFrom(GemNodeList) : GeodeNodeList[1],
                    80 => flag ? Game1.random.ChooseFrom(GemNodeList) : GeodeNodeList[2],
                    121 => Game1.random.ChooseFrom(flag ? GemNodeList : GeodeNodeList),
                    _ => "-1",
                };
            }
            else if (mine is VolcanoDungeon)
            {
                result = flag ? Game1.random.ChooseFrom(GemNodeList) : "819";
            }
            else
            {
                string locdata = Game1.player.currentLocation.GetData().CustomFields[flag ? TalentCore.Key_CrystalCavern : TalentCore.Key_Upheaval].Trim();
                if (!string.IsNullOrEmpty(locdata))
                {
                    string[] strings = locdata.Split('/', StringSplitOptions.TrimEntries);
                    result = Game1.random.ChooseFrom(strings);
                }
                else
                {
                    result = Game1.random.ChooseFrom(flag ? GemNodeList : GeodeNodeList);
                }
            }
            if (ItemExtensions_GemNodeList.Contains(result) || ItemExtensions_GeodeNodeList.Contains(result))
            {
                if (ModEntry.ItemExtensionsAPI.IsResource(result, out int? nodeHealth, out string _))
                {
                    health = nodeHealth;
                }
            }
            health ??= GetStoneHealth(result);
            return result;
        } 
    }
}
