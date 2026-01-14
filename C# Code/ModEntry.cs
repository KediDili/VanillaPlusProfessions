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
using VanillaPlusProfessions.Talents.Patchers;

namespace VanillaPlusProfessions
{
    public class ModEntry : Mod
    {
        internal readonly static PerScreen<ModEntry> CoreModEntry = new(createNewState: () => new());

        internal new IModHelper Helper;
        internal IMonitor ModMonitor;
        internal IManifest Manifest;

        internal IContentPatcher ContentPatcherAPI;
        internal IGenericModConfigMenu GenericModConfigMenuAPI;
        internal ISpaceCore SpaceCoreAPI;
        internal IWearMoreRings WearMoreRingsAPI;
        internal IItemExtensions ItemExtensionsAPI;
        internal IBetterGameMenuApi BetterGameMenuAPI;
        internal IExtraAnimalConfigApi ExtraAnimalConfigAPI;

        internal VanillaPlusProfessionsAPI VanillaPlusProfessionsAPI = new();

        internal CustomQueries CustomQueries = new();
        internal Harmony Harmony { get; } = new("KediDili.VanillaPlusProfessions");

        internal static IProfessionManager[] Managers = new IProfessionManager[6];
        internal int[] levelExperiences;
        internal static GameLocation EmptyCritterRoom;

        internal bool IsUninstalling;
        internal bool IsRecalculatingPoints;
        internal Config ModConfig;

        //So mods can access it without needing reflection.
        public static Dictionary<string, Profession> Professions = new();

        public override void Entry(IModHelper helper)
        {
            CoreModEntry.Value = this;
            CoreModEntry.Value.Helper = helper;
            CoreModEntry.Value.ModMonitor = Monitor;
            CoreModEntry.Value.Manifest = ModManifest;

            CoreModEntry.Value.ModConfig = Helper.ReadConfig<Config>();
            CoreModEntry.Value.levelExperiences = Helper.Data.ReadJsonFile<int[]>("assets/levelExperiences.json");
            Professions = Helper.Data.ReadJsonFile<Dictionary<string, Profession>>("assets/professions.json");

            ContentEditor.CoreContentEditor.Value.Initialize(this);
            DisplayHandler.CoreDisplayHandler.Value.Initialize(this);

            CoreModEntry.Value.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            CoreModEntry.Value.Helper.Events.Input.ButtonPressed += OnButtonPressed;
            CoreModEntry.Value.Helper.Events.Input.ButtonReleased += OnButtonReleased;
            CoreModEntry.Value.Helper.Events.GameLoop.DayStarted += DayStartHandler.OnDayStarted;
            CoreModEntry.Value.Helper.Events.GameLoop.DayEnding += OnDayEnding;
            CoreModEntry.Value.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            CoreModEntry.Value.Helper.Events.Player.LevelChanged += OnLevelChanged;
            CoreModEntry.Value.Helper.Events.Player.Warped += OnWarped;
            CoreModEntry.Value.Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
<<<<<<< Updated upstream
=======
            CoreModEntry.Value.Helper.Events.World.ObjectListChanged += OnInventoryChanged;
>>>>>>> Stashed changes

            CorePatcher.ApplyPatches();
            TalentCore.TalentCoreEntry.Value.Initialize(this);
            BuildingPatcher.ApplyPatches();
            CraftablePatcher.ApplyPatches();
            MachineryPatcher.ApplyPatches();

            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.removeAll", "Removes all professions, talents and metadata added by Vanilla Plus Professions if added true after writing the command. Use only for testing or uninstalling.", CoreUtility.remove);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.recalculatepoints", "Recalculates all talent points, useful for existing saves that are being loaded for the first time with this mod.", CoreUtility.recalculate);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.details", "Prints out skill related information. Might be useful for troubleshooting.", CoreUtility.details);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.reset", "Can be used to reset professions added by VPP. First parameter is the level (15 or 20), second is the level (0 - Farming, 1 - Fishing, 2 - Foraging, 3 - Mining or 4 - Combat)", ManagerUtility.reset);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.showXPLeft", "Shows how much XP left for the next level in all vanilla skills.", CoreUtility.showXPLeft);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.clearTrinkets", "It's a dummy command, supposed to be used ONLY by the mod's devs or beta users when instructed.", CoreUtility.clearTrinkets);
            CoreModEntry.Value.Helper.ConsoleCommands.Add("vpp.test", "It's a dummy command, supposed to be used ONLY by the mod's devs or beta users when instructed.", CoreUtility.Test);

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
            CoreModEntry.Value.ModMonitor.Log("Mod with the name of " + mod.Manifest.Name + " and with the unique ID of " + mod.Manifest.UniqueID + " has requested the API.");
            return CoreModEntry.Value.VanillaPlusProfessionsAPI;
        }

        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModEntry me = GetMe();
            DisplayHandler.CoreDisplayHandler.Value.XPDisplayInstalled = me.Helper.ModRegistry.IsLoaded("Shockah.XPDisplay");
            if (me.ModConfig.MasteryCaveChanges != 10 && me.ModConfig.MasteryCaveChanges != 15 && me.ModConfig.MasteryCaveChanges != 20)
            {
                me.ModConfig.MasteryCaveChanges = 20;
                me.ModMonitor.Log("Mastery Cave Changes was changed to an invalid value. Modified it to 20.", LogLevel.Warn);
            }
            try
            {
                me.ContentPatcherAPI = me.Helper.ModRegistry.GetApi<IContentPatcher>(Constants.ModId_ContentPatcher);
                me.GenericModConfigMenuAPI = me.Helper.ModRegistry.GetApi<IGenericModConfigMenu>(Constants.ModId_GenericModConfigMenu);
                me.SpaceCoreAPI = me.Helper.ModRegistry.GetApi<ISpaceCore>(Constants.ModId_SpaceCore);
                me.WearMoreRingsAPI = me.Helper.ModRegistry.GetApi<IWearMoreRings>(Constants.ModId_WearMoreRings);
                me.ItemExtensionsAPI = me.Helper.ModRegistry.GetApi<IItemExtensions>(Constants.ModId_ItemExtensions);
                me.BetterGameMenuAPI = me.Helper.ModRegistry.GetApi<IBetterGameMenuApi>(Constants.ModId_BetterGameMenu);
                me.ExtraAnimalConfigAPI = me.Helper.ModRegistry.GetApi<IExtraAnimalConfigApi>(Constants.ModId_ExtraAnimalConfig);
            }
            catch (Exception)
            {
                me.ModMonitor.Log("Something has seriously gone wrong with an API request. This could indicate VPP's versions of APIs being out of date, outright the wrong type or some other error. Little to no interactions may work this session.");
            }

            me.CustomQueries.Initialize();
            DisplayHandler.CoreDisplayHandler.Value.InitializeBetterGameMenu();

            if (me.ContentPatcherAPI is not null)
            {
                me.ContentPatcherAPI.RegisterToken(me.Manifest, "HasProfessions", GetProfessions);
                me.ContentPatcherAPI.RegisterToken(me.Manifest, "HasTalents", new HasTalents());
                me.ContentPatcherAPI.RegisterToken(me.Manifest, "ContentPaths", new ContentPaths());
                me.ContentPatcherAPI.RegisterToken(me.Manifest, "ProfessionsOnly", () => new string[] { me.ModConfig.ProfessionsOnly.ToString() });
            }
            else
                me.ModMonitor.Log("Content Patcher is either not installed or there was a problem while requesting the API. Skipping token additions.", LogLevel.Info);
            if (me.GenericModConfigMenuAPI is not null)
            {
                me.GenericModConfigMenuAPI.Register(me.Manifest, () => me.ModConfig = new Config(), () => SaveConfig());
                me.GenericModConfigMenuAPI.AddSectionTitle(me.Manifest, () => me.Helper.Translation.Get("GMCM.MainOptionsSection.Name"));
                me.GenericModConfigMenuAPI.AddBoolOption(me.Manifest, () => me.ModConfig.ColorBlindnessChanges, value => me.ModConfig.ColorBlindnessChanges = value, () => me.Helper.Translation.Get("GMCM.ColorBlindnessChanges.Name"), () => me.Helper.Translation.Get("GMCM.ColorBlindnessChanges.Desc"));
                me.GenericModConfigMenuAPI.AddBoolOption(me.Manifest, () => me.ModConfig.DeveloperOrTestingMode, value => me.ModConfig.DeveloperOrTestingMode = value, () => me.Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Name"), () => me.Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.MasteryCaveChanges, value => me.ModConfig.MasteryCaveChanges = (int)value, () => me.Helper.Translation.Get("GMCM.MasteryCaveChanges.Name"), () => me.Helper.Translation.Get("GMCM.MasteryCaveChanges.Desc"), 10, 20, 5);
                me.GenericModConfigMenuAPI.AddBoolOption(me.Manifest, () => me.ModConfig.StaminaCostAdjustments, value => me.ModConfig.StaminaCostAdjustments = value, () => me.Helper.Translation.Get("GMCM.StaminaCostAdjustments.Name"), () => me.Helper.Translation.Get("GMCM.StaminaCostAdjustments.Desc"));
                me.GenericModConfigMenuAPI.AddBoolOption(me.Manifest, () => me.ModConfig.ProfessionsOnly, value => me.ModConfig.ProfessionsOnly = value, () => me.Helper.Translation.Get("GMCM.ProfessionsOnly.Name"), () => me.Helper.Translation.Get("GMCM.ProfessionsOnly.Desc"));
                me.GenericModConfigMenuAPI.AddTextOption(me.Manifest, () => me.ModConfig.TalentHintLevel, value => me.ModConfig.TalentHintLevel = value, () => me.Helper.Translation.Get("GMCM.TalentHintLevel.Name"), () => me.Helper.Translation.Get("GMCM.TalentHintLevel.Desc"), new string[] { "Hidden", "Partial", "Full" }, option => me.Helper.Translation.Get($"GMCM.TalentHintLevel.Options.{option}"));
                me.GenericModConfigMenuAPI.AddKeybindList(me.Manifest, () => me.ModConfig.TalentMenuKeybind, value => me.ModConfig.TalentMenuKeybind = value, () => me.Helper.Translation.Get("GMCM.TalentMenuKeybind.Name"), () => me.Helper.Translation.Get("GMCM.TalentMenuKeybind.Desc"));
                me.GenericModConfigMenuAPI.AddSectionTitle(me.Manifest, () => me.Helper.Translation.Get("GMCM.BalanceSection.Name"));
                me.GenericModConfigMenuAPI.AddParagraph(me.Manifest, () => me.Helper.Translation.Get("GMCM.BalanceSection.Paragraph"));
                //Chances
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.CycleOfLife_Chance, value => me.ModConfig.CycleOfLife_Chance = value, () => me.Helper.Translation.Get("GMCM.CycleOfLifeChance.Name"), () => me.Helper.Translation.Get("GMCM.CycleOfLifeChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.WildGrowth_Chance, value => me.ModConfig.WildGrowth_Chance = value, () => me.Helper.Translation.Get("GMCM.WildGrowthChance.Name"), () => me.Helper.Translation.Get("GMCM.WildGrowthChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Fallout_Chance, value => me.ModConfig.Fallout_Chance = value, () => me.Helper.Translation.Get("GMCM.FalloutChance.Name"), () => me.Helper.Translation.Get("GMCM.FalloutChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Volatility_Chance, value => me.ModConfig.Volatility_Chance = value, () => me.Helper.Translation.Get("GMCM.VolatilityChance.Name"), () => me.Helper.Translation.Get("GMCM.VolatilityChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.CrystalCavern_Chance, value => me.ModConfig.CrystalCavern_Chance = value, () => me.Helper.Translation.Get("GMCM.CrystalCavernChance.Name"), () => me.Helper.Translation.Get("GMCM.CrystalCavernChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Upheaval_Chance, value => me.ModConfig.Upheaval_Chance = value, () => me.Helper.Translation.Get("GMCM.UpheavalChance.Name"), () => me.Helper.Translation.Get("GMCM.UpheavalChance.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.SpawningSeason_Chance, value => me.ModConfig.SpawningSeason_Chance = value, () => me.Helper.Translation.Get("GMCM.SpawningSeasonChance.Name"), () => me.Helper.Translation.Get("GMCM.SpawningSeasonChance.Desc"));
                //multipliers
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Aquaculturalist_Multiplier, value => me.ModConfig.Aquaculturalist_Multiplier = value, () => me.Helper.Translation.Get("GMCM.AquaculturalistMultiplier.Name"), () => me.Helper.Translation.Get("GMCM.AquaculturalistMultiplier.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Admiration_Multiplier, value => me.ModConfig.Admiration_Multiplier = value, () => me.Helper.Translation.Get("GMCM.AdmirationMultiplier.Name"), () => me.Helper.Translation.Get("GMCM.AdmirationMultiplier.Desc"));
                //whole numbers
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.Meditation_Health, value => me.ModConfig.Meditation_Health = value, () => me.Helper.Translation.Get("GMCM.MeditationHealth.Name"), () => me.Helper.Translation.Get("GMCM.MeditationHealth.Desc"));
                me.GenericModConfigMenuAPI.AddNumberOption(me.Manifest, () => me.ModConfig.DownInTheDepths_Stones, value => me.ModConfig.DownInTheDepths_Stones = value, () => me.Helper.Translation.Get("GMCM.DownInTheDepthsStones.Name"), () => me.Helper.Translation.Get("GMCM.DownInTheDepthsStones.Desc"));
            }

            else
                me.ModMonitor.Log("Generic Mod Config Menu is either not installed or there was a problem while requesting the API. The config menu wont be created.", LogLevel.Info);
            if (me.SpaceCoreAPI is null)
                me.ModMonitor.Log("SpaceCore is either not installed or there was a problem while requesting the API. If its the latter, custom skill mod integrations will not work.", LogLevel.Info);
            else
            {
                me.SpaceCoreAPI.RegisterSerializerType(typeof(ParrotPerch));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(TrinketRing));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(SlingshotEnchantment));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(ThriftyEnchantment));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(BatKillerEnchantment));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(AutoFireEnchantment));
                me.SpaceCoreAPI.RegisterSerializerType(typeof(RapidEnchantment));
            }

            if (me.WearMoreRingsAPI is null)
            {
                me.ModMonitor.Log("Wear More Rings is either not installed or there was a problem while requesting the API. If its the latter, custom ring slots will not be recognized by this mod.", LogLevel.Info);
            }

            if (me.ItemExtensionsAPI is null)
            {
                me.ModMonitor.Log("Item Extensions is either not installed or there was a problem while requesting the API. If its the latter; custom gem, ore and stone nodes will not be recognized by this mod.", LogLevel.Info);
            }
        }

        public void SaveConfig()
        {
            ModEntry me = GetMe();
            if (me.ModConfig.MasteryCaveChanges != 10 && me.ModConfig.MasteryCaveChanges != 15 && me.ModConfig.MasteryCaveChanges != 20)
            {
                me.ModConfig.MasteryCaveChanges = 20;
                me.ModMonitor.Log("Mastery Cave Changes was changed to an invalid value. Modified it to 20.", LogLevel.Warn);
            }
            me.Helper.WriteConfig(ModConfig);
        }

        public static ModEntry GetMe()
        {
            return CoreModEntry.Value;
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
            if (CoreModEntry.Value.BetterGameMenuAPI != null && menu != null)
                return CoreModEntry.Value.BetterGameMenuAPI.GetCurrentPage(menu);
            return null;
        }

        public static bool IsGameMenu(IClickableMenu menu)
        {
            if (menu is GameMenu)
                return true;
            if (CoreModEntry.Value.BetterGameMenuAPI != null)
                return CoreModEntry.Value.BetterGameMenuAPI.AsMenu(menu) != null;
            return false;
        }

        public static IEnumerable<string> GetTalents()
        {
            if (!Context.IsWorldReady || CoreModEntry.Value.ModConfig.ProfessionsOnly)
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
        private void OnInventoryChanged(object sender, ObjectListChangedEventArgs e)
        {
            foreach (var item in e.Added)
            {
                if (item.Value is StardewValley.Object obj && obj.isLamp.Value)
                {
                    obj.lightSource.color.Value = new Color(255, 215, 175);
                    Game1.player.currentLocation.sharedLights[obj.lightSource.Id].color.Value = new Color(0, 40, 80);
                }
            }
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Utility.ForEachItem(item =>
            {
                if (item is StardewValley.Object obj && obj.ItemId == Constants.Id_GlowingCrystal && obj.modData.ContainsKey(Constants.Key_GlowingCrystalColor))
                {
                    string[] colorcodes = obj.modData[Constants.Key_GlowingCrystalColor].Split(',');
                    obj.lightSource.color.Value = new Color(byte.Parse(colorcodes[0]), byte.Parse(colorcodes[1]), byte.Parse(colorcodes[2]), byte.Parse(colorcodes[3]));
                }
                return true;
            });
            BuildingHandler.OnSaveLoaded();
        }
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady)
            {
                if (MachineryEventHandler.BirdsOnFeeders.TryGetValue(Game1.player.currentLocation.NameOrUniqueName, out var val))
                {
                    foreach (var bird in val)
                    {
                        //Nyehehehe
                        bird.update(Game1.currentGameTime, EmptyCritterRoom);
                    }
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                var data = e.NewLocation?.GetData()?.CustomFields ?? new();
                MineShaft shaft = e.NewLocation as MineShaft;
                MachineryEventHandler.OnPlayerWarp();

                if (e.NewLocation is not null && shaft is not null)
                {
                    if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_MineForage, useThisInstead: e.Player) && Game1.random.NextBool(0.15) && shaft.getMineArea(shaft.mineLevel) is 80 && !shaft.rainbowLights.Value)
                    {
                        shaft.rainbowLights.Value = true;
                        if (Context.IsMainPlayer && Context.HasRemotePlayers)
                        {
                            CoreModEntry.Value.Helper.Multiplayer.SendMessage(true, Manifest.UniqueID + "/MushroomLevel", new string[] { Manifest.UniqueID });
                        }
                    }
                    if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_Fallout) && shaft.getMineArea() is 80 or 121)
                    {
                        List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                     where tileobjpair.Value is not null && TalentUtility.IsBlandStone(tileobjpair.Value)
                                                     select tileobjpair.Key).ToList();
                        bool success = false;

                        Dictionary<Vector2, string> CoordinatesForMP = new();
                        for (int i = 0; i < validcoords.Count; i++)
                        {
                            if (Game1.random.NextBool(CoreModEntry.Value.ModConfig.Fallout_Chance * shaft.mineLevel))
                            {
                                e.NewLocation.Objects[validcoords[i]] = ItemRegistry.Create<StardewValley.Object>("95");
                                e.NewLocation.Objects[validcoords[i]].MinutesUntilReady = 25;
                                CoordinatesForMP.Add(validcoords[i], e.NewLocation.Objects[validcoords[i]].ItemId);
                                success = true;
                            }
                        }
                        if (success && Context.IsMainPlayer && Context.HasRemotePlayers)
                        {
                            CoreModEntry.Value.Helper.Multiplayer.SendMessage(CoordinatesForMP, Manifest.UniqueID + "/SwitchMineStones", new string[] { Manifest.UniqueID });
                        }
                    }
                    if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_DownInTheDepths))
                    {
                        if (shaft.modData.ContainsKey(Constants.Key_DownInTheDepths))
                            shaft.modData[Constants.Key_DownInTheDepths] = "0";
                    }
                    if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_RoomAndPillar) && shaft.isQuarryArea)
                    {
                        bool success = false;
                        List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                     where tileobjpair.Value is not null && (TalentUtility.IsBlandStone(tileobjpair.Value) || (ItemExtensionsAPI?.IsStone(tileobjpair.Value.QualifiedItemId) is true && ItemExtensionsAPI?.IsResource(tileobjpair.Value.QualifiedItemId, out int? _, out string itemDropped) is true && itemDropped.Contains("390")))
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
                            CoreModEntry.Value.Helper.Multiplayer.SendMessage(CoordinatesForMP, Manifest.UniqueID + "/SwitchMineStones", new string[] { Manifest.UniqueID });
                        }
                    }
                }
                if (data?.ContainsKey(Constants.Key_CrystalCavern) is true || data?.ContainsKey(Constants.Key_Upheaval) is true || e.NewLocation is MineShaft or VolcanoDungeon)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && TalentUtility.IsBlandStone(tileobjpair.Value)
                                                 select tileobjpair.Key).ToList();

                    if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_CrystalCavern) && (data?.ContainsKey(Constants.Key_CrystalCavern) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(ModConfig.CrystalCavern_Chance))
                    {
                        TalentUtility.GemAndGeodeNodes(true, validcoords, Game1.player.currentLocation);
                    }
                    else if (TalentUtility.AllPlayersHaveTalent(Constants.Talent_Upheaval) && (data?.ContainsKey(Constants.Key_Upheaval) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(ModConfig.Upheaval_Chance))
                    {
                        TalentUtility.GemAndGeodeNodes(false, validcoords, Game1.player.currentLocation);
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Fortified, who: e.Player))
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
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (ForagingPatcher.IsAnyCharAround(Game1.player.currentLocation, Game1.player.Tile))
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
                        else if (Game1.player.ActiveObject is Trinket trinket && trinket?.QualifiedItemId == "(TR)FairyBox" && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_HiddenBenefits))
                        {
                            if (value is HoeDirt dirt && dirt.crop is not null && !dirt.crop.fullyGrown.Value && dirt.crop.currentPhase.Value != dirt.crop.phaseDays.Count - 1)
                            {
                                bool shouldGrow = false;
                                if (!trinket.modData.TryAdd(Constants.Key_HiddenBenefit_FairyBox, "1"))
                                {
                                    if (trinket.modData[Constants.Key_HiddenBenefit_FairyBox] is not "3")
                                    {
                                        shouldGrow = true;
                                        trinket.modData[Constants.Key_HiddenBenefit_FairyBox] = (int.Parse(trinket.modData[Constants.Key_HiddenBenefit_FairyBox]) + 1).ToString();
                                    }
                                    else
                                        Game1.showGlobalMessage(CoreModEntry.Value.Helper.Translation.Get("Message.FairyBreak"));
                                }
                                else
                                    shouldGrow = true;
                                if (shouldGrow)
                                {
                                    if (!dirt.crop.modData.TryAdd(Constants.Key_HiddenBenefit_Crop, "true"))
                                    {
                                        if (dirt.crop.modData[Constants.Key_HiddenBenefit_Crop] == "true")
                                            Game1.showGlobalMessage(CoreModEntry.Value.Helper.Translation.Get("Message.AlreadyFertilized"));
                                        else
                                        {
                                            Game1.playSound("wand");
                                            dirt.crop.modData[Constants.Key_HiddenBenefit_Crop] = "true";
                                            Game1.showGlobalMessage(CoreModEntry.Value.Helper.Translation.Get("Message.Fertilized"));
                                        }
                                    }
                                    else
                                    {
                                        Game1.playSound("wand");
                                        Game1.showGlobalMessage(CoreModEntry.Value.Helper.Translation.Get("Message.Fertilized"));
                                        dirt.crop.modData[Constants.Key_HiddenBenefit_Crop] = "true";
                                    }
                                }
                            }
                        }
                    }
                    else if (Game1.player.CurrentTool is FishingRod rod && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_TakeItSlow))
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
                    else if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.Tile, out value2) && value2.ItemId == Constants.Id_BoxTrough && value2.lastInputItem.Value is null && Game1.player.ActiveObject?.QualifiedItemId == (ExtraAnimalConfigAPI?.GetFeedOverride(Game1.currentLocation.ParentBuilding?.buildingType.Value) ?? "(O)178"))
                    {
                        value2.lastInputItem.Value = Game1.player.ActiveObject.getOne();
                        Game1.player.ActiveObject.ConsumeStack(1);
                        value2.showNextIndex.Value = true;
                    }
                    else if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.Tile, out value2) && value2 is not ParrotPerch && value2.QualifiedItemId == "(BC)Kedi.VPP.HiddenBenefits.ParrotPerch" && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_HiddenBenefits))
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
                    if (Game1.player.ActiveObject?.ItemId is Constants.Id_MossyFertilizer or Constants.Id_WildTotem or Constants.Id_SunTotem or Constants.Id_SnowTotem or Constants.Id_NodeLifter)
                    {
                        CraftableHandler.OnInteract(Game1.player, Game1.player.ActiveObject);
                    }
                    else if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object value3))
                    {
                        if (value3.heldObject.Value?.heldObject.Value is Chest chest && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Fertigation))
                        {
                            chest.GetMutex().RequestLock(chest.ShowMenu);
                        }
                        if (value3.ItemId == Constants.Id_GlowingCrystal && Game1.player.ActiveItem.Category is StardewValley.Object.mineralsCategory or StardewValley.Object.GemCategory)
                        {
                            value3.lightSource.color.Value = TailoringMenu.GetDyeColor(Game1.player.ActiveItem) ?? value3.lightSource.color.Value;
                            Color color = new Color(value3.lightSource.color.Value.R + 100, value3.lightSource.color.Value.G + 100, value3.lightSource.color.Value.B + 100, 255);
                            Game1.player.currentLocation.sharedLights[value3.lightSource.Id].color.Value = new(255 - color.R, 255 - color.G, 255 - color.B);
                            value3.modData[Constants.Key_GlowingCrystalColor] = $"{color.R},{color.G},{color.B},{color.A}";
                        }
                        else if (value3.isLamp.Value)
                        {
                            value3.lightSource.color.Value = new Color(255, 255, 255) * 0.25f;
                            Game1.player.currentLocation.sharedLights[value3.lightSource.Id].color.Value = new Color(0, 0, 0);
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
                        if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_FarmForage) && value.IsTapper())
                            value.shakeTimer = 0;
                        else if (value.heldObject.Value is null && value.IsSprinkler() && Game1.player.ActiveObject?.QualifiedItemId is "(O)Kedi.VPP.Fertigator" && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Fertigation))
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
                            if (TalentUtility.AnyPlayerHasTalent(Constants.Talent_MiniFridgeBigSpace))
                            {
                                chest4.SpecialChestType = Chest.SpecialChestTypes.BigChest;
                            }
                        }
                    }
                    else if (Game1.player.ActiveObject?.QualifiedItemId is "(O)96" or "(O)97" or "(O)98" or "(O)99" && Game1.player.canUnderstandDwarves && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_ElderScrolls))
                    {
                        Game1.player.gainExperience(3, 250);
                        Game1.player.currentLocation.playSound("shwip");
                        var msg = HUDMessage.ForItemGained(Game1.player.ActiveObject, 1, "ElderScrolls");
                        msg.message = CoreModEntry.Value.Helper.Translation.Get("Message.ReadDwarfScroll");
                        Game1.addHUDMessage(msg);
                        if (Game1.player.ActiveObject.ConsumeStack(1) is null)
                        {
                            Game1.player.ActiveObject = null;
                        }
                    }
                    else if (Game1.player.CurrentTool is not null and Slingshot slingshot && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_TripleShot))
                    {
                        foreach (var item in Game1.player.enchantments)
                            if (item is AutoFireEnchantment) //Balance
                                return;
                        if (TalentCore.TalentCoreEntry.Value.TripleShotCooldown <= 0)
                        {
                            slingshot.beginUsing(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
                            slingshot.lastUser = Game1.player;
                            TalentCore.TalentCoreEntry.Value.TripleShotCooldown = 5000;
                            Game1.player.usingSlingshot = true;
                            TalentCore.TalentCoreEntry.Value.IsActionButtonUsed = true;
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
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.CoreDisplayHandler.Value.LittlePlus.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive = !DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive;
                            page.skillBars = DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive ? DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars.ToList() : DisplayHandler.CoreDisplayHandler.Value.VanillaSkillBars.ToList();
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
                    pagee.allClickableComponents.Add(DisplayHandler.CoreDisplayHandler.Value.LittlePlus);
                    DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars = pagee.skillBars.ToArray();
                    NewSkillsPage skillsPage2 = new(Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen, Game1.activeClickableMenu.width + ((LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.it) ? 64 : 0), Game1.activeClickableMenu.height);
                    DisplayHandler.CoreDisplayHandler.Value.VanillaSkillBars = skillsPage2.skillBars.ToArray();
                    if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
                    {
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.CoreDisplayHandler.Value.LittlePlus.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            List<(int, string)> IndexAndProfessions = new();
                            List<string> AlreadyPickedProfessions = new();

                            for (int i = 0; i < DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars.Length; i++)
                            {
                                foreach (var item in Professions)
                                {
                                    if (DisplayHandler.CoreDisplayHandler.Value.IsInCorrectLine(DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[i].bounds, pagee.skillAreas, item.Value.Skill.ToString()))
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
                                            DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[i].name = "-1";
                                        }
                                    }
                                }
                                foreach (var item in TalentCore.SkillsByName.Values)
                                {
                                    if (DisplayHandler.CoreDisplayHandler.Value.IsInCorrectLine(DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[i].bounds, pagee.skillAreas, item.Id))
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
                                                    DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[i].name = "-1";
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
                                    DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[IndexAndProfessions[index].Item1].name = splitted[0];
                                }
                                else
                                {
                                    DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[IndexAndProfessions[index].Item1].name = IndexAndProfessions[index].Item2;
                                    description = LevelUpMenu.getProfessionDescription(int.Parse(IndexAndProfessions[index].Item2));
                                    description.RemoveAt(0);
                                    toBeMeasured = LevelUpMenu.getProfessionTitleFromNumber(int.Parse(IndexAndProfessions[index].Item2));
                                }

                                DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars[IndexAndProfessions[index].Item1].hoverText = Game1.parseText(description.Join(delimiter: "\n"), Game1.smallFont, (int)Game1.dialogueFont.MeasureString(toBeMeasured).X + 100);
                            }
                            DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive = !DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive;
                            pagee.skillBars = DisplayHandler.CoreDisplayHandler.Value.IsOverlayActive ? DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars.ToList() : DisplayHandler.CoreDisplayHandler.Value.VanillaSkillBars.ToList();
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
                    if (DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars is not null)
                    {
                        foreach (var item in DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars)
                        {
                            if (item.downNeighborID == -1)
                            {
                                item.downNeighborID = 46780;
                                break;
                            }
                        }
                    }
                    if (DisplayHandler.CoreDisplayHandler.Value.VanillaSkillBars is not null)
                    {
                        foreach (var item in DisplayHandler.CoreDisplayHandler.Value.VanillaSkillBars)
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
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (TalentCore.TalentCoreEntry.Value.IsActionButtonUsed && e.Button.IsActionButton() && Game1.player.CurrentTool is not null and Slingshot slingshot)
            {
                CoreUtility.PerformFire(Game1.player.currentLocation, Game1.player, slingshot);
                slingshot.canPlaySound = false;
                slingshot.PerformFire(Game1.player.currentLocation, Game1.player);
                TalentCore.TalentCoreEntry.Value.IsActionButtonUsed = false;
                Game1.player.completelyStopAnimatingOrDoingAction();
            }
            if (IsGameMenu(Game1.activeClickableMenu))
            {
                if ((e.Button.IsUseToolButton() || e.Button.IsActionButton()) && DisplayHandler.CoreDisplayHandler.Value.OpenTalentMenuCooldown && !ModConfig.ProfessionsOnly)
                {
                    var menuPage = GetGameMenuPage(Game1.activeClickableMenu);
                    if (menuPage is SkillsPage page)
                    {
                        for (int i = 0; i < page.skillAreas.Count; i++)
                        {
                            if (page.skillAreas[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i, this);
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
                                Game1.activeClickableMenu = new TalentSelectionMenu(i, this);
                            }
                        }
                    }
                }
            }
        }
        private void TerrainFeatureTapper(TerrainFeature TreeOrCrop, ButtonPressedEventArgs e)
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
                TreeOrCrop.modData[Constants.Key_TFHasTapper] = "false";
            }
            else if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_FarmForage))
            {
                if (Game1.player.ActiveObject?.IsHeldOverHead() == true && Game1.player.ActiveObject?.IsTapper() == true && e.Button.IsUseToolButton())
                {
                    var obsj = Game1.player.ActiveObject;

                    if (TreeOrCrop.modData.TryAdd(Constants.Key_TFHasTapper, "true")
                    && TreeOrCrop.modData.TryAdd(Constants.Key_TFTapperID, Game1.player.ActiveObject.QualifiedItemId))
                    {
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(ManagerUtility.GetFeatureTile(TreeOrCrop), obsj);
                    }
                    else if (TreeOrCrop.modData.TryGetValue(Constants.Key_TFHasTapper, out var value2) && TreeOrCrop.modData.ContainsKey(Constants.Key_TFTapperID) && value2 is "false")
                    {
                        TreeOrCrop.modData[Constants.Key_TFHasTapper] = "true";
                        TreeOrCrop.modData[Constants.Key_TFTapperID] = Game1.player.ActiveObject.QualifiedItemId;
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(ManagerUtility.GetFeatureTile(TreeOrCrop), obsj);
                    }
                    else if (value2 is "true" && TreeOrCrop.modData.ContainsKey(Constants.Key_TFTapperID) && !TreeOrCrop.Location.Objects.TryGetValue(TreeOrCrop.Tile, out StardewValley.Object _))
                    {
                        TreeOrCrop.modData[Constants.Key_TFHasTapper] = "false";
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
            MachineryEventHandler.BirdsOnFeeders.Clear();
            CoreUtility.confirmTrinkets = false;
            bool Orchardist = CoreUtility.AnyPlayerHasProfession(Constants.Profession_Orchardist);
            bool Accumulation = TalentUtility.AnyPlayerHasTalent(Constants.Talent_Accumulation);
            bool Abundance = TalentUtility.AnyPlayerHasTalent(Constants.Talent_Abundance);

            Utility.ForEachItem(item =>
            {
                if (item is not null and StardewValley.Object obj)
                {
                    if (obj is CrabPot crabPot)
                    {
                        if (crabPot.heldObject.Value is not null)
                        {
                            if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_BaitAndSwitch, crabPot.owner.Value) && crabPot.heldObject.Value.HasContextTag("fish_crab_pot") is true && Game1.random.NextBool(0.05))
                            {
                                crabPot.heldObject.Value.Quality++;
                                crabPot.heldObject.Value.FixQuality();
                            }
                        }
                        else if (crabPot.bait.Value is not null && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_FishsWishes, crabPot.owner.Value))
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
                            if (Game1.random.NextBool(0.05))
                            {
                                obj.heldObject.Value.Quality++;
                                obj.heldObject.Value.FixQuality();
                            }
                        }
                    }
                    if (obj.QualifiedItemId == Constants.Id_BeeHouse && obj.heldObject.Value is not null)
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
                if (building.GetIndoors() is SlimeHutch slimeHutch && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_CombatFarm, farmerID: building.owner.Value))
                {
                    var str = (!slimeHutch.waterSpots.Contains(true)).ToString().ToLower();
                    if (!slimeHutch.modData.TryAdd(Constants.Key_IsSlimeHutchWatered, str))
                    {
                        slimeHutch.modData[Constants.Key_IsSlimeHutchWatered] = str;
                    }
                }
                else if (building is FishPond fishPond && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Exsquidsite, farmerID: fishPond.owner.Value))
                {
                    if (fishPond.output.Value is not null && fishPond.output.Value.QualifiedItemId is "(O)812" or "(O)814" && Game1.random.NextBool(0.24))
                    {
                        fishPond.output.Value.Quality++;
                        fishPond.output.Value.FixQuality();
                    }
                }
                /*else if (building.GetIndoors() is AnimalHouse animalHouse)
                {
                    List<FarmAnimal> hungryAnimals = new();
                    List<StardewValley.Object> boxTroughs = new();
                    foreach (var animal in animalHouse.Animals.Values)
                    {
                        if (animal.fullness.Value < 200)
                        {
                            hungryAnimals.Add(animal);
                        }
                    }
                    foreach (var obj in animalHouse.Objects.Values)
                    {
                        if (obj.ItemId == Constants.Id_BoxTrough)
                        {
                            if (obj.lastInputItem.Value is not null)
                            {
                                boxTroughs.Add(obj);
                            }
                        }
                    }
                    if (hungryAnimals.Count > 0 && boxTroughs.Count > 0)
                    {
                        for (int i = 0; i < boxTroughs.Count; i++)
                        {
                            boxTroughs[i].lastInputItem.Value = null;
                            boxTroughs[i].showNextIndex.Value = false;
                            hungryAnimals[i].fullness.Value = 255;
                            hungryAnimals.Remove(hungryAnimals[i]);
                            if (hungryAnimals.Count == 0)
                                break;
                        }
                    }
                }*/
                return true;
            }
            );
            Utility.ForEachLocation(loc =>
            {
                if (!loc.modData.TryAdd(Constants.Key_WasRainingHere, loc.IsRainingHere().ToString().ToLower()))
                {
                    loc.modData[Constants.Key_WasRainingHere] = loc.IsRainingHere().ToString().ToLower();
                }
                foreach (var obj in loc.Objects.Values)
                {
                    if (obj.isForage() && CraftablePatcher.ForageCropLocations.TryGetValue(obj.Location.NameOrUniqueName, out List<Vector2> tiles1))
                    {
                        if (tiles1.Contains(obj.TileLocation))
                        {
                            obj.modData.TryAdd(Constants.Key_VPPDeluxeForage, "");
                        }
                    }
                }
                return true;
            });
            BuildingHandler.OnDayEnding();
            //It works. don't touch.
            IEnumerable<TrinketRing> lalalal = TalentUtility.GetAllTrinketRings(Game1.player);
            if (lalalal.Any())
            {
                foreach (var trinketRing in lalalal)
                {
                    trinketRing.GetRingTrinket().onUnequip(Game1.player);
                    Game1.player.trinketItems.Remove(trinketRing.GetRingTrinket());
                }
            }
            if (!Game1.player.modData.TryAdd(Constants.Key_TalentPoints, TalentCore.TalentCoreEntry.Value.TalentPointCount.ToString()))
                Game1.player.modData[Constants.Key_TalentPoints] = TalentCore.TalentCoreEntry.Value.TalentPointCount.ToString();
            if (!Game1.player.modData.TryAdd(Constants.Key_DisabledTalents, string.Join('|', TalentCore.DisabledTalents.ToArray())))
            {
                Game1.player.modData[Constants.Key_DisabledTalents] = string.Join('|', TalentCore.DisabledTalents.ToArray());
            }
        }
        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLevel > e.OldLevel)
            {
                if (e.NewLevel is 15 or 20 && (int)e.Skill < 5)
                {
                    DisplayHandler.CoreDisplayHandler.Value.ShouldHandleSkillPage = true;
                }
                if (IsUninstalling)
                {
                    TalentCore.TalentCoreEntry.Value.TalentPointCount = 0;
                }
                else if (e.OldLevel + 1 == e.NewLevel && !IsRecalculatingPoints)
                {
                    TalentCore.TalentCoreEntry.Value.AddTalentPoint();
                }
            }
        }
    }
}
