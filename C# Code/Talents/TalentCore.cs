using System.Collections.Generic;
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
using StardewValley.BellsAndWhistles;

namespace VanillaPlusProfessions.Talents
{
    public class TalentCore
    {
        internal readonly static PerScreen<TalentCore> TalentCoreEntry = new( createNewState: () => new());

        internal int TalentPointCount;
        internal bool HasWaterCan;
        internal bool IsActionButtonUsed;
        internal int prevTimeSpeed;
        internal bool IsCookoutKit;
        internal static string VoidButterflyLocation;
        internal int TripleShotCooldown;

        internal static bool IsDayStartOrEnd = false;
        internal bool? GiveOrTakeStardropEffects = null;

        internal static Dictionary<string, Talent> Talents = new();
        internal static Dictionary<string, Skills.Skill> SkillsByName = new();

        [Obsolete("This field is obsolete as of 1.0.4. The disabled talents are handled through the mail flags now. This field is here to prevent save loss")]
        internal static List<string> DisabledTalents = new();

        internal void Initialize(ModEntry modEntry)
        {
            HasWaterCan = false;
            TalentCoreEntry.Value = this;
            modEntry.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            modEntry.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            modEntry.Helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            modEntry.Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            modEntry.Helper.Events.GameLoop.TimeChanged += OnTimeChanged;

            if (!ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly)
            {
                modEntry.Helper.Events.Player.InventoryChanged += OnInventoryChanged;
                modEntry.Helper.Events.World.NpcListChanged += OnNPCListChanged;
                modEntry.Helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
                modEntry.Helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
                modEntry.Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

                List<Talent> Talentlist = modEntry.Helper.ModContent.Load<List<Talent>>("assets\\talents.json");

                for (int i = 0; i < Talentlist.Count; i++)
                {
                    Talents.Add(Talentlist[i].Name, Talentlist[i]);
                }

                SpaceEvents.AfterGiftGiven += OnAfterGiftGiven;
                SpaceEvents.ChooseNightlyFarmEvent += OnChooseNightlyFarmEvent;

                modEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { Constants.Talent_AlchemicReversal, Constants.Talent_OverTheRainbow, Constants.Talent_SurvivalCooking, Constants.Talent_DriftFencing, Constants.Talent_TakeItSlow, Constants.Talent_Upcycling, Constants.Talent_CampSpirit, Constants.Talent_SpringThaw, Constants.Talent_Accessorise, Constants.Talent_EssenceInfusion, Constants.Talent_DoubleHook, Constants.Talent_ColdPress, Constants.Talent_SugarRush, Constants.Talent_SapSipper, Constants.Talent_TrashedTreasure, Constants.Talent_EyeSpy, Constants.Talent_FisheryGrant, Constants.Talent_MonumentalDiscount, Constants.Talent_Overcrowding, Constants.Talent_InTheWeeds, Constants.Talent_BigFishSmallPond, Constants.Talent_EveryonesBestFriend, Constants.Talent_BookclubBargains, Constants.Talent_WelcomeToTheJungle, Constants.Talent_VastDomain, Constants.Talent_HiddenBenefits, Constants.Talent_SleepUnderTheStars, Constants.Talent_BreedLikeRabbits, Constants.Talent_OneFishTwoFish }, TalentUtility.DataUpdates);
                modEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { Constants.Talent_SapSipper, Constants.Talent_SugarRush, Constants.Talent_Accessorise }, TalentUtility.OnItemBasedTalentBoughtOrRefunded);
                modEntry.VanillaPlusProfessionsAPI.RegisterTalentStatusAction(new string[] { Constants.Talent_GiftOfTheTalented }, TalentUtility.GiftOfTheTalented_ApplyOrUnApply);

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
                ModEntry.CoreModEntry.Value.ModMonitor.LogOnce("Talent system is disabled, and only VPP professions will work. If you didn't intend this, turn the ProfessionsOnly config off.", LogLevel.Info);
            }
        }
        internal void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModEntry.CoreModEntry.Value.Manifest.UniqueID && e.FromPlayerID == Game1.MasterPlayer.UniqueMultiplayerID && !Context.IsMainPlayer)
            {
                if (e.Type == ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/SwitchMineStones" && e.ReadAs<Dictionary<Vector2, string>>() is Dictionary<Vector2, string> dict)
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
                else if (e.Type == ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/BirdFeederData" && e.ReadAs<List<Critter>>() is List<Critter> birdList)
                {
                    MachineryEventHandler.BirdsOnFeeders.Add(Game1.MasterPlayer.currentLocation.NameOrUniqueName, birdList);
                }
                if (e.ReadAs<List<Vector2>>() is List<Vector2> tileList)
                {
                    string locName = Game1.GetPlayer(e.FromPlayerID, true)?.currentLocation?.NameOrUniqueName;
                    if (locName is not null)
                    {
                        if (e.Type.StartsWith(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/DrillLocationData"))
                        {
                            MachineryEventHandler.DrillLocations[locName] = tileList;
                        }
                        else if (e.Type.StartsWith(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/ThermalReactorLocationData"))
                        {
                            MachineryEventHandler.ThermalReactorLocations[locName] = tileList;
                        }
                        else if (e.Type.StartsWith(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/NodeMakerLocationData"))
                        {
                            MachineryEventHandler.NodeMakerLocations[locName] = tileList;
                        }
                    }
                }
                else if (e.Type == ModEntry.CoreModEntry.Value.Manifest.UniqueID + "/MushroomLevel")
                {
                    (Game1.player.currentLocation as MineShaft).rainbowLights.Value = true;
                }
            }
        }
        internal void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
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

        internal void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            Game1.player.mailReceived.Add(Constants.Key_PointsCalculated);
        }

        internal void OnChooseNightlyFarmEvent(object sender, EventArgsChooseNightlyFarmEvent e)
        {
            if (TalentUtility.HostHasTalent(Constants.Talent_FairysKiss))
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
        }
        internal void OnNPCListChanged(object sender, NpcListChangedEventArgs e)
        {
            if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_HarvestSeason) && e.IsCurrentLocation && e.Added is not null)
                foreach (var item in e.Added)
                    if (item is JunimoHarvester)
                        item.speed += 2;
        }
        internal void OnAfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_PrimrosePath) && e.Gift.Category == StardewValley.Object.flowersCategory)
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

        internal void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (TripleShotCooldown > 0)
            {
                TripleShotCooldown -= 1000;
            }
            if (!DisplayHandler.CoreDisplayHandler.Value.OpenTalentMenuCooldown && e.IsMultipleOf(15) && ModEntry.IsGameMenu(Game1.activeClickableMenu))
            {
                DisplayHandler.CoreDisplayHandler.Value.OpenTalentMenuCooldown = true;
            }
        }
        internal void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.OldTime < e.NewTime && IsTimeFollowing(e))
            {
                MachineryEventHandler.OnTimeChanged(e);
                if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_SpeedOfDarkness))
                {
                    if (e.NewTime is 2400 && e.OldTime is 2350)
                    {
                        BuffEffects buffEffects = new();
                        buffEffects.Speed.Value = 1;
                        Buff buff = new("VPP.SpeedOfDarkness.Speed", "VPP.SpeedOfDarkness", "Speed Of Darkness", -2, ModEntry.CoreModEntry.Value.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]), 27, buffEffects, false, ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"), Game1.parseText(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.SpeedOfDarkness.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"))));
                        Game1.player.buffs.Apply(buff);
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_Meditation) && !Game1.player.isMoving() && Context.IsPlayerFree)
                {
                    int extraHealth = ModEntry.CoreModEntry.Value.ModConfig.Meditation_Health;
                    if (Game1.player.health + extraHealth >= Game1.player.maxHealth)
                    {
                        Game1.player.health = Game1.player.maxHealth;
                    }
                    else if (Game1.player.health < Game1.player.maxHealth)
                    {
                        Game1.player.health += extraHealth;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Resurgence) && HasWaterCan)
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
            if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_SharedFocus))
            {
                var data = Game1.MasterPlayer.currentLocation.modData;
                if ((Game1.MasterPlayer.currentLocation is MineShaft or VolcanoDungeon || data?.ContainsKey(Constants.Key_SharedFocus) is true) && prevTimeSpeed is 0)
                {
                    Game1.isTimePaused = true;
                    prevTimeSpeed = Game1.realMilliSecondsPerGameTenMinutes;
                    int currentSecondAmount = Game1.gameTimeInterval / Game1.realMilliSecondsPerGameMinute;
                    Game1.realMilliSecondsPerGameTenMinutes = 12000;
                    Game1.realMilliSecondsPerGameMinute = 1200;
                    Game1.gameTimeInterval = Game1.realMilliSecondsPerGameMinute * currentSecondAmount;
                    Game1.isTimePaused = false;
                }
                else if ((Game1.MasterPlayer.currentLocation is not MineShaft or VolcanoDungeon || data?.ContainsKey(Constants.Key_SharedFocus) is false) && prevTimeSpeed != 0)
                {
                    Game1.isTimePaused = true;
                    Game1.realMilliSecondsPerGameTenMinutes = prevTimeSpeed;
                    int currentSecondAmount = Game1.gameTimeInterval / Game1.realMilliSecondsPerGameMinute;
                    Game1.realMilliSecondsPerGameMinute = Game1.realMilliSecondsPerGameTenMinutes / 10;
                    prevTimeSpeed = 0;
                    Game1.gameTimeInterval = Game1.realMilliSecondsPerGameMinute * currentSecondAmount;
                    Game1.isTimePaused = false;
                }
            }
        }
        internal void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
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
                                HasWaterCan = true;
                                can.modData[Constants.Key_Resurgence] = "0";
                                break;
                            }
                            else if (item is StardewValley.Object obj && obj.QualifiedItemId == "(O)92")
                            {
                                if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_SapSipper))
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
                            HasWaterCan = false;
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
        internal void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache(PathUtilities.NormalizeAssetName("Strings/UI"));
            ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache(PathUtilities.NormalizeAssetName("LooseSprites/Cursors_1_6"));

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
            if (ModEntry.CoreModEntry.Value.SpaceCoreAPI?.GetCustomSkills().Length > 0)
            {
                SkillsByName = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<Dictionary<string, Skills.Skill>>(typeof(Skills), "SkillsByName").GetValue();
            }
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

            if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI is not null)
            {
                var nodeList = from obj in DataLoader.Objects(Game1.content)
                               where ModEntry.CoreModEntry.Value.ItemExtensionsAPI.IsStone(obj.Key) && !ModEntry.CoreModEntry.Value.ItemExtensionsAPI.IsClump(obj.Key)
                               select obj;

                foreach (var item in nodeList)
                {
                    if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI.IsResource(item.Key, out int? _, out string itemDropped) && itemDropped is not null)
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

            if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_Overcrowding) || TalentUtility.AnyPlayerHasTalent(Constants.Talent_BreedLikeRabbits))
            {
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data\\Buildings");
                if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_BreedLikeRabbits))
                {
                    ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("Data/FarmAnimals");
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
        internal void OnMailFlagGiven(string flag)
        {
            if (flag == "Farm_Eternal")
            {
                AddTalentPoint(10);
            }
        }
        internal void OnSpecialOrderChanged(NetList<SpecialOrder, NetRef<SpecialOrder>> list, int index, SpecialOrder OldOrder, SpecialOrder NewOrder)
        {
            if (NewOrder is not null)
            {
                if (NewOrder.ShouldDisplayAsComplete() && list.Contains(NewOrder) && NewOrder.orderType.Value is "Qi")
                    AddTalentPoint();
            }
        }

        public void AddTalentPoint(int increase = 1, bool postMessage = true)
        {
            if (postMessage && ModEntry.CoreModEntry.Value.IsRecalculatingPoints)
            {
                ModEntry.CoreModEntry.Value.IsRecalculatingPoints = false;
            }
            TalentPointCount += increase;
            if (postMessage && !ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly)
                Game1.showGlobalMessage(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Message.TalentPoint"));
        }

        internal void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.Added.Any() && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Reforestation))
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
        internal void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            TalentPointCount = 0;
        }
    }
}
