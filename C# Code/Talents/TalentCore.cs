﻿using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Characters;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using VanillaPlusProfessions.Talents.Patchers;
using System;
using StardewValley.Events;
using StardewValley.Extensions;
using SpaceCore.Events;
using VanillaPlusProfessions.Utilities;
using StardewModdingAPI;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using SpaceCore;
using StardewValley.GameData.Objects;
using VanillaPlusProfessions.Craftables;
using StardewValley.Triggers;
using Microsoft.Xna.Framework;

namespace VanillaPlusProfessions.Talents
{
    public class TalentCore
    {
        internal static readonly PerScreen<int> TalentPointCount = new(createNewState: () => 0);
        internal static readonly PerScreen<bool> HasWaterCan = new();
        internal static readonly PerScreen<bool> IsActionButtonUsed = new();
        internal static readonly PerScreen<int> prevTimeSpeed = new();
        internal static readonly PerScreen<bool> IsCookoutKit = new();
        internal static string VoidButterflyLocation;
        internal static int TripleShotCooldown = new();

        internal static bool IsDayStartOrEnd = false;
        internal static bool? GiveOrTakeStardropEffects = null;

        

        internal static Dictionary<string, Talent> Talents = new();
        internal static Dictionary<string, Skills.Skill> SkillsByName = new();

        internal static List<string> DisabledTalents = new();

        internal static void Initialize()
        {
            HasWaterCan.Value = false;
            ModEntry.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            ModEntry.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            ModEntry.Helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            ModEntry.Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            ModEntry.Helper.Events.GameLoop.TimeChanged += OnTimeChanged;

            if (!ModEntry.ModConfig.Value.ProfessionsOnly)
            {
                ModEntry.Helper.Events.Player.InventoryChanged += OnInventoryChanged;
                ModEntry.Helper.Events.World.NpcListChanged += OnNPCListChanged;
                ModEntry.Helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
                ModEntry.Helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
                ModEntry.Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

                List<Talent> Talentlist = ModEntry.Helper.ModContent.Load<List<Talent>>("assets\\talents.json");

                for (int i = 0; i < Talentlist.Count; i++)
                {
                    Talents.Add(Talentlist[i].Name, Talentlist[i]);
                }

                SpaceEvents.AfterGiftGiven += OnAfterGiftGiven;
                SpaceEvents.ChooseNightlyFarmEvent += OnChooseNightlyFarmEvent;

                ModEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { "AlchemicReversal", "OverTheRainbow", "SurvivalCooking", "DriftFencing", "TakeItSlow", "Upcycling", "CampSpirit", "SpringThaw", "Accessorise", "EssenceInfusion", "DoubleHook", "ColdPress", "SugarRush", "SapSipper", "TrashedTreasure", "EyeSpy", "FisheryGrant", "MonumentalDiscount", "Overcrowding", "InTheWeeds", "LegendaryVariety", "EveryonesBestFriend", "BookclubBargains", "WelcomeToTheJungle", "VastDomain", "HiddenBenefits", "SleepUnderTheStars", "BreedLikeRabbits" }, TalentUtility.DataUpdates);
                ModEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { "SapSipper", "SugarRush", "Accessorise" }, TalentUtility.OnItemBasedTalentBoughtOrRefunded);
                ModEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { "GiftOfTheTalented" }, TalentUtility.GiftOfTheTalented_ApplyOrUnApply);

                FarmingPatcher.ApplyPatches();
                MiningPatcher.ApplyPatches();
                ForagingPatcher.ApplyPatches();
                CombatPatcher.ApplyPatches();
                FishingPatcher.ApplyPatches();
                MiscPatcher.ApplyPatches();

                TriggerActionManager.RegisterTrigger("KediDili.VanillaPlusProfessions_UpdateRecipes");
            }
            else
            {
                ModEntry.ModMonitor.LogOnce("Talent system is disabled, and only VPP professions will work. If you didn't intend this, turn the ProfessionsOnly config off.", LogLevel.Info);
            }
        }
        internal static void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModEntry.Manifest.UniqueID && e.FromPlayerID == Game1.MasterPlayer.UniqueMultiplayerID && !Context.IsMainPlayer)
            {
                if (e.Type == ModEntry.Manifest.UniqueID + "/SwitchMineStones" && e.ReadAs<Dictionary<Vector2, string>>() is Dictionary<Vector2, string> dict)
                {
                    foreach (var key in dict.Keys)
                    {
                        var stone = ItemRegistry.Create<StardewValley.Object>(dict[key]);
                        if (stone.ItemId is "2" or "4" or "6" or "8" or "10" or "12" or "14" or "95")
                        {
                            stone.MinutesUntilReady = TalentUtility.GetStoneHealth(stone.ItemId);
                        }
                        Game1.player.currentLocation.Objects[key] = stone;
                    }
                }
                else if (e.Type == ModEntry.Manifest.UniqueID + "/MushroomLevel")
                {
                    (Game1.player.currentLocation as MineShaft).rainbowLights.Value = true;
                }
            }
        }
        internal static void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
        {
            if (e.QuantityChanged is not null)
            {
                foreach (var item in e.QuantityChanged)
                {
                    if (item.OldSize > item.NewSize)
                    {
                        TalentUtility.DetermineGeodeDrop(item.Item);
                    }
                }
            }
        }

        internal static void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            Game1.player.mailReceived.Add(Constants.Key_PointsCalculated);
        }

        internal static void OnChooseNightlyFarmEvent(object sender, EventArgsChooseNightlyFarmEvent e)
        {
            if (TalentUtility.HostHasTalent("FairysKiss"))
            {
                if (e.NightEvent is null)
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

                        if (random.NextBool(baseChance + (fairyRoseNumber * 0.0007)))
                            e.NightEvent = new FairyEvent();
                    }
                }
            }
            if (TalentUtility.HostHasTalent("BreedLikeRabbits"))
            {
                e.NightEvent = new QuestionEvent(2);
                e.NightEvent = new QuestionEvent(2);
            }
        }
        internal static void OnNPCListChanged(object sender, NpcListChangedEventArgs e)
        {
            if (TalentUtility.AnyPlayerHasTalent("HarvestSeason") && e.IsCurrentLocation && e.Added is not null)
                foreach (var item in e.Added)
                    if (item is JunimoHarvester)
                        item.speed += 2;
        }
        internal static void OnAfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (TalentUtility.CurrentPlayerHasTalent("PrimrosePath") && e.Gift.Category == StardewValley.Object.flowersCategory)
            {
                Farmer who = sender as Farmer;
                Friendship friendship = who?.friendshipData[e.Npc.Name];
                if (who is not null && friendship is not null)
                {
                    float friendshipChangeMultiplier = 1f;
                    int tasteForItem = e.Npc.getGiftTasteForThisItem(e.Gift);
                    float qualityChangeMultipler = 1f;
                    if (e.Gift.Quality is 1)
                        qualityChangeMultipler = 1.1f;
                    else if (e.Gift.Quality is 2)
                        qualityChangeMultipler = 1.25f;
                    else if (e.Gift.Quality is 4)
                        qualityChangeMultipler = 1.5f;
                    
                    if (e.Npc.isBirthday())
                        friendshipChangeMultiplier = 8f;
                    if (e.Npc.getSpouse() is not null && e.Npc.getSpouse().Equals(who))
                        friendshipChangeMultiplier /= 2f;

                    int totalPoints = tasteForItem switch
                    {
                        7 => Math.Min(750, (int)(250f * friendshipChangeMultiplier)),
                        6 => (int)(-40f * friendshipChangeMultiplier),
                        4 => (int)(-20f * friendshipChangeMultiplier),
                        2 => (int)(45f * friendshipChangeMultiplier * qualityChangeMultipler),
                        0 => (int)(80f * friendshipChangeMultiplier * qualityChangeMultipler),
                        _ => (int)(20f * friendshipChangeMultiplier)
                    };
                    who.changeFriendship(totalPoints / 4, e.Npc);
                }
            }
        }
        internal static bool IsTimeFollowing(TimeChangedEventArgs e)
        {
            if (e.OldTime.ToString().EndsWith("50"))
            {
                return e.NewTime.ToString().EndsWith("00");
            }
            return e.OldTime + 10 == e.NewTime;
        }

        internal static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (TripleShotCooldown > 0)
            {
                TripleShotCooldown -= 1000;
            }
            if (!DisplayHandler.OpenTalentMenuCooldown.Value && e.IsMultipleOf(15) && ModEntry.IsGameMenu(Game1.activeClickableMenu))
            {
                DisplayHandler.OpenTalentMenuCooldown.Value = true;
            }
        }
        internal static void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.OldTime < e.NewTime && IsTimeFollowing(e))
            {
                MachineryEventHandler.OnTimeChanged(e);
                if (TalentUtility.AllPlayersHaveTalent("SpeedOfDarkness"))
                {
                    if (e.NewTime is 2400 && e.OldTime is 2350)
                    {
                        BuffEffects buffEffects = new();
                        buffEffects.Speed.Value = 1;
                        Buff buff = new("VPP.SpeedOfDarkness.Speed", "VPP.SpeedOfDarkness", "Speed Of Darkness", -2, ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]), 27, buffEffects, false, ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"))));
                        Game1.player.buffs.Apply(buff);
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent("Meditation") && !Game1.player.isMoving() && Context.IsPlayerFree)
                {
                    if (Game1.player.health + 15 >= Game1.player.maxHealth)
                    {
                        Game1.player.health = Game1.player.maxHealth;
                    }
                    else if (Game1.player.health < Game1.player.maxHealth)
                    {
                        Game1.player.health += 15;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("Resurgence") && HasWaterCan.Value)
                {
                    foreach (var item in Game1.player.Items)
                    {
                        if (item is WateringCan can && !can.modData.TryAdd(Constants.Key_Resurgence, "0"))
                        {
                            if (can.modData.TryGetValue(Constants.Key_Resurgence, out string val))
                            {
                                int value = int.TryParse(val, out int result) ? result : -1;
                                if (value >= 90 && can.WaterLeft < can.waterCanMax)
                                {
                                    can.modData[Constants.Key_Resurgence] = "0";
                                    can.WaterLeft += can.waterCanMax / 5;
                                    if (can.waterCanMax < can.WaterLeft)
                                        can.WaterLeft = can.waterCanMax;

                                    break;
                                }
                                else if (!can.IsBottomless && value != -1)
                                {
                                    can.modData[Constants.Key_Resurgence] = (value + 10).ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (TalentUtility.AllPlayersHaveTalent("SharedFocus"))
            {
                var data = Game1.MasterPlayer.currentLocation.modData;
                if ((Game1.MasterPlayer.currentLocation is MineShaft or VolcanoDungeon || data?.ContainsKey(Constants.Key_SharedFocus) is true) && prevTimeSpeed.Value is 0)
                {
                    Game1.isTimePaused = true;
                    prevTimeSpeed.Value = Game1.realMilliSecondsPerGameTenMinutes;
                    int currentSecondAmount = Game1.gameTimeInterval / Game1.realMilliSecondsPerGameMinute;
                    Game1.realMilliSecondsPerGameTenMinutes = 12000;
                    Game1.realMilliSecondsPerGameMinute = 1200;
                    Game1.gameTimeInterval = Game1.realMilliSecondsPerGameMinute * currentSecondAmount;
                    Game1.isTimePaused = false;
                }
                else if ((Game1.MasterPlayer.currentLocation is not MineShaft or VolcanoDungeon || data?.ContainsKey(Constants.Key_SharedFocus) is false) && prevTimeSpeed.Value != 0)
                {
                    Game1.isTimePaused = true;
                    Game1.realMilliSecondsPerGameTenMinutes = prevTimeSpeed.Value;
                    int currentSecondAmount = Game1.gameTimeInterval / Game1.realMilliSecondsPerGameMinute;
                    Game1.realMilliSecondsPerGameMinute = Game1.realMilliSecondsPerGameTenMinutes / 10;
                    prevTimeSpeed.Value = 0;
                    Game1.gameTimeInterval = Game1.realMilliSecondsPerGameMinute * currentSecondAmount;
                    Game1.isTimePaused = false;
                }
            }
        }
        internal static void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.QuantityChanged is not null)
                {
                    foreach (var item in e.QuantityChanged)
                    {
                        if (item is not null)
                        {
                            if (item.NewSize != item.OldSize)
                            {
                                TalentUtility.DetermineGeodeDrop(item.Item);
                            }
                        }
                    }
                }
                if (e.Added is not null)
                {
                    foreach (var item in e.Added)
                    {
                        if (Game1.activeClickableMenu is MenuWithInventory menu && menu.heldItem != item || Game1.activeClickableMenu is null)
                        {
                            if (item is WateringCan can && !can.IsBottomless)
                            {
                                HasWaterCan.Value = true;
                                can.modData[Constants.Key_Resurgence] = "0";
                                break;
                            }
                            else if (item is StardewValley.Object obj && obj.QualifiedItemId == "(O)92")
                            {
                                if (TalentUtility.AnyPlayerHasTalent("SapSipper"))
                                {
                                    obj.Edibility = 3;
                                }
                                else
                                {
                                    obj.Edibility = -1;
                                }
                            }
                            else if (!item.modData.ContainsKey(Constants.Key_XrayDrop))
                            {
                                TalentUtility.DetermineGeodeDrop(item);
                            }
                        }                        
                    }
                }
                if (e.Removed is not null)
                {
                    foreach (var item in e.Removed)
                    {
                        if (item is WateringCan can && !can.IsBottomless)
                        {
                            HasWaterCan.Value = false;
                            can.modData[Constants.Key_Resurgence] = "0";
                            break;
                        }
                        else if (Game1.activeClickableMenu is MenuWithInventory menu && menu.heldItem != item || Game1.activeClickableMenu is null)
                        {
                            if (!item.modData.ContainsKey(Constants.Key_XrayDrop))
                            {
                                TalentUtility.DetermineGeodeDrop(item);
                            }
                        }
                    }
                }
            }
        }
        internal static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.Helper.GameContent.InvalidateCache(PathUtilities.NormalizeAssetName("Strings/UI"));

            if (Game1.player.modData.TryGetValue(Constants.Key_TalentPoints, out string value))
            {
                if (int.TryParse(value, out int result) && result >= 0)
                {
                    AddTalentPoint(result, false);
                }
            }
            else
            {
                Game1.player.modData.TryAdd(Constants.Key_TalentPoints, "0");
            }
            if (ModEntry.SpaceCoreAPI?.GetCustomSkills().Length > 0)
            {
                SkillsByName = ModEntry.Helper.Reflection.GetField<Dictionary<string, Skills.Skill>>(typeof(Skills), "SkillsByName").GetValue();
            }
            if (TalentUtility.CurrentPlayerHasTalent("Accessorise"))
            {
                foreach (var item in TalentUtility.GetAllTrinketRings(Game1.player))
                {
                    item.onEquip(Game1.player);
                }
            }
            //Game1.player.achievements.OnValueAdded += OnAchievementAdded;
            Game1.player.team.specialOrders.OnElementChanged += OnSpecialOrderChanged;
            Game1.player.mailReceived.OnValueAdded += OnMailFlagGiven;

            if (Game1.player.modData.TryGetValue(Constants.Key_DisabledTalents, out string value2) && value is not null and "")
            {
                DisabledTalents = value2.Split('|').ToList();
            }
            foreach (var item in DisabledTalents)
            {
                if (!Game1.player.mailReceived.Contains(item + "_disabled"))
                {
                    Game1.player.mailReceived.Add(item + "_disabled");
                }
            }

            if (ModEntry.ItemExtensionsAPI is not null)
            {
                var nodeList = from obj in DataLoader.Objects(Game1.content)
                               where ModEntry.ItemExtensionsAPI.IsStone(obj.Key) && !ModEntry.ItemExtensionsAPI.IsClump(obj.Key)
                               select obj;

                foreach (var item in nodeList)
                {
                    if (ModEntry.ItemExtensionsAPI.IsResource(item.Key, out int? _, out string itemDropped) && itemDropped is not null)
                    {
                        if (ItemRegistry.GetData(itemDropped).RawData is not ObjectData objectData || objectData?.ContextTags?.Contains(Constants.ContextTag_Banned_Node) is true)
                            continue;

                        if (objectData is ObjectData && objectData.Category == StardewValley.Object.GemCategory)
                        {
                            TalentUtility.ItemExtensions_GemNodeList.Add(item.Key);
                        }
                        else if (objectData.GeodeDrops is not null || objectData.GeodeDropsDefaultItems)
                        {
                            TalentUtility.ItemExtensions_GeodeNodeList.Add(item.Key);
                        }
                    }
                }
            }

            if (TalentUtility.AnyPlayerHasTalent("Overcrowding") || TalentUtility.AnyPlayerHasTalent("BreedLikeRabbits"))
            {
                ModEntry.Helper.GameContent.InvalidateCache("Data\\Buildings");
                if (TalentUtility.AnyPlayerHasTalent("BreedLikeRabbits"))
                {
                    ModEntry.Helper.GameContent.InvalidateCache("Data/FarmAnimals");
                }
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
        }
        internal static void OnMailFlagGiven(string flag)
        {
            if (flag == "Farm_Eternal")
            {
                AddTalentPoint(10);
            }
        }
        internal static void OnSpecialOrderChanged(NetList<SpecialOrder, NetRef<SpecialOrder>> list, int index, SpecialOrder OldOrder, SpecialOrder NewOrder)
        {
            if (NewOrder is not null)
            {
                if (NewOrder.ShouldDisplayAsComplete() && list.Contains(NewOrder) && NewOrder.orderType.Value is "Qi")
                    AddTalentPoint();
            }
        }

        public static void AddTalentPoint(int increase = 1, bool postMessage = true)
        {
            if (postMessage && ModEntry.IsRecalculatingPoints.Value)
            {
                ModEntry.IsRecalculatingPoints.Value = false;
            }
            TalentPointCount.Value += increase;
            if (postMessage && !ModEntry.ModConfig.Value.ProfessionsOnly)
                Game1.showGlobalMessage(ModEntry.Helper.Translation.Get("Message.TalentPoint"));
        }

        internal static void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.Added.Any() && TalentUtility.CurrentPlayerHasTalent("Reforestation"))
            {
                foreach (var item in e.Added)
                {
                    if (item.Value is Tree tree && tree.growthStage.Value is 0)
                    {
                        if (!tree.modData.TryAdd(Constants.Key_Reforestation, "true"))
                            tree.modData[Constants.Key_Reforestation] = "true";
                    }
                }
            }
        }
        internal static void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            TalentPointCount.Value = 0;
        }
    }
}
