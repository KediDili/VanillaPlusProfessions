using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.GiantCrops;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Compatibility;
using VanillaPlusProfessions.Managers;

namespace VanillaPlusProfessions
{
    public class ModEntry : Mod
    {
        internal static new IModHelper Helper;
        internal static IMonitor ModMonitor;
        internal static IManifest Manifest;

        internal static PerScreen<IContentPatcher> ContentPatcherAPI = new();
        internal static PerScreen<IGenericModConfigMenu> GenericModConfigMenuAPI = new();
        internal static PerScreen<ISpaceCore> SpaceCoreAPI = new();

        internal static readonly VanillaPlusProfessionsAPI VanillaPlusProfessionsAPI = new();

        internal static Harmony Harmony { get; } = new("KediDili.VanillaPlusProfessions");

        internal static IProfessionManager[] Managers = new IProfessionManager[6];
        internal static int[] levelExperiences;

        internal static Dictionary<string, Profession> Professions = new();
        internal static PerScreen<Config> ModConfig = new();

        internal static PerScreen<List<string>> ProfessionsChosen = new(createNewState: () => new());

        internal const string Key_HasFoundForage = "Kedi.VPP.HasFoundForageGame";
        internal const string Key_DaysLeftForForageGuess = "Kedi.VPP.GuessForageGameDaysLeft";
        internal const string Key_ForageGuessItemID = "Kedi.VPP.GuessForageID";

        internal const string Key_TFTapperDaysLeft = "Kedi.VPP.ProduceDaysLeft";
        internal const string Key_TFTapperID = "Kedi.VPP.TapperID";
        internal const string Key_TFHasTapper = "Kedi.VPP.IsTapped";

        internal const string Key_IsSlimeHutchWatered = "Kedi.VPP.IsWatered";
        internal const string Key_SlimeWateredDaysSince = "Kedi.VPP.SlimeWateredDaysSince";
        internal const string Key_FishRewardOrQuestDayLeft = "Kedi.VPP.FishQuestOrRewardDuration";

        public override void Entry(IModHelper helper)
        {
            Helper = helper;
            ModMonitor = Monitor;
            Manifest = ModManifest;

            DisplayHandler.Initialize();
            ContentEditor.Initialize();
            TalentCore.Initialize();
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.DayStarted += DayStartHandler.OnDayStarted;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.Player.LevelChanged += OnLevelChanged;
            Helper.Events.Player.Warped += OnWarped;

            Helper.ConsoleCommands.Add("vpp.removeAll", "Removes all professions and metadata added by Some More Professions.", CoreUtility.remove);
            Helper.ConsoleCommands.Add("vpp.recalculatePoints", "Recalculates all talent points, useful for existing saves that are being loaded for the first time with this mod.", CoreUtility.recalculate);

            levelExperiences = Helper.Data.ReadJsonFile<int[]>("assets/levelExperiences.json");
            Professions = Helper.Data.ReadJsonFile<Dictionary<string, Profession>>("assets/professions.json");
            ModConfig.Value = Helper.ReadConfig<Config>();

            Harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getProfessionForSkill)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.getProfessionForSkill_Postfix)))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkForLevelGain)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.checkForLevelGain_Postfix)))
            );
            if (Game1.versionLabel.Contains("beta"))
            {
                Harmony.Patch(
                    original: AccessTools.Constructor(typeof(LevelUpMenu), new Type[] { typeof(int), typeof(int) }),
                //    postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.LevelUpMenu_Postfix))),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.Transpiler)))
                );
                Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.Transpiler)))
                );
            }
            else
            {
                /*Harmony.Patch(
                    original: AccessTools.Constructor(typeof(LevelUpMenu), new Type[] { typeof(int), typeof(int) }),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.LevelUpMenu_Postfix)))
                );*/
            }

            Harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), "getProfessionName"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.getProfessionName_Postfix)))
            );            
            Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.answerDialogueAction_Postfix)))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.canRespec)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.canRespec_Postfix)))
            );

            Managers = new IProfessionManager[] { new FarmingManager(), new MiningManager(), new ForagingManager(), new FishingManager(), new CombatManager(), new ComboManager() };

            var dict = Professions.Keys.ToList();
            foreach (var manager in Managers)
            {
                foreach (var item in Professions)
                    if (item.Value.ID % 8 == manager.SkillValue || manager.SkillValue == 9)
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
            ContentPatcherAPI.Value = Helper.ModRegistry.GetApi<IContentPatcher>("Pathoschild.ContentPatcher");
            GenericModConfigMenuAPI.Value = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
            SpaceCoreAPI.Value = Helper.ModRegistry.GetApi<ISpaceCore>("spacechase0.SpaceCore");

            if (ContentPatcherAPI is not null)
            {
                ContentPatcherAPI.Value.RegisterToken(Manifest, "HasProfessions", GetProfessions);
                ContentPatcherAPI.Value.RegisterToken(Manifest, "HasTalents", GetTalents);
            }
            else
            {
                ModMonitor.Log("Content Patcher is either not installed or there was a problem while requesting the API. Skipping token additions.", LogLevel.Info);
            }
            if (GenericModConfigMenuAPI.Value is not null)
            {
                GenericModConfigMenuAPI.Value.Register(Manifest, () => ModConfig.Value = new Config(), () => Helper.WriteConfig(ModConfig));
                GenericModConfigMenuAPI.Value.AddBoolOption(Manifest, () => ModConfig.Value.ColorBlindnessChanges, value => ModConfig.Value.ColorBlindnessChanges = value, () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Name"), () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Desc"));
                GenericModConfigMenuAPI.Value.AddBoolOption(Manifest, () => ModConfig.Value.DeveloperOrTestingMode, value => ModConfig.Value.DeveloperOrTestingMode = value, () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Name"), () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Desc"));
            }
            else
            {
                ModMonitor.Log("Generic Mod Config Menu is either not installed or there was a problem while requesting the API. The config menu wont be created.", LogLevel.Info);
            }
            if (SpaceCoreAPI is null)
            {
                ModMonitor.Log("SpaceCore is either not installed or there was a problem while requesting the API. If its the latter, custom skill mod integrations will not work.", LogLevel.Info);
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
                if (CoreUtility.CurrentPlayerHasProfession(item.Value.ID))
                {
                    yield return item.Key;
                }
            }
        }

        public static IEnumerable<string> GetTalents()
        {
            if (!Context.IsWorldReady)
            {
                yield return null;
                yield break;
            }
            for (int i = 0; i < TalentCore.GainedTalents.Value.Count; i++)
            {
                yield return TalentCore.GainedTalents.Value[i];
            }
        }
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft shaft && CoreUtility.CurrentPlayerHasProfession(73, e.Player) && Game1.random.NextBool(0.15) && shaft.getMineArea(shaft.mineLevel) is 80)
            {
                shaft.rainbowLights.Value = true;
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (Game1.activeClickableMenu is null)
            {
                if (e.Button.IsUseToolButton())
                {
                    if (Game1.player.ActiveObject?.QualifiedItemId == "(O)Kedi.SMP.GemDust")
                    {
                        if (Game1.player.currentLocation.terrainFeatures.TryGetValue(e.Cursor.GrabTile, out TerrainFeature value))
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
                    }
                    else if (Game1.player.currentLocation.terrainFeatures.TryGetValue(e.Cursor.GrabTile, out TerrainFeature value) && value is FruitTree or GiantCrop)
                    {
                        TerrainFeatureTapper(ref value, e);
                    }
                }
                else if (e.Button.IsActionButton())
                {
                    if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object value) && value.IsTapper())
                    {
                        if (CoreUtility.CurrentPlayerHasProfession(70))
                            value.shakeTimer = 0;
                    }
                }
            }            
        }
        private static void TerrainFeatureTapper(ref TerrainFeature TreeOrCrop, ButtonPressedEventArgs e)
        {
            if (Game1.player.CurrentTool is Pickaxe or Axe && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object obj) && obj.IsTapper())
            {
                TreeOrCrop.modData[Key_TFHasTapper] = "false";
            }
            else if (CoreUtility.CurrentPlayerHasProfession(70))
            {
                if (Game1.player.ActiveObject?.IsHeldOverHead() == true && Game1.player.ActiveObject?.IsTapper() == true && e.Button.IsUseToolButton())
                {
                    FruitTreeData fruitTreeData = (TreeOrCrop as FruitTree).GetData();
                    GiantCropData giantCropData = (TreeOrCrop as GiantCrop).GetData();
                    var obsj = ItemRegistry.Create<StardewValley.Object>(TreeOrCrop.modData[Key_TFTapperID]);

                    obsj.modData.Add(Key_TFTapperDaysLeft, "0");

                    var dsdsd = ItemRegistry.Create<StardewValley.Object>(fruitTreeData?.Fruit?[0]?.ItemId ?? giantCropData.FromItemId);
                    dsdsd.modData.Add("Kedi.SMP.CurrentPreserveType", "Kedi.SMP.FruitSyrup");
                    obsj.lastInputItem.Add(dsdsd);

                    if (TreeOrCrop.modData.TryAdd(Key_TFHasTapper, "true")
                    && TreeOrCrop.modData.TryAdd(Key_TFTapperID, Game1.player.ActiveObject.QualifiedItemId))
                    {
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(TreeOrCrop.Tile, obsj);
                    }
                    else if (TreeOrCrop.modData.TryGetValue(Key_TFHasTapper, out var value2) && TreeOrCrop.modData.ContainsKey(Key_TFTapperID) && value2 is "false")
                    {
                        TreeOrCrop.modData[Key_TFHasTapper] = "true";
                        TreeOrCrop.modData[Key_TFTapperID] = Game1.player.ActiveObject.QualifiedItemId;
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack <= 0)
                            Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        TreeOrCrop.Location.Objects.Add(TreeOrCrop.Tile, obsj);
                    }
                }
                else if (TreeOrCrop is FruitTree tree && tree.fruit.Count > 0)
                {
                    tree.shake(TreeOrCrop.Tile, true);
                }
            }
        }
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (DisplayHandler.WasSkillMenuRaised.Value)
            {
                Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
                DisplayHandler.WasSkillMenuRaised.Value = false;
            }
            if (CoreUtility.AnyPlayerHasProfession(78))
            {
                Utility.ForEachBuilding<Building>(building =>
                {
                    if (building.GetIndoors() is SlimeHutch slimeHutch)
                    {
                        var str = (!slimeHutch.waterSpots.Contains(true)).ToString().ToLower();
                        if (!slimeHutch.modData.TryAdd(Key_IsSlimeHutchWatered, str))
                        {
                            slimeHutch.modData[Key_IsSlimeHutchWatered] = str;
                        }
                    }
                    return true;
                }
                );
            }
        }
        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.NewLevel > 10 && (int)e.Skill < 5)
                {
                    DisplayHandler.ShouldHandleSkillPage.Value = true;
                }
                TalentCore.TalentPointCount.Value++;
            }
        }
    }
}
