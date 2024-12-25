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

namespace VanillaPlusProfessions.Talents
{
    public class TalentCore
    {
        internal static readonly PerScreen<int> TalentPointCount = new(createNewState: () => 0);
        internal static readonly PerScreen<bool> HasWaterCan = new();
        internal static readonly PerScreen<bool> IsActionButtonUsed = new();
        internal static readonly PerScreen<int> prevTimeSpeed = new();
        internal static readonly PerScreen<bool> IsCookoutKit = new();
        internal static bool IsDayStartOrEnd = false;

        internal const string Key_TalentPoints = "Kedi.VPP.TalentPointCount";
        internal const string Key_PointsCalculated = "Kedi.VPP.TalentPointsCalculated";

        internal const string Key_XrayDrop = "Kedi.VPP.XrayDrop";
        internal const string Key_AccessoriseRing = "Kedi.VPP.AccessoriseRing";
        internal const string Key_CrystalCavern = "Kedi.VPP.CrystalCavern";
        internal const string Key_Upheaval = "Kedi.VPP.Upheaval";
        internal const string Key_SharedFocus = "Kedi.VPP.SharedFocus";
        internal const string Key_Resurgence = "Kedi.VPP.Resurgence";
        internal const string Key_SlowerSliming = "Kedi.VPP.SlowerSliming";
        internal const string Key_FaeBlessings = "Kedi.VPP.FaeBlessings";
        internal const string Key_LocalKnowledge = "Kedi.VPP.LocalKnowledge";
        internal const string Key_WasRainingHere = "Kedi.VPP.WasRainingHere";
        internal const string Key_DownInTheDepths = "Kedi.VPP.DownInTheDepths";
        internal const string Key_WildGrowth = "Kedi.VPP.WildGrowth";
        internal const string Key_StaticCharge = "Kedi.VPP.StaticCharge";
        internal const string Key_Reforestation = "Kedi.VPP.Reforestation";
        internal const string Key_Flurry = "Kedi.VPP.Flurry";
        internal const string Key_RendingStrike = "Kedi.VPP.RendingStrike";
        internal const string Key_HiddenBenefit_FairyBox = "Kedi.VPP.HiddenBenefit";
        internal const string Key_HiddenBenefit_Crop = "Kedi.VPP.FairyBox";
        internal const string Key_HiddenBenefit_FrogEggs = "Kedi.VPP.FrogEgg";
        internal const string Key_Efflorescence = "Kedi.VPP.Efflorescence";
        internal const string Key_Tropical_Bliss = "Kedi.VPP.TropicalBliss";

        internal const string ContextTag_PoisonousMushroom = "Kedi_VPP_Poisonous_Mushroom";
        internal const string ContextTag_BlandStone = "Kedi_VPP_Bland_Stone_Node";
        internal const string ContextTag_SurvivalCooking = "kedi_vpp_survival_cooking_food";
        internal const string ContextTag_Matryoshka_Banned_FromDropping = "kedi_vpp_banned_from_dropping";
        internal const string ContextTag_Matryoshka_Banned_FromBeingDropped = "kedi_vpp_banned_from_being_dropped";
        internal const string ContextTag_Banned_Node = "kedi_vpp_banned_node";

        internal static Dictionary<string, Talent> Talents = new();
        internal static Dictionary<string, Skills.Skill> SkillsByName = new();

        internal static void Initialize()
        {
            HasWaterCan.Value = false;
            ModEntry.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            ModEntry.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            ModEntry.Helper.Events.GameLoop.SaveCreated += OnSaveCreated;

            if (!ModEntry.ModConfig.Value.ProfessionsOnly)
            {
                ModEntry.Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
                ModEntry.Helper.Events.Player.InventoryChanged += OnInventoryChanged;
                ModEntry.Helper.Events.World.NpcListChanged += OnNPCListChanged;
                ModEntry.Helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;

                List<Talent> Talentlist = ModEntry.Helper.ModContent.Load<List<Talent>>("assets\\talents.json");

                for (int i = 0; i < Talentlist.Count; i++)
                {
                    Talents.Add(Talentlist[i].Name, Talentlist[i]);
                }

                SpaceEvents.AfterGiftGiven += OnAfterGiftGiven;
                SpaceEvents.ChooseNightlyFarmEvent += OnChooseNightlyFarmEvent;

                FarmingPatcher.ApplyPatches();
                MiningPatcher.ApplyPatches();
                ForagingPatcher.ApplyPatches();
                CombatPatcher.ApplyPatches();
                FishingPatcher.ApplyPatches();
                MiscPatcher.ApplyPatches();
            }
            else
            {
                ModEntry.ModMonitor.LogOnce("Talent system is disabled, and only VPP professions will work. If you didn't intend this, turn the ProfessionsOnly config off.", LogLevel.Info);
            }
        }

        internal static void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            Game1.player.mailReceived.Add(Key_PointsCalculated);
        }

        internal static void OnChooseNightlyFarmEvent(object sender, EventArgsChooseNightlyFarmEvent e)
        {
            if (TalentUtility.HostHasTalent("Farming_Fairys_Kiss"))
            {
                if (e.NightEvent is null or not QiPlaneEvent or WorldChangeEvent or FairyEvent)
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

                        int fairyRoseNumber = (from terrainFeature in Game1.getFarm().terrainFeatures.Pairs
                                               where terrainFeature.Value is HoeDirt hoedirt && hoedirt.crop is not null and Crop cCrop &&
                                               cCrop.indexOfHarvest.Value == "595" && cCrop.fullyGrown.Value
                                               select terrainFeature).Count();

                        double baseChance = 0.01;

                        if (random.NextBool(baseChance + (fairyRoseNumber * 0.007)))
                            e.NightEvent = new FairyEvent();
                    }
                }
            }
        }
        internal static void OnNPCListChanged(object sender, NpcListChangedEventArgs e)
        {
            if (TalentUtility.AnyPlayerHasTalent("Farming_Harvest_Season") && e.IsCurrentLocation && e.Added is not null)
                foreach (var item in e.Added)
                    if (item is JunimoHarvester)
                        item.speed += 2;
        }
        internal static void OnAfterGiftGiven(object sender, EventArgsGiftGiven e)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Foraging_Primrose_Path") && e.Gift.Category == StardewValley.Object.flowersCategory)
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
        internal static void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            MachineryEventHandler.OnTimeChanged(e);
            if (TalentUtility.AllPlayersHaveTalent("Mining_Speed_Of_Darkness"))
            {
                if (e.NewTime is 2400 && e.OldTime is 2350)
                {
                    BuffEffects buffEffects = new();
                    buffEffects.Speed.Value = 1;
                    Buff buff = new("VPP.SpeedOfDarkness.Speed", "VPP.SpeedOfDarkness", "Speed Of Darkness", -2, ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]), 27, buffEffects, false, ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.Helper.Translation.Get("Buff.SpeedOfDarkness.Name"))));
                    Game1.player.buffs.Apply(buff);
                }
            }
            if (TalentUtility.AllPlayersHaveTalent("Combat_Meditation") && !Game1.player.isMoving() && Context.IsPlayerFree)
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
            if (TalentUtility.CurrentPlayerHasTalent("Farming_Resurgence") && HasWaterCan.Value)
            {
                foreach (var item in Game1.player.Items)
                {
                    if (item is WateringCan can && can.modData.TryGetValue(Key_Resurgence, out string val))
                    {
                        if (val != "90" && can.WaterLeft < can.waterCanMax)
                        {
                            can.modData[Key_Resurgence] = "0";
                            can.WaterLeft += 15;
                            if (can.waterCanMax < can.WaterLeft)
                            {
                                can.WaterLeft = can.waterCanMax;
                            }
                            break;
                        }
                        else if (!can.IsBottomless)
                        {
                            can.modData[Key_Resurgence] = (int.Parse(val) + 10).ToString();
                            break;
                        }
                    }
                }
            }
            if (TalentUtility.AllPlayersHaveTalent("Mining_Shared_Focus"))
            {
                var data = Game1.MasterPlayer.currentLocation.modData;
                if ((Game1.MasterPlayer.currentLocation is MineShaft or VolcanoDungeon || data?.ContainsKey(Key_SharedFocus) is true) && prevTimeSpeed.Value is 0)
                {
                    Game1.isTimePaused = true;
                    prevTimeSpeed.Value = Game1.realMilliSecondsPerGameTenMinutes;
                    int currentSecondAmount = Game1.gameTimeInterval / Game1.realMilliSecondsPerGameMinute;
                    Game1.realMilliSecondsPerGameTenMinutes = 12000;
                    Game1.realMilliSecondsPerGameMinute = 1200;
                    Game1.gameTimeInterval = Game1.realMilliSecondsPerGameMinute * currentSecondAmount;
                    Game1.isTimePaused = false;
                }
                if ((Game1.MasterPlayer.currentLocation is not MineShaft or VolcanoDungeon || data?.ContainsKey(Key_SharedFocus) is false) && prevTimeSpeed.Value != 0)
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
                if (e.Added is not null)
                {
                    foreach (var item in e.Added)
                    {
                        TalentUtility.DetermineGeodeDrop(item);
                    }
                    foreach (var item in e.Added)
                    {
                        if (item is WateringCan can && can is not null && !can.IsBottomless)
                        {
                            HasWaterCan.Value = true;
                            if (!can.modData.TryAdd(Key_Resurgence, "0"))
                            {
                                can.modData[Key_Resurgence] = "0";
                                break;
                            }
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
                            if (TalentUtility.AnyPlayerHasTalent("BigFishSmallPond") || TalentUtility.AnyPlayerHasTalent("SugarRush"))
                            {
                                if (obj.Category is StardewValley.Object.FishCategory or StardewValley.Object.CookingCategory)
                                {
                                    obj.MarkContextTagsDirty();
                                }
                            }
                        }
                    }
                }
                if (e.Removed is not null)
                {
                    foreach (var item in e.Removed)
                    {
                        if (item is WateringCan can && can is not null && !can.IsBottomless)
                        {
                            HasWaterCan.Value = false;
                            if (!can.modData.TryAdd(Key_Resurgence, "0"))
                            {
                                can.modData[Key_Resurgence] = "0";
                                break;
                            }
                        }
                    }
                    foreach (var item in e.Removed)
                    {
                        if (Game1.activeClickableMenu is not GeodeMenu or GameMenu)
                        {
                            TalentUtility.DetermineGeodeDrop(item);
                        }
                    }
                }
                if (e.QuantityChanged is not null)
                {
                    foreach (var item in e.QuantityChanged)
                    {
                        if (item is not null)
                        {
                            if (item.NewSize < item.OldSize)
                            {
                                TalentUtility.DetermineGeodeDrop(item.Item);
                            }
                        }
                    }
                }
            }
        }
        internal static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.Helper.GameContent.InvalidateCache(PathUtilities.NormalizeAssetName("Strings/UI"));
            if (Game1.player.modData.TryGetValue(Key_TalentPoints, out string value))
            {
                if (int.TryParse(value, out int result) && result >= 0)
                {
                    AddTalentPoint(result, false);
                }
            }
            else
            {
                Game1.player.modData.TryAdd(Key_TalentPoints, "0");
            }
            if (ModEntry.SpaceCoreAPI.Value?.GetCustomSkills().Length > 0)
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
            Game1.player.achievements.OnValueAdded += OnAchievementAdded;
            Game1.player.team.specialOrders.OnElementChanged += OnSpecialOrderChanged;
            Game1.player.mailReceived.OnValueAdded += OnMailFlagGiven;
            if (!ModEntry.Helper.ModRegistry.IsLoaded("spacechase0.LuckSkill"))
            {
                if (Game1.player.professions.Count > 0)
                {
                    List<int> copy = new();
                    foreach (var item in Game1.player.professions)
                    {
                        copy.Add(item);
                    }
                    foreach (var item in copy)
                    {
                        if (item > 29 && item < 467800)
                        {
                            Game1.player.professions.Add(item + 467800);
                            Game1.player.professions.Remove(item);
                        }
                    }
                }
            }
            if (ModEntry.ItemExtensionsAPI.Value is not null)
            {
                var nodeList = from obj in DataLoader.Objects(Game1.content)
                               where ModEntry.ItemExtensionsAPI.Value.IsStone(obj.Key) && !ModEntry.ItemExtensionsAPI.Value.IsClump(obj.Key)
                               select obj;

                foreach (var item in nodeList)
                {
                    if (ModEntry.ItemExtensionsAPI.Value.IsResource(item.Key, out int? _, out string itemDropped) && itemDropped is not null)
                    {
                        if (ItemRegistry.GetData(itemDropped).RawData is not ObjectData objectData || objectData?.ContextTags?.Contains(ContextTag_Banned_Node) is true)
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
        }
        internal static void OnMailFlagGiven(string flag)
        {
            if (flag == "Farm_Eternal")
            {
                AddTalentPoint(10);
            }
        }
        internal static void OnAchievementAdded(int value) => AddTalentPoint();
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
            TalentPointCount.Value += increase;
            if (postMessage && !ModEntry.ModConfig.Value.ProfessionsOnly)
                Game1.showGlobalMessage(ModEntry.Helper.Translation.Get("Message.TalentPoint"));
        }

        internal static void OnTerrainFeatureListChanged(object sender, TerrainFeatureListChangedEventArgs e)
        {
            if (e.Added.Any() && TalentUtility.CurrentPlayerHasTalent("Foraging_Reforestation"))
            {
                foreach (var item in e.Added)
                {
                    if (item.Value is Tree tree && tree.growthStage.Value is 0)
                    {
                        if (!tree.modData.TryAdd(Key_Reforestation, "true"))
                            tree.modData[Key_Reforestation] = "true";
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
