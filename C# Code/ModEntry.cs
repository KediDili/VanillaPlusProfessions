using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Objects.Trinkets;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Buffs;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.GiantCrops;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using VanillaPlusProfessions.Enchantments;
using VanillaPlusProfessions.Utilities;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Compatibility;
using VanillaPlusProfessions.Managers;
using StardewValley.Menus;
using SpaceCore.Interface;
using VanillaPlusProfessions.Talents.UI;
using SpaceCore;
using VanillaPlusProfessions.Craftables;
using StardewValley.Internal;

namespace VanillaPlusProfessions
{
    public class ModEntry : Mod
    {
        internal static new IModHelper Helper;
        internal static IMonitor ModMonitor;
        internal static IManifest Manifest;

        internal static IContentPatcher ContentPatcherAPI;
        internal static IGenericModConfigMenu GenericModConfigMenuAPI;
        internal static ISpaceCore SpaceCoreAPI;
        internal static IWearMoreRings WearMoreRingsAPI;
        internal static IItemExtensions ItemExtensionsAPI;
        internal static IBetterGameMenuApi BetterGameMenuAPI;

        internal static VanillaPlusProfessionsAPI VanillaPlusProfessionsAPI = new();

        internal static CustomQueries CustomQueries = new();
        internal static Harmony Harmony { get; } = new("KediDili.VanillaPlusProfessions");

        internal static IProfessionManager[] Managers = new IProfessionManager[6];
        internal static int[] levelExperiences;

        internal readonly static PerScreen<bool> UpdateGeodeInMenu = new(() => false);
        internal readonly static PerScreen<int> GeodeStackSize = new(() => 0);
        internal readonly static PerScreen<bool> IsUninstalling = new(() => false);
        internal readonly static PerScreen<bool> IsRecalculatingPoints = new(() => false);
        internal readonly static PerScreen<Config> ModConfig = new(() => new Config());

        //So mods can access it without needing reflection.
        public static Dictionary<string, Profession> Professions = new();

        internal const string Key_HasFoundForage = "Kedi.VPP.HasFoundForageGame";
        internal const string Key_DaysLeftForForageGuess = "Kedi.VPP.GuessForageGameDaysLeft";
        internal const string Key_ForageGuessItemID = "Kedi.VPP.GuessForageID";

        internal const string Key_TFTapperDaysLeft = "Kedi.VPP.ProduceDaysLeft";
        internal const string Key_TFTapperID = "Kedi.VPP.TapperID";
        internal const string Key_TFHasTapper = "Kedi.VPP.IsTapped";
        internal const string Key_FruitTreeOrGiantCrop = "Kedi.VPP.DoesUseCustomItem";

        internal const string Key_IsSlimeHutchWatered = "Kedi.VPP.IsWatered";
        internal const string Key_SlimeWateredDaysSince = "Kedi.VPP.SlimeWateredDaysSince";
        internal const string Key_FishRewardOrQuestDayLeft = "Kedi.VPP.FishQuestOrRewardDuration";

        public override void Entry(IModHelper helper)
        {
            Helper = helper;
            ModMonitor = Monitor;
            Manifest = ModManifest;

            ModConfig.Value = Helper.ReadConfig<Config>();
            levelExperiences = Helper.Data.ReadJsonFile<int[]>("assets/levelExperiences.json");
            Professions = Helper.Data.ReadJsonFile<Dictionary<string, Profession>>("assets/professions.json");

            ContentEditor.Initialize();
            DisplayHandler.Initialize();

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonReleased += OnButtonReleased;
            Helper.Events.GameLoop.DayStarted += DayStartHandler.OnDayStarted;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.Player.LevelChanged += OnLevelChanged;
            Helper.Events.Player.Warped += OnWarped;
            
            CorePatcher.ApplyPatches();
            TalentCore.Initialize();
            BuildingPatcher.ApplyPatches();
            CraftablePatcher.ApplyPatches();

            Helper.ConsoleCommands.Add("vpp.removeAll", "Removes all professions, talents and metadata added by Vanilla Plus Professions.", CoreUtility.remove);
            Helper.ConsoleCommands.Add("vpp.recalculatepoints", "Recalculates all talent points, useful for existing saves that are being loaded for the first time with this mod.", CoreUtility.recalculate);
            Helper.ConsoleCommands.Add("vpp.details", "Prints out skill related information. Might be useful for troubleshooting.", CoreUtility.details);
            Helper.ConsoleCommands.Add("vpp.reset", "Can be used to reset professions added by VPP. First parameter is the level (15 or 20), second is the level (0 - Farming, 1 - Fishing, 2 - Foraging, 3 - Mining or 4 - Combat)", ManagerUtility.reset);
            Helper.ConsoleCommands.Add("vpp.resetTrinkets", "Can be used to reset trinkets from bugs caused by VPP. Warning: If you use this while there are no problems, you WILL encounter errors.", TalentUtility.RemoveAndReapplyAllTrinketEffects);
            Helper.ConsoleCommands.Add("vpp.showXPLeft", "Shows how much XP left for the next level in all vanilla skills.", CoreUtility.showXPLeft);
            Managers = new IProfessionManager[] { new FarmingManager(), new MiningManager(), new ForagingManager(), new FishingManager(), new CombatManager(), new ComboManager() };

            var dict = Professions?.Keys.ToList();
            foreach (var manager in Managers)
            {
                foreach (var item in from item in Professions where item.Value.ID % 8 == manager.SkillValue || manager.SkillValue == 9 select item)
                    manager.RelatedProfessions.Add(item.Key, item.Value);
                manager.ApplyPatches();
            }
        }
        public override object GetApi(IModInfo mod)
        {
            ModMonitor.Log("Mod with the name of " + mod.Manifest.Name + " and with the unique ID of " + mod.Manifest.UniqueID + " has requested the API.");
            return VanillaPlusProfessionsAPI;
        }

        public static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                ContentPatcherAPI = Helper.ModRegistry.GetApi<IContentPatcher>("Pathoschild.ContentPatcher");
                GenericModConfigMenuAPI = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
                SpaceCoreAPI = Helper.ModRegistry.GetApi<ISpaceCore>("spacechase0.SpaceCore");
                WearMoreRingsAPI = Helper.ModRegistry.GetApi<IWearMoreRings>("bcmpinc.WearMoreRings");
                ItemExtensionsAPI = Helper.ModRegistry.GetApi<IItemExtensions>("mistyspring.ItemExtensions");
                BetterGameMenuAPI = Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
            }
            catch (Exception)
            {
                ModMonitor.Log("Something has seriously gone wrong with an API request. This could indicate VPP's versions of APIs being out of date, outright the wrong type or some other error. Little to no interactions may work this session.");
            }

            CustomQueries.Initialize();
            DisplayHandler.InitializeBetterGameMenu();

            if (ContentPatcherAPI is not null)
            {
                ContentPatcherAPI.RegisterToken(Manifest, "HasProfessions", GetProfessions);
                ContentPatcherAPI.RegisterToken(Manifest, "HasTalents", new HasTalents());
                ContentPatcherAPI.RegisterToken(Manifest, "ContentPaths", new ContentPaths());
                ContentPatcherAPI.RegisterToken(Manifest, "ProfessionsOnly", () => new string[] { ModConfig.Value.ProfessionsOnly.ToString() });
            }
            else
                ModMonitor.Log("Content Patcher is either not installed or there was a problem while requesting the API. Skipping token additions.", LogLevel.Info);
            if (GenericModConfigMenuAPI is not null)
            {
                GenericModConfigMenuAPI.Register(Manifest, () => ModConfig.Value = new Config(), () => Helper.WriteConfig(ModConfig.Value));
                GenericModConfigMenuAPI.AddBoolOption(Manifest, () => ModConfig.Value.ColorBlindnessChanges, value => ModConfig.Value.ColorBlindnessChanges = value, () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Name"), () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Desc"));
                GenericModConfigMenuAPI.AddBoolOption(Manifest, () => ModConfig.Value.DeveloperOrTestingMode, value => ModConfig.Value.DeveloperOrTestingMode = value, () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Name"), () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Desc"));
                //GenericModConfigMenuAPI.AddBoolOption(Manifest, () => ModConfig.Value.MasteryCaveChanges, value => ModConfig.Value.MasteryCaveChanges = value, () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Name"), () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Desc"));
                GenericModConfigMenuAPI.AddNumberOption(Manifest, () => ModConfig.Value.MasteryCaveChanges, value => ModConfig.Value.MasteryCaveChanges = value, () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Name"), () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Desc"), 10, 20, 5);
                GenericModConfigMenuAPI.AddBoolOption(Manifest, () => ModConfig.Value.StaminaCostAdjustments, value => ModConfig.Value.StaminaCostAdjustments = value, () => Helper.Translation.Get("GMCM.StaminaCostAdjustments.Name"), () => Helper.Translation.Get("GMCM.StaminaCostAdjustments.Desc"));
                GenericModConfigMenuAPI.AddBoolOption(Manifest, () => ModConfig.Value.ProfessionsOnly, value => ModConfig.Value.ProfessionsOnly = value, () => Helper.Translation.Get("GMCM.ProfessionsOnly.Name"), () => Helper.Translation.Get("GMCM.ProfessionsOnly.Desc"));
                GenericModConfigMenuAPI.AddTextOption(Manifest, () => ModConfig.Value.TalentHintLevel, value => ModConfig.Value.TalentHintLevel = value, () => Helper.Translation.Get("GMCM.TalentHintLevel.Name"), () => Helper.Translation.Get("GMCM.TalentHintLevel.Desc"), new string[] { "Hidden", "Partial", "Full" }, option => Helper.Translation.Get($"GMCM.TalentHintLevel.Options.{option}"));
            }

            else
                ModMonitor.Log("Generic Mod Config Menu is either not installed or there was a problem while requesting the API. The config menu wont be created.", LogLevel.Info);
            if (SpaceCoreAPI is null)
                ModMonitor.Log("SpaceCore is either not installed or there was a problem while requesting the API. If its the latter, custom skill mod integrations will not work.", LogLevel.Info);
            else
            {
                SpaceCoreAPI.RegisterSerializerType(typeof(ParrotPerch));
                SpaceCoreAPI.RegisterSerializerType(typeof(TrinketRing));
                SpaceCoreAPI.RegisterSerializerType(typeof(SlingshotEnchantment));
                SpaceCoreAPI.RegisterSerializerType(typeof(ThriftyEnchantment));
                SpaceCoreAPI.RegisterSerializerType(typeof(BatKillerEnchantment));
                SpaceCoreAPI.RegisterSerializerType(typeof(AutoFireEnchantment));
                SpaceCoreAPI.RegisterSerializerType(typeof(RapidEnchantment));
            }

            if (WearMoreRingsAPI is null)
            {
                ModMonitor.Log("Wear More Rings is either not installed or there was a problem while requesting the API. If its the latter, custom ring slots will not be recognized by this mod.", LogLevel.Info);
            }

            if (ItemExtensionsAPI is null)
            {
                ModMonitor.Log("Item Extensions is either not installed or there was a problem while requesting the API. If its the latter; custom gem, ore and stone nodes will not be recognized by this mod.", LogLevel.Info);
            }
        }
        public static IEnumerable<string> GetProfessions()
        {
            if (!Context.IsWorldReady)
            {
                yield return null;
                yield break;
            }
            foreach (var item in Professions)
            {
                if (CoreUtility.CurrentPlayerHasProfession(item.Key))
                {
                    yield return item.Key;
                }
            }
        }
        /// <summary>
        /// Get the current page from the provided <see cref="GameMenu"/> or
        /// <c>BetterGameMenu</c> instance. Returns <c>null</c> if the provided
        /// menu is not one of those menus.
        /// </summary>
        /// <param name="menu">The menu to get the current page from.</param>
        public static IClickableMenu GetGameMenuPage(IClickableMenu menu)
        {
            if (menu is GameMenu gameMenu)
                return gameMenu.GetCurrentPage();
            if (BetterGameMenuAPI != null && menu != null)
                return BetterGameMenuAPI.GetCurrentPage(menu);
            return null;
        }

        public static bool IsGameMenu(IClickableMenu menu)
        {
            if (menu is GameMenu)
                return true;
            if (BetterGameMenuAPI != null)
                return BetterGameMenuAPI.AsMenu(menu) != null;
            return false;
        }

        public static IEnumerable<string> GetTalents()
        {
            if (!Context.IsWorldReady || ModConfig.Value.ProfessionsOnly)
            {
                yield return null;
                yield break;
            }
            foreach (var item in TalentCore.Talents)
            {
                if (TalentUtility.CurrentPlayerHasTalent(item.Value.MailFlag, ignoreDisabledTalents: true))
                {
                    yield return item.Value.Name;
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                var data = e.NewLocation?.GetData()?.CustomFields ?? new();

                if (e.NewLocation is MineShaft shaft)
                {
                    if (CoreUtility.CurrentPlayerHasProfession("Mine-Forage", useThisInstead: e.Player) && Game1.random.NextBool(0.15) && shaft.getMineArea(shaft.mineLevel) is 80 && !shaft.rainbowLights.Value)
                    {
                        shaft.rainbowLights.Value = true;
                        if (Context.IsMainPlayer && Context.IsMainPlayer && Context.HasRemotePlayers)
                        {
                            Helper.Multiplayer.SendMessage(true, Manifest.UniqueID + "/MushroomLevel", new string[] { Manifest.UniqueID });
                        }
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent("Fallout") && e.NewLocation is MineShaft shaft45 && shaft45?.getMineArea() is 80 or 121)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && tileobjpair.Value.IsBlandStone()
                                                 select tileobjpair.Key).ToList();
                    bool success = false;

                    Dictionary<Vector2, string> CoordinatesForMP = new();
                    for (int i = 0; i < validcoords.Count; i++)
                    {
                        if (Game1.random.NextBool(0.0001 * shaft45.mineLevel))
                        {
                            e.NewLocation.Objects[validcoords[i]] = ItemRegistry.Create<StardewValley.Object>("95");
                            e.NewLocation.Objects[validcoords[i]].MinutesUntilReady = 25;
                            CoordinatesForMP.Add(validcoords[i], e.NewLocation.Objects[validcoords[i]].ItemId);
                            success = true;
                        }
                    }
                    if (success && Context.IsMainPlayer && Context.HasRemotePlayers)
                    {
                        Helper.Multiplayer.SendMessage(CoordinatesForMP, Manifest.UniqueID + "/SwitchMineStones", new string[] { Manifest.UniqueID });
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("Fortified", who: e.Player) && e.NewLocation is not null)
                {
                    int monsters = e.NewLocation.characters.Where(item => item.IsMonster).Count();
                    if (monsters > 0)
                    {
                        BuffEffects buffEffects = new();
                        buffEffects.Defense.Value = Math.Min(8, monsters);
                        Buff buff = new("VPP.Fortified.Defense", "Fortified", "Fortified", -2, Game1.buffsIcons, 10, buffEffects, false, Helper.Translation.Get("Buff.Fortified.Name"), Game1.parseText(Helper.Translation.Get("Buff.Fortified.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(Helper.Translation.Get("Buff.Fortified.Name"))));
                        e.Player.buffs.Apply(buff);
                    }
                    else
                    {
                        e.Player.buffs.Remove("VPP.Fortified.Defense");
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent("DownInTheDepths") && e.NewLocation is MineShaft shaft6)
                {
                    if (shaft6.modData.ContainsKey(TalentCore.Key_DownInTheDepths))
                        shaft6.modData[TalentCore.Key_DownInTheDepths] = "0";
                }
                if (TalentUtility.AllPlayersHaveTalent("RoomAndPillar") && e.NewLocation is MineShaft shaft5 && shaft5.isQuarryArea)
                {
                    bool success = false;
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && (tileobjpair.Value.IsBlandStone() || (ItemExtensionsAPI?.IsStone(tileobjpair.Value.QualifiedItemId) is true && ItemExtensionsAPI?.IsResource(tileobjpair.Value.QualifiedItemId, out int? _, out string itemDropped) is true && itemDropped.Contains("390")))
                                                 select tileobjpair.Key).ToList();

                    Dictionary<Vector2, string> CoordinatesForMP = new();

                    for (int i = 0; i < validcoords.Count; i++)
                    {
                        if (Game1.random.NextBool(0.0008))
                        {
                            e.NewLocation.Objects[validcoords[i]] = ItemRegistry.Create<StardewValley.Object>(TalentUtility.GetNodeForRoomAndPillar());
                            e.NewLocation.Objects[validcoords[i]].MinutesUntilReady = 25;
                            CoordinatesForMP.Add(validcoords[i], e.NewLocation.Objects[validcoords[i]].ItemId);
                            success = true;
                        }
                    }
                    if (success && Context.IsMainPlayer && Context.HasRemotePlayers)
                    {
                        Helper.Multiplayer.SendMessage(CoordinatesForMP, Manifest.UniqueID + "/SwitchMineStones", new string[] { Manifest.UniqueID });
                    }
                }
                if (data?.ContainsKey(TalentCore.Key_CrystalCavern) is true || data?.ContainsKey(TalentCore.Key_Upheaval) is true || e.NewLocation is MineShaft or VolcanoDungeon)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && tileobjpair.Value.IsBlandStone()
                                                 select tileobjpair.Key).ToList();

                    if (TalentUtility.AllPlayersHaveTalent("CrystalCavern") && (data?.ContainsKey(TalentCore.Key_CrystalCavern) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(0.003))
                    {
                        TalentUtility.GemAndGeodeNodes(true, validcoords, Game1.player.currentLocation);
                    }
                    else if (TalentUtility.AllPlayersHaveTalent("Upheaval") && (data?.ContainsKey(TalentCore.Key_Upheaval) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(0.003))
                    {
                        TalentUtility.GemAndGeodeNodes(false, validcoords, Game1.player.currentLocation);
                    }
                }
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (Game1.activeClickableMenu is null)
            {
                TalentCore.IsDayStartOrEnd = false;
                if (e.Button.IsUseToolButton())
                {
                    if (Game1.player.currentLocation.terrainFeatures.TryGetValue(e.Cursor.GrabTile, out TerrainFeature value))
                    {
                        if (Game1.player.ActiveObject?.QualifiedItemId == "(O)Kedi.VPP.GemDust")
                        {
                            string fertilizer = ManagerUtility.GemDustToFertilizer(Game1.player.ActiveObject);
                            if (value is Tree wildTree && !wildTree.fertilized.Value && fertilizer == "(O)805")
                            {
                                wildTree.fertilize();
                                ManagerUtility.FertilizerStackEffects();
                            }
                            else if (value is HoeDirt hoeDirt && !hoeDirt.HasFertilizer() && fertilizer != "(O)805")
                            {
                                hoeDirt.fertilizer.Value = fertilizer;
                                ManagerUtility.FertilizerStackEffects();
                            }
                        }
                        else if (value is FruitTree)
                        {
                            TerrainFeatureTapper(value, e);
                        }
                        else if (Game1.player.ActiveObject is Trinket trinket && trinket?.QualifiedItemId == "(TR)FairyBox" && TalentUtility.CurrentPlayerHasTalent("HiddenBenefits"))
                        {
                            if (value is HoeDirt dirt && dirt.crop is not null && !dirt.crop.fullyGrown.Value && dirt.crop.currentPhase.Value != dirt.crop.phaseDays.Count - 1)
                            {
                                bool shouldGrow = false;
                                if (!trinket.modData.TryAdd(TalentCore.Key_HiddenBenefit_FairyBox, "1"))
                                {
                                    if (trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox] is not "3")
                                    {
                                        shouldGrow = true;
                                        trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox] = (int.Parse(trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox]) + 1).ToString();
                                    }
                                    else
                                        Game1.showGlobalMessage(Helper.Translation.Get("Message.FairyBreak"));
                                }
                                else
                                    shouldGrow = true;
                                if (shouldGrow)
                                {
                                    if (!dirt.crop.modData.TryAdd(TalentCore.Key_HiddenBenefit_Crop, "true"))
                                    {
                                        if (dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] == "true")
                                            Game1.showGlobalMessage(Helper.Translation.Get("Message.AlreadyFertilized"));
                                        else
                                        {
                                            Game1.playSound("wand");
                                            dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] = "true";
                                            Game1.showGlobalMessage(Helper.Translation.Get("Message.Fertilized"));
                                        }
                                    }
                                    else
                                    {
                                        Game1.playSound("wand");
                                        Game1.showGlobalMessage(Helper.Translation.Get("Message.Fertilized"));
                                        dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] = "true";
                                    }
                                }
                            }
                        }
                    }
                    else if (Game1.player.CurrentTool is FishingRod rod && TalentUtility.CurrentPlayerHasTalent("TakeItSlow"))
                    {
                        if (rod.CanUseTackle() && rod.GetTackleQualifiedItemIDs().Contains("(O)Kedi.VPP.SnailTackle"))
                        {
                            rod.castingTimerSpeed = 0.0007f;
                        }
                        else
                        {
                            rod.castingTimerSpeed = 0.001f;
                        }
                    }

                    else if (Game1.player.ActiveObject?.QualifiedItemId == "(TR)BasiliskPaw" && TalentUtility.CurrentPlayerHasTalent("HiddenBenefits") && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.Tile, out var value2) && !value2.HasTypeBigCraftable() && value2.Category != StardewValley.Object.litterCategory)
                    {
                        if (Game1.player.couldInventoryAcceptThisItem(value2))
                        {
                            Game1.player.addItemByMenuIfNecessary(value2);
                            Game1.player.currentLocation.Objects.Remove(e.Cursor.Tile);
                        }
                        else
                        {
                            Game1.hudMessages.Add(new("Full inventory", HUDMessage.error_type));
                        }
                    }
                    else if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.Tile, out value2) && value2 is not ParrotPerch && value2.QualifiedItemId == "(BC)Kedi.VPP.HiddenBenefits.ParrotPerch" && TalentUtility.CurrentPlayerHasTalent("HiddenBenefits"))
                    {
                        Game1.player.currentLocation.Objects[e.Cursor.Tile] = new ParrotPerch(e.Cursor.Tile, "Kedi.VPP.HiddenBenefits.ParrotPerch", false);
                    }
                    else if (Game1.player.ActiveObject?.Category == StardewValley.Object.litterCategory && !Game1.player.currentLocation.Objects.ContainsKey(e.Cursor.GrabTile))
                    {
                        if (Game1.player.currentLocation.Objects.TryAdd(e.Cursor.GrabTile, Game1.player.ActiveObject.getOne() as StardewValley.Object) && Game1.player.Tile != e.Cursor.GrabTile)
                        {
                            Game1.player.ActiveObject.Stack--;
                            if (Game1.player.ActiveObject.Stack == 0)
                            {
                                Game1.player.ActiveObject = null;
                            }
                        }
                    }
                    //There's a false because nothing here is supposed to be accessed yet.
                    if (false && Game1.player.ActiveObject?.QualifiedItemId is "(O)KediDili.VPPData.CP_MossyFertilizer" or "(O)KediDili.VPPData.CP_WildTotem" or "(O)KediDili.VPPData.CP_SunTotem" or "(O)KediDili.VPPData.CP_SnowTotem")
                    {
                        CraftableHandler.OnInteract(Game1.player, Game1.player.ActiveObject);
                    }
                    else if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object value3))
                    {
                        if (value3.heldObject.Value?.heldObject.Value is Chest chest && TalentUtility.CurrentPlayerHasTalent("Fertigation"))
                        {
                            chest.GetMutex().RequestLock(chest.ShowMenu);
                        }
                        MachineryEventHandler.OnMachineInteract(value3, Game1.player);
                    }
                    else
                    {
                        foreach (var item in Game1.player.currentLocation.resourceClumps)
                        {
                            if (item is GiantCrop crop)
                            {
                                var rect = crop.getBoundingBox();
                                if (rect.Contains(e.Cursor.AbsolutePixels))
                                {
                                    TerrainFeatureTapper(crop, e);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (e.Button.IsActionButton())
                {
                    if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object value))
                    {
                        if (CoreUtility.CurrentPlayerHasProfession("Farm-Forage") && value.IsTapper())
                            value.shakeTimer = 0;
                        else if (value.heldObject.Value is null && value.IsSprinkler() && Game1.player.ActiveObject?.QualifiedItemId is "(O)Kedi.VPP.Fertigator" && TalentUtility.CurrentPlayerHasTalent("Fertigation"))
                        {
                            Chest chest = new();
                            StardewValley.Object ToPlace = Game1.player.ActiveObject.getOne() as StardewValley.Object;
                            if (ToPlace is not null)
                            {
                                chest.MinutesUntilReady = -1;
                                chest.SpecialChestType = Chest.SpecialChestTypes.Enricher;
                                ToPlace.heldObject.Value = chest;
                            }
                            ToPlace.MinutesUntilReady = -1;
                            value.heldObject.Value = ToPlace;
                            Game1.player.ActiveObject.Stack--;
                            if (Game1.player.ActiveObject.Stack == 0)
                            {
                                Game1.player.ActiveObject = null;
                            }
                        }
                        if (value is Chest chest4 && chest4.QualifiedItemId == "(BC)216")
                        {
                            if (TalentUtility.AnyPlayerHasTalent("MiniFridgeBigSpace"))
                            {
                                chest4.SpecialChestType = Chest.SpecialChestTypes.BigChest;
                            }
                        }
                    }
                    else if (Game1.player.ActiveObject?.QualifiedItemId is "(O)96" or "(O)97" or "(O)98" or "(O)99" && Game1.player.canUnderstandDwarves && TalentUtility.CurrentPlayerHasTalent("ElderScrolls"))
                    {
                        Game1.player.gainExperience(3, 250);
                        Game1.player.currentLocation.playSound("shwip");
                        var msg = HUDMessage.ForItemGained(Game1.player.ActiveObject, 1, "ElderScrolls");
                        msg.message = Helper.Translation.Get("Message.ReadDwarfScroll");
                        Game1.addHUDMessage(msg);
                        if (Game1.player.ActiveObject.ConsumeStack(1) is null)
                        {
                            Game1.player.ActiveObject = null;
                        }
                    }
                    else if (Game1.player.CurrentTool is not null and Slingshot slingshot && TalentUtility.CurrentPlayerHasTalent("TripleShot"))
                    {
                        foreach (var item in Game1.player.enchantments)
                            if (item is AutoFireEnchantment) //Balance
                                return;
                        if (TalentCore.TripleShotCooldown <= 0)
                        {
                            slingshot.beginUsing(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
                            slingshot.lastUser = Game1.player;
                            TalentCore.TripleShotCooldown = 5000;
                            Game1.player.usingSlingshot = true;
                            TalentCore.IsActionButtonUsed.Value = true;
                        }
                        else
                        {
                            Game1.player.doEmote(Character.xEmote);
                        }
                    }
                }
            }
            else if (IsGameMenu(Game1.activeClickableMenu))
            {
                var menuPage = GetGameMenuPage(Game1.activeClickableMenu);
                if (menuPage is SkillsPage page)
                {
                    if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
                    {
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.LittlePlus.Value.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            DisplayHandler.IsOverlayActive.Value = !DisplayHandler.IsOverlayActive.Value;
                            page.skillBars = DisplayHandler.IsOverlayActive.Value ? DisplayHandler.MyCustomSkillBars.Value.ToList() : DisplayHandler.VanillaSkillBars.Value.ToList();
                        }
                        AssignIDs(page.skillBars);
                    }
                    else
                    {
                        AssignIDs(null);
                    }
                }
                else if (menuPage is NewSkillsPage pagee)
                {
                    pagee.allClickableComponents.Add(DisplayHandler.LittlePlus.Value);
                    DisplayHandler.MyCustomSkillBars.Value = pagee.skillBars.ToArray();
                    NewSkillsPage skillsPage2 = new(Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen, Game1.activeClickableMenu.width + ((LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.it) ? 64 : 0), Game1.activeClickableMenu.height);
                    DisplayHandler.VanillaSkillBars.Value = skillsPage2.skillBars.ToArray();
                    if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
                    {
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.LittlePlus.Value.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            List<(int, string)> IndexAndProfessions = new();
                            List<string> AlreadyPickedProfessions = new();

                            for (int i = 0; i < DisplayHandler.MyCustomSkillBars.Value.Length; i++)
                            {
                                foreach (var item in Professions)
                                {
                                    if (DisplayHandler.IsInCorrectLine(DisplayHandler.MyCustomSkillBars.Value[i].bounds, pagee.skillAreas, item.Value.Skill.ToString()))
                                    {
                                        if (CoreUtility.CurrentPlayerHasProfession(item.Key, ignoreMode: true) && !AlreadyPickedProfessions.Contains(item.Value.ID.ToString()))
                                        {
                                            var list = Game1.player.professions;
                                            IndexAndProfessions.Add((i, item.Value.ID.ToString()));
                                            AlreadyPickedProfessions.Add(item.Value.ID.ToString());
                                            break;
                                        }
                                        else
                                        {
                                            DisplayHandler.MyCustomSkillBars.Value[i].name = "-1";
                                        }
                                    }
                                }
                                foreach (var item in TalentCore.SkillsByName.Values)
                                {
                                    if (DisplayHandler.IsInCorrectLine(DisplayHandler.MyCustomSkillBars.Value[i].bounds, pagee.skillAreas, item.Id))
                                    {
                                        foreach (var pair in item.ProfessionsForLevels)
                                        {
                                            if (pair.Level >= SpaceCoreAPI.GetLevelForCustomSkill(Game1.player, item.Id))
                                            {
                                                if (CoreUtility.CurrentPlayerHasProfession(pair.First.GetVanillaId().ToString(), ignoreMode: true) && !AlreadyPickedProfessions.Contains(NewSkillsPage.CustomSkillPrefix + pair.First.GetVanillaId()))
                                                {
                                                    IndexAndProfessions.Add((i, NewSkillsPage.CustomSkillPrefix + pair.First.Id + "/" + item.Id));
                                                    AlreadyPickedProfessions.Add(NewSkillsPage.CustomSkillPrefix + pair.First.Id);
                                                    break;
                                                }
                                                else if (CoreUtility.CurrentPlayerHasProfession(pair.Second.GetVanillaId().ToString(), ignoreMode: true) && !AlreadyPickedProfessions.Contains(NewSkillsPage.CustomSkillPrefix + pair.Second.GetVanillaId()))
                                                {
                                                    IndexAndProfessions.Add((i, NewSkillsPage.CustomSkillPrefix + pair.Second.Id + "/" + item.Id));
                                                    AlreadyPickedProfessions.Add(NewSkillsPage.CustomSkillPrefix + pair.Second.Id);
                                                    break;
                                                }
                                                else
                                                {
                                                    DisplayHandler.MyCustomSkillBars.Value[i].name = "-1";
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < IndexAndProfessions.Count; i++)
                            {
                                int index = i;

                                List<string> description = new();
                                string toBeMeasured = "";
                                string[] splitted = IndexAndProfessions[index].Item2.Split('/');
                                if (IndexAndProfessions[index].Item2.StartsWith(NewSkillsPage.CustomSkillPrefix))
                                {
                                    foreach (var item in Skills.GetSkill(splitted[1]).Professions)
                                    {
                                        if (NewSkillsPage.CustomSkillPrefix + item.Id == splitted[0])
                                        {
                                            toBeMeasured = item.GetName();
                                            description.Add(item.GetDescription());
                                            break;
                                        }
                                    }
                                    DisplayHandler.MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].name = splitted[0];
                                }
                                else
                                {
                                    DisplayHandler.MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].name = IndexAndProfessions[index].Item2;
                                    description = LevelUpMenu.getProfessionDescription(int.Parse(IndexAndProfessions[index].Item2));
                                    description.RemoveAt(0);
                                    toBeMeasured = LevelUpMenu.getProfessionTitleFromNumber(int.Parse(IndexAndProfessions[index].Item2));
                                }

                                DisplayHandler.MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].hoverText = Game1.parseText(description.Join(delimiter: "\n"), Game1.smallFont, (int)Game1.dialogueFont.MeasureString(toBeMeasured).X + 100);
                            }
                            DisplayHandler.IsOverlayActive.Value = !DisplayHandler.IsOverlayActive.Value;
                            pagee.skillBars = DisplayHandler.IsOverlayActive.Value ? DisplayHandler.MyCustomSkillBars.Value.ToList() : DisplayHandler.VanillaSkillBars.Value.ToList();
                            AssignIDs(pagee.skillBars);
                        }
                    }
                    else
                    {
                        AssignIDs(null);
                    }
                }
                void AssignIDs(List<ClickableTextureComponent> skillBars)
                {
                    if (skillBars is not null)
                    {
                        foreach (var item in skillBars)
                        {
                            if (item.downNeighborID == -1)
                            {
                                item.downNeighborID = 46780;
                                break;
                            }
                        }
                    }
                    if (DisplayHandler.MyCustomSkillBars.Value is not null)
                    {
                        foreach (var item in DisplayHandler.MyCustomSkillBars.Value)
                        {
                            if (item.downNeighborID == -1)
                            {
                                item.downNeighborID = 46780;
                                break;
                            }
                        }
                    }
                    if (DisplayHandler.VanillaSkillBars.Value is not null)
                    {
                        foreach (var item in DisplayHandler.VanillaSkillBars.Value)
                        {
                            if (item.downNeighborID == -1)
                            {
                                item.downNeighborID = 46780;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private static void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (TalentCore.IsActionButtonUsed.Value && e.Button.IsActionButton() && Game1.player.CurrentTool is not null and Slingshot slingshot)
            {
                CoreUtility.PerformFire(Game1.player.currentLocation, Game1.player, slingshot);
                slingshot.canPlaySound = false;
                slingshot.PerformFire(Game1.player.currentLocation, Game1.player);
                TalentCore.IsActionButtonUsed.Value = false;
                Game1.player.completelyStopAnimatingOrDoingAction();
            }
            if (IsGameMenu(Game1.activeClickableMenu))
            {
                if ((e.Button.IsUseToolButton() || e.Button.IsActionButton()) && DisplayHandler.OpenTalentMenuCooldown.Value && !ModConfig.Value.ProfessionsOnly)
                {
                    var menuPage = GetGameMenuPage(Game1.activeClickableMenu);
                    if (menuPage is SkillsPage page)
                    {
                        for (int i = 0; i < page.skillAreas.Count; i++)
                        {
                            if (page.skillAreas[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i);
                            }
                        }
                    }
                    else if (menuPage is NewSkillsPage pagee)
                    {
                        for (int i = 0; i < pagee.skillAreas.Count; i++)
                        {
                            if (pagee.skillAreas[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i);
                            }
                        }
                    }
                }
            }
        }
        private static void TerrainFeatureTapper(TerrainFeature TreeOrCrop, ButtonPressedEventArgs e)
        {
            if (TreeOrCrop is null)
                return;
            if (TreeOrCrop is FruitTree tree11 && tree11.GetData() is null)
            {
                return;
            }
            else if (TreeOrCrop is GiantCrop giantcrop && giantcrop.GetData() is null)
            {
                return;
            }
            else if (TreeOrCrop is not FruitTree && TreeOrCrop is not GiantCrop)
            {
                return;
            }

            if (Game1.player.CurrentTool is Pickaxe or Axe && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object obj) && obj.IsTapper())
            {
                TreeOrCrop.modData[Key_TFHasTapper] = "false";
            }
            else if (CoreUtility.CurrentPlayerHasProfession("Farm-Forage"))
            {
                if (Game1.player.ActiveObject?.IsHeldOverHead() == true && Game1.player.ActiveObject?.IsTapper() == true && e.Button.IsUseToolButton())
                {
                    FruitTreeData fruitTreeData = (TreeOrCrop as FruitTree)?.GetData();
                    GiantCropData giantCropData = (TreeOrCrop as GiantCrop)?.GetData();
                    var obsj = Game1.player.ActiveObject;

                    obsj.modData[Key_TFTapperDaysLeft] = "0";

                    StardewValley.Object dsdsd;
                    if (fruitTreeData?.CustomFields?.TryGetValue(Key_FruitTreeOrGiantCrop, out string value) is true && value is not null)
                    {
                        dsdsd = ItemRegistry.Create<StardewValley.Object>(value);
                        dsdsd.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                        obsj.modData[Key_TFTapperDaysLeft] = (dsdsd.Price / 20).ToString();
                    }
                    else if (giantCropData?.CustomFields?.TryGetValue(Key_FruitTreeOrGiantCrop, out string value2) is true && value2 is not null)
                    {
                        dsdsd = ItemRegistry.Create<StardewValley.Object>(value2);
                        dsdsd.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                        obsj.modData[Key_TFTapperDaysLeft] = (dsdsd.Price / 20).ToString();
                    }
                    else
                    {
                        obsj.modData[Key_TFTapperDaysLeft] = ManagerUtility.GetProduceTimeBasedOnPrice(TreeOrCrop, out StardewValley.Object produce);
                        dsdsd = produce;
                        dsdsd.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Kedi.VPP.FruitSyrup");
                    }
                    if (dsdsd is not null)
                        obsj.lastInputItem.Value = dsdsd;

                    if (TreeOrCrop.modData.TryAdd(Key_TFHasTapper, "true")
                    && TreeOrCrop.modData.TryAdd(Key_TFTapperID, Game1.player.ActiveObject.QualifiedItemId))
                    {
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(ManagerUtility.GetFeatureTile(TreeOrCrop), obsj);
                    }
                    else if (TreeOrCrop.modData.TryGetValue(Key_TFHasTapper, out var value2) && TreeOrCrop.modData.ContainsKey(Key_TFTapperID) && value2 is "false")
                    {
                        TreeOrCrop.modData[Key_TFHasTapper] = "true";
                        TreeOrCrop.modData[Key_TFTapperID] = Game1.player.ActiveObject.QualifiedItemId;
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(ManagerUtility.GetFeatureTile(TreeOrCrop), obsj);
                    }
                    else if (value2 is "true" && TreeOrCrop.modData.ContainsKey(Key_TFTapperID) && !TreeOrCrop.Location.Objects.TryGetValue(TreeOrCrop.Tile, out StardewValley.Object _))
                    {
                        TreeOrCrop.modData[Key_TFHasTapper] = "false";
                    }
                }
                else if (TreeOrCrop is FruitTree tree)
                {
                    tree.shake(TreeOrCrop.Tile, true);
                }
            }
        }
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            TalentCore.IsDayStartOrEnd = true;
            bool Orchardist = CoreUtility.AnyPlayerHasProfession("Orchardist");
            bool Accumulation = TalentUtility.AnyPlayerHasTalent("Accumulation");
            bool Abundance = TalentUtility.AnyPlayerHasTalent("Abundance");

            Utility.ForEachItem(item =>
            {
                if (item is not null and StardewValley.Object obj)
                {
                    if (obj is CrabPot crabPot)
                    {
                        if (crabPot.heldObject.Value is not null)
                        {
                            if (TalentUtility.CurrentPlayerHasTalent("BaitAndSwitch", crabPot.owner.Value) && crabPot.heldObject.Value.HasContextTag("fish_crab_pot") is true && Game1.random.NextBool(0.05))
                            {
                                crabPot.heldObject.Value.Quality++;
                                crabPot.heldObject.Value.FixQuality();
                            }
                        }
                        else if (crabPot.bait.Value is not null && TalentUtility.CurrentPlayerHasTalent("FishsWishes", crabPot.owner.Value))
                        {
                            var list = crabPot.Location.GetData().Fish;

                            ItemQueryContext context = new(crabPot.Location, Game1.player, Game1.random, "Fish's Wishes context");

                            var normalData = (from keyvaluepair in DataLoader.Fish(Game1.content)
                                              where !keyvaluepair.Value.Contains("trap")
                                              select keyvaluepair.Key).ToList();

                            List<string> locFishList = new();
                            foreach (var fesh in list)
                            {
                                ItemQueryResult result = ItemQueryResolver.TryResolve(fesh, context).FirstOrDefault();
                                if (fesh?.ItemId is not null and "" && result?.Item?.QualifiedItemId is not null)
                                {
                                    if (result.Item.QualifiedItemId.StartsWith("(O)"))
                                    {
                                        locFishList.Add(fesh.ItemId);
                                    }
                                }
                            }

                            if (locFishList.Count > 0)
                            {
                                var endList = normalData.Intersect(locFishList).ToList();
                                if (endList.Count > 0)
                                {
                                    crabPot.heldObject.Value = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(endList), quality: crabPot.heldObject.Value.Quality);
                                }
                            }
                        }
                    }
                    if (obj.IsTapper() is true && obj.heldObject.Value is not null)
                    {
                        if (Orchardist && Game1.random.NextBool(0.05) && obj.Location.terrainFeatures.TryGetValue(obj.TileLocation, out TerrainFeature terrainFeature) && terrainFeature is Tree or FruitTree or GiantCrop && obj.heldObject.Value.Stack < 5)
                        {
                            obj.heldObject.Value.Stack += Game1.random.Next(1, 3);
                        }
                        if (Accumulation)
                        {
                            if (Game1.random.NextBool(0.005))
                            {
                                obj.heldObject.Value.Quality++;
                                obj.heldObject.Value.FixQuality();
                            }
                        }
                    }
                    if (obj.QualifiedItemId == "(BC)10" && obj.heldObject.Value is not null)
                    {
                        int tiles = TalentUtility.FlowersInBeeHouseRange(obj.Location, obj.TileLocation);
                        if (Abundance && Game1.random.NextBool(tiles * 0.05))
                        {
                            obj.heldObject.Value.Quality++;
                            obj.heldObject.Value.FixQuality();
                        }
                    }
                }
                return true;
            });

            Utility.ForEachBuilding<Building>(building =>
            {
                if (building.GetIndoors() is SlimeHutch slimeHutch && CoreUtility.CurrentPlayerHasProfession("Combat-Farm", farmerID: building.owner.Value))
                {
                    var str = (!slimeHutch.waterSpots.Contains(true)).ToString().ToLower();
                    if (!slimeHutch.modData.TryAdd(Key_IsSlimeHutchWatered, str))
                    {
                        slimeHutch.modData[Key_IsSlimeHutchWatered] = str;
                    }
                }
                else if (building is FishPond fishPond && TalentUtility.CurrentPlayerHasTalent("Ex-squid-site", farmerID: fishPond.owner.Value))
                {
                    if (fishPond.output.Value is not null && fishPond.output.Value.QualifiedItemId is "(O)812" or "(O)814" && Game1.random.NextBool(0.24))
                    {
                        fishPond.output.Value.Quality++;
                        fishPond.output.Value.FixQuality();
                    }
                }
                return true;
            }
            );
            Utility.ForEachLocation(loc =>
            {
                if (!loc.modData.TryAdd(TalentCore.Key_WasRainingHere, loc.IsRainingHere().ToString().ToLower()))
                {
                    loc.modData[TalentCore.Key_WasRainingHere] = loc.IsRainingHere().ToString().ToLower();
                }
                return true;
            });
            if (!Game1.player.modData.TryAdd(TalentCore.Key_TalentPoints, TalentCore.TalentPointCount.Value.ToString()))
                Game1.player.modData[TalentCore.Key_TalentPoints] = TalentCore.TalentPointCount.Value.ToString();
            if (!Game1.player.modData.TryAdd(TalentCore.Key_DisabledTalents, string.Join('|', TalentCore.DisabledTalents.ToArray())))
            {
                Game1.player.modData[TalentCore.Key_DisabledTalents] = string.Join('|', TalentCore.DisabledTalents.ToArray());
            }
        }
        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLevel > e.OldLevel)
            {
                if (e.NewLevel is 15 or 20 && (int)e.Skill < 5)
                {
                    DisplayHandler.ShouldHandleSkillPage.Value = true;
                }
                if (IsUninstalling.Value)
                {
                    TalentCore.TalentPointCount.ResetAllScreens();
                }
                else if (e.OldLevel + 1 == e.NewLevel && !IsRecalculatingPoints.Value)
                {
                    TalentCore.AddTalentPoint();
                }
            }
        }
    }
}
