using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using VanillaPlusProfessions.Compatibility;
using VanillaPlusProfessions.Craftables;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Talents.Patchers;

namespace VanillaPlusProfessions.Utilities
{
    public class TalentUtility
    {
        public static List<string> ItemExtensions_GeodeNodeList = new();
        public static List<string> ItemExtensions_GemNodeList = new();

        public static void DataUpdates(Dictionary<string, string> talentStatuses)
        {
            //Alchemic Reversal/Over The Rainbow/Survival Cooking/Drift Fencing
            //Take It Slow/Upcycling/Camp Spirit/Spring Thaw/Accessorise/Hidden Benefits
            string[] recipeUpdaters = new string[] { Constants.Talent_AlchemicReversal, Constants.Talent_OverTheRainbow, Constants.Talent_SurvivalCooking, Constants.Talent_DriftFencing, Constants.Talent_TakeItSlow,
            Constants.Talent_Upcycling, Constants.Talent_CampSpirit, Constants.Talent_SpringThaw, Constants.Talent_Accessorise, Constants.Talent_HiddenBenefits, Constants.Talent_Overcrowding };
            bool updateRecipes = recipeUpdaters.Intersect(talentStatuses.Keys).Any();

            //Essence Infusion/Double Hook/Cold Press
            bool updateMachines = talentStatuses.ContainsKey(Constants.Talent_DoubleHook) || talentStatuses.ContainsKey(Constants.Talent_ColdPress);

            //Survival Cooking/Sugar Rush/Sap Sipper
            bool updateObjects = talentStatuses.ContainsKey(Constants.Talent_SurvivalCooking) || talentStatuses.ContainsKey(Constants.Talent_SugarRush) || talentStatuses.ContainsKey(Constants.Talent_SapSipper);

            //Trashed Treasure / Eye Spy
            bool updateGarbageCans = talentStatuses.ContainsKey(Constants.Talent_TrashedTreasure) || talentStatuses.ContainsKey(Constants.Talent_EyeSpy);

            //Fishery Grant/Monumental Discount
            bool updateBuildings = talentStatuses.ContainsKey(Constants.Talent_FisheryGrant) || talentStatuses.ContainsKey(Constants.Talent_MonumentalDiscount) || talentStatuses.ContainsKey(Constants.Talent_Overcrowding) || talentStatuses.ContainsKey(Constants.Talent_BreedLikeRabbits);

            //In The Weeds / Legendary Variety
            bool updateFishPonds = talentStatuses.ContainsKey(Constants.Talent_InTheWeeds) || talentStatuses.ContainsKey(Constants.Talent_BigFishSmallPond);

            //Everyone's Best Friend
            if (talentStatuses.ContainsKey(Constants.Talent_EveryonesBestFriend))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/NPCGiftTastes");
            }

            //One Fish Two Fish
            if (talentStatuses.ContainsKey(Constants.Talent_OneFishTwoFish))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("LooseSprites/Cursors_1_6");
            }

            if (talentStatuses.ContainsKey(Constants.Talent_BreedLikeRabbits))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/FarmAnimals");
            }

            //Bookclub Bargains
            if (talentStatuses.ContainsKey(Constants.Talent_BookclubBargains))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/Shops");
            }

            //Welcome To The Jungle
            if (talentStatuses.ContainsKey(Constants.Talent_WelcomeToTheJungle))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/WildTrees");
            }

            //Vast Domain
            if (talentStatuses.ContainsKey(Constants.Talent_VastDomain))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/Locations");
            }

            //Hidden Benefits
            if (talentStatuses.ContainsKey(Constants.Talent_HiddenBenefits))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/TailoringRecipes");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/AnimalShop");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/SebastianRoom");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/HaleyHouse");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/SeedShop");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/AdventureGuild");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/JoshHouse");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/WizardHouseBasement");
            }

            //Sleep Under The Stars
            if (talentStatuses.ContainsKey(Constants.Talent_SleepUnderTheStars))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/Beach");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/Mountain");
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Maps/Forest");
            }

            if (updateObjects)
            {
                //Survival Cooking/Sugar Rush
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/Objects");
            }
            if (updateGarbageCans)
            {
                //Trashed Treasure / Eye Spy
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/GarbageCans");
            }

            if (updateBuildings)
            {
                //Fishery Grant/Monumental Discount/Overcrowding
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/Buildings");
                if (talentStatuses.TryGetValue(Constants.Talent_Overcrowding, out string val))
                {
                    if (val == Constants.ValidTalentStatuses[0] || val == Constants.ValidTalentStatuses[2])
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
                    else if (val == Constants.ValidTalentStatuses[1])
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
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/FishPondData");
            }

            if (updateMachines)
            {
                //Double Hook/Cold Press
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/Machines");
            }

            if (updateRecipes)
            {
                TriggerActionManager.Raise("KediDili.VanillaPlusProfessions_UpdateRecipes");
            }
        }
        
        public static List<KeyValuePair<string, ObjectData>> FilterObjectData(List<int> anyOfTheseCategories, List<string> includeTags = null, List<string> excludeTags = null)
        {
            return (from sds in Game1.objectData
                    where anyOfTheseCategories.Contains(sds.Value.Category) && (sds.Value.ContextTags is null || (sds.Value.ContextTags is not null && !excludeTags.Intersect(sds.Value.ContextTags).Any() && (includeTags is not null || includeTags?.Intersect(sds.Value.ContextTags).Any() is true)))
                    select sds).ToList();
        }

        public static void ApplyExtraDamage(Monster monster, Farmer who, int damage)
        {
            if (monster is not BigSlime && monster.Health - damage >= 0)
            {
                monster.takeDamage(damage, 0, 0, false, 1, who);
                if (monster is not BigSlime && monster.Health < 0)
                {
                    monster.deathAnimation();
                    ModEntry.CoreModEntry.Value.Helper.Reflection.GetMethod(who.currentLocation, "onMonsterKilled").Invoke(new object[] { who, monster, monster.GetBoundingBox(), false });
                }
            }
            else if (monster is not BigSlime && monster.Health - damage < 0)
            {
                // It has to NOT be one of mummy, armored bug, pupating grub, or a shelled rock crab using its shell OR has to have Warrior profession.
                if ((!(monster is Mummy || monster is Bug { isArmoredBug.Value: true } || monster is Grub { pupating.Value: true } || (monster is RockCrab crab && !crab.shellGone.Value && crab.Sprite.currentFrame % 4 == 0)) || CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Warrior, useThisInstead: who)))
                {
                    monster.Health -= damage;
                    monster.deathAnimation();
                    ModEntry.CoreModEntry.Value.Helper.Reflection.GetMethod(who.currentLocation, "onMonsterKilled").Invoke(new object[] { who, monster, monster.GetBoundingBox(), false });
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
                    if (location.terrainFeatures.TryGetValue(CheckTiles[i], out var terrainFeature) && terrainFeature is HoeDirt dirt && dirt.crop != null)
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
            if (ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly)
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

            if (ModEntry.CoreModEntry.Value.ModMonitor.IsVerbose || ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode)
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Checked talent {flag}: {returnValue}", LogLevel.Warn);

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
            if (!Context.IsWorldReady || ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly)
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
            bool Efflorescence = HostHasTalent(Constants.Talent_Efflorescence) && ItemRegistry.GetData(crop.GetData().HarvestItemId).IsErrorItem && ItemRegistry.GetData(crop.GetData().HarvestItemId).Category == StardewValley.Object.flowersCategory && EligibleForCropPerks(crop.netSeedIndex.Value, Constants.Talent_Efflorescence);
            bool Nourishing_Rain = HostHasTalent(Constants.Talent_NourishingRain) && dirt.Location.modData.TryGetValue(Constants.Key_WasRainingHere, out string value2) && value2 is "true" && EligibleForCropPerks(crop.netSeedIndex.Value, Constants.Talent_NourishingRain);
            bool Tropical_Bliss = HostHasTalent(Constants.Talent_TropicalBliss) && dirt.Location.InIslandContext() && (crop.GetData()?.Seasons.Contains(Season.Summer) is true || crop.GetData()?.Seasons.Count > 1) && EligibleForCropPerks(crop.netSeedIndex.Value, Constants.Talent_TropicalBliss);
            bool Deluxe_Wild_Seeds = !CraftablePatcher.IsVPPForageCrop(crop, false) && crop.currentLocation.GetData()?.CustomFields?.ContainsKey("Kedi.VPP.ForestLocation") is true || crop.currentLocation is Forest or Woods;

            return Efflorescence || Nourishing_Rain || Tropical_Bliss || Deluxe_Wild_Seeds;
        }

        public static bool EligibleForGeodePerks(string item, string perk, bool role) //role: True for gives drop, false for becomes dropped
        {
            if (ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Geode))
                return false;

            switch (perk)
            {
                case Constants.Talent_Matryoshka:
                    return !ItemContextTagManager.HasBaseTag(item, role ? Constants.ContextTag_Matryoshka_Banned_FromDropping : Constants.ContextTag_Matryoshka_Banned_FromBeingDropped);
                case Constants.Talent_Xray:
                    return !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Xray);
            }
            return false;
        }

        public static bool EligibleForForagePerks(string item, string perk)
        {
            if (ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Forage))
                return false;
            return perk switch
            {
                Constants.Profession_Ranger => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Ranger),
                Constants.Profession_Adventurer => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Adventurer),
                Constants.Talent_NatureSecrets => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_NatureSecrets),
                Constants.Id_WildTotem => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_WildTotem),
                Constants.Id_SecretGlade => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_SecretGlade),
                _ => false,
            };
        }

        public static bool EligibleForCropPerks(string item, string perk)
        {
            if (ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Crop))
                return false;

            return perk switch
            {
                Constants.LevelPerk_Foraging_16 => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_GrassDrop),
                Constants.Talent_Efflorescence => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_Efflorescence),
                Constants.Talent_TropicalBliss => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_TropicalBliss),
                Constants.Talent_NourishingRain => !ItemContextTagManager.HasBaseTag(item, Constants.ContextTag_Banned_NourishingRain),
                _ => false,
            };
        }

        public static bool AnyPlayerHasTalent(string flag)
        {
            if (!Context.IsWorldReady || ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly)
            {
                return false;
            }

            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (flag is not null && CurrentPlayerHasTalent(GetFlag(flag), who: farmer))
                    return true;
            }
            return false;
        }
        public static bool HostHasTalent(string flag) => CurrentPlayerHasTalent(GetFlag(flag), who: Game1.MasterPlayer);

        public static List<Vector2> GetTilesAroundBeeHouse(float xStart, float yStart)
        {
            Vector2 tile;
            List<Vector2> PositiveList = new();

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (x + y > 5)
                        break;
                    else if (x + y == 0)
                        continue;

                    tile = new(x, y);
                    if (!PositiveList.Contains(tile))
                        PositiveList.Add(tile);
                }
            }
            List<Vector2> CumulativeList = new();
            for (int i = 0; i < PositiveList.Count; i++)
            {
                tile = PositiveList[i];
                bool xPositive = tile.X > 0;
                bool yPositive = tile.Y > 0;
                CumulativeList.Add(new(xStart + tile.X, yStart + tile.Y));
                if (yPositive)
                    CumulativeList.Add(new(xStart + tile.X, yStart - tile.Y));
                if (xPositive)
                    CumulativeList.Add(new(xStart - tile.X, yStart + tile.Y));
                //To prevent duplicates from the center plus
                if (xPositive && yPositive)
                    CumulativeList.Add(new(xStart - tile.X, yStart - tile.Y));
            }
            return CumulativeList;
        }

        public static void DetermineGeodeDrop(Item geode, bool update = true) //remove the update parameter
        {
            if (Utility.IsGeode(geode, true))
            {
                MiningPatcher.IsUpdating = true;
                Item drop = Utility.getTreasureFromGeode(geode);
                MiningPatcher.IsUpdating = false;
                geode.modData[Constants.Key_XrayDrop] = drop.QualifiedItemId;
            }
        }

        public static bool isFavoredMonster(Monster monster, Farmer who)
        {
            CustomMonsterData monsterData = null;
            foreach (var item in ModEntry.CoreModEntry.Value.VanillaPlusProfessionsAPI.CustomMonsters)
            {
                if (item.Type.Equals(monster.GetType()))
                {
                    monsterData = item;
                    break;
                }
            }
            if (CurrentPlayerHasTalent(Constants.Talent_MonsterSpecialist, who: who))
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
            if (talentStatuses[Constants.Talent_GiftOfTheTalented] == Constants.ValidTalentStatuses[0])
            {
                TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects = true;
            }
            else if (talentStatuses[Constants.Talent_GiftOfTheTalented] == Constants.ValidTalentStatuses[1] || talentStatuses[Constants.Talent_GiftOfTheTalented] == Constants.ValidTalentStatuses[2])
            {

                TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects = false;
            }
            else
            {
                TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects = null;
            }
        }

        internal static void OnItemBasedTalentBoughtOrRefunded(Dictionary<string, string> talentStatuses)
        {
            bool switchTrinketRings = false;
            bool switchSapEdibility = false;
            bool markContextTagsDirty_SugarRush = false;
            bool hasSurvivalCooking = CurrentPlayerHasTalent(Constants.Talent_SurvivalCooking);

            if (talentStatuses.TryGetValue(Constants.Talent_Accessorise, out string val) && val == Constants.ValidTalentStatuses[1])
                switchTrinketRings = true;

            if (talentStatuses.TryGetValue(Constants.Talent_SapSipper, out val))
                switchSapEdibility = true;

            if (talentStatuses.TryGetValue(Constants.Talent_SugarRush, out val) && (val == Constants.ValidTalentStatuses[0] || val == Constants.ValidTalentStatuses[2]))
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
                            obj.Edibility = talentStatuses[Constants.Talent_SapSipper] == Constants.ValidTalentStatuses[0] || talentStatuses[Constants.Talent_SapSipper] == Constants.ValidTalentStatuses[2] ? 3 : -1;
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

        public static bool IsBlandStone(StardewValley.Object obj)
        {
            if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI is not null && ModEntry.CoreModEntry.Value.ItemExtensionsAPI.IsResource(obj.ItemId, out int? _, out string itemDropped))
            {
                return itemDropped is "390" or "(O)390";
            }
            return Constants.BlandStones.Contains(obj.ItemId) || obj.HasContextTag(Constants.ContextTag_BlandStone);
        }

        public static string GetNodeForRoomAndPillar()
        {
            return Game1.random.ChooseFrom(Constants.OreNodes);
        }

        public static string GetIDOfRing(Trinket trinket)
        {
            TrinketData data = trinket.GetTrinketData();

            if (data.CustomFields?.TryGetValue(Constants.Key_AccessoriseRing, out string value) is true && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            ModEntry.CoreModEntry.Value.ModMonitor.Log($"Cannot find ring ID for trinket {trinket.ItemId}. This should be reported to the author of the mod its coming from and not to KediDili.", LogLevel.Warn);
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
            if (ModEntry.CoreModEntry.Value.WearMoreRingsAPI is not null)
            {
                for (int i = 0; i < ModEntry.CoreModEntry.Value.WearMoreRingsAPI?.RingSlotCount(); i++)
                {
                    try
                    {
                        if (ModEntry.CoreModEntry.Value.WearMoreRingsAPI?.GetRing(i) is not null)
                        {
                            if (ModEntry.CoreModEntry.Value.WearMoreRingsAPI?.GetRing(i) is CombinedRing combinedRing && combinedRing is not null)
                            {
                                if (combinedRing.combinedRings[0] is TrinketRing)
                                    result.Add(combinedRing.combinedRings[0] as TrinketRing);

                                if (combinedRing.combinedRings[1] is TrinketRing)
                                    result.Add(combinedRing.combinedRings[1] as TrinketRing);
                            }
                            if (ModEntry.CoreModEntry.Value.WearMoreRingsAPI?.GetRing(i) is TrinketRing trinketRing && trinketRing != null)
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
                if (!ModEntry.CoreModEntry.Value.Helper.ModRegistry.IsLoaded(Constants.ModId_TrinketTinker) || !GameStateQuery.CheckConditions($"mushymato.TrinketTinker_DIRECT_EQUIP_ONLY {trinket.ItemId}"))
                {
                    //get the new guid and give it to the trinket's moddata
                    trinket.modData.TryAdd(Constants.Key_RingTrinkets, Guid.NewGuid().ToString());

                    //Add the trinket to the global inventory
                    player.team.GetOrCreateGlobalInventory(Constants.GlobalInventoryID_RingTrinkets).Add(trinket);

                    //Create the trinket ring and give it the same guid.
                    TrinketRing output = new(trinket);
                    output.modData.Add(Constants.Key_RingTrinkets, trinket.modData[Constants.Key_RingTrinkets]);

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
                ModEntry.CoreModEntry.Value.Helper.Multiplayer.SendMessage(CoordsForMP, ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/SwitchMineStones", new string[] { ModEntry.CoreModEntry.Value.Manifest.UniqueID });
            }
        }

        private static bool ShouldAddToThePool(string node, bool flag, MineShaft mineShaft)
        {
            if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI.GetResourceData(node, false, out object resourceData))
            {
                string SpawnOnFloors = ModEntry.CoreModEntry.Value.Helper.Reflection.GetProperty<string>(resourceData, "SpawnOnFloors").GetValue();
                double SpawnFrequency = ModEntry.CoreModEntry.Value.Helper.Reflection.GetProperty<double>(resourceData, "SpawnFrequency").GetValue();

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
                GemNodeList.AddRange(Constants.GemNodes);
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
                GeodeNodeList.AddRange(Constants.GeodeNodes);
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
                string locdata = Game1.player.currentLocation.GetData().CustomFields[flag ? Constants.Key_CrystalCavern : Constants.Key_Upheaval].Trim();
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
                if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI.IsResource(result, out int? nodeHealth, out string _))
                {
                    health = nodeHealth;
                }
            }
            health ??= GetStoneHealth(result);
            return result;
        }
    }
}
