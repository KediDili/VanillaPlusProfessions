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
using StardewValley.Internal;
using SpaceCore.Interface;
using VanillaPlusProfessions.Talents.UI;

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
        internal static PerScreen<IWearMoreRings> WearMoreRingsAPI = new();

        internal static readonly VanillaPlusProfessionsAPI VanillaPlusProfessionsAPI = new();

        internal static CustomQueries CustomQueries = new();

        internal static Harmony Harmony { get; } = new("KediDili.VanillaPlusProfessions");

        internal static IProfessionManager[] Managers = new IProfessionManager[6];
        internal static int[] levelExperiences;

        internal static Dictionary<string, Profession> Professions = new();
        internal static PerScreen<bool> UpdateGeodeInMenu = new(() => false);
        internal static PerScreen<int> GeodeStackSize = new(() => 0);

        internal static PerScreen<Config> ModConfig = new(createNewState: () => new Config());

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

            Helper.ConsoleCommands.Add("vpp.removeAll", "Removes all professions, talents and metadata added by Vanilla Plus Professions.", CoreUtility.remove);
            Helper.ConsoleCommands.Add("vpp.recalculatePoints", "Recalculates all talent points, useful for existing saves that are being loaded for the first time with this mod.", CoreUtility.recalculate);
            Helper.ConsoleCommands.Add("vpp.details", "Prints out skill related information. Might be useful for troubleshooting.", CoreUtility.details);
            Helper.ConsoleCommands.Add("vpp.reset", "Can be used to reset professions added by VPP. First parameter is the level (15 or 20), second is the level (0 - Farming, 1 - Fishing, 2 - Foraging, 3 - Mining or 4 - Combat)", ManagerUtility.reset);

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
            ContentPatcherAPI.Value = Helper.ModRegistry.GetApi<IContentPatcher>("Pathoschild.ContentPatcher");
            GenericModConfigMenuAPI.Value = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
            SpaceCoreAPI.Value = Helper.ModRegistry.GetApi<ISpaceCore>("spacechase0.SpaceCore");
            WearMoreRingsAPI.Value = Helper.ModRegistry.GetApi<IWearMoreRings>("bcmpinc.WearMoreRings");

            CustomQueries.Initialize();

            if (ContentPatcherAPI.Value is not null)
            {
                ContentPatcherAPI.Value.RegisterToken(Manifest, "HasProfessions", GetProfessions);
                ContentPatcherAPI.Value.RegisterToken(Manifest, "ContentPaths", new ContentPaths());
            }
            else
                ModMonitor.Log("Content Patcher is either not installed or there was a problem while requesting the API. Skipping token additions.", LogLevel.Info);
            if (GenericModConfigMenuAPI.Value is not null)
            {
                GenericModConfigMenuAPI.Value.Register(Manifest, () => ModConfig.Value = new Config(), () => Helper.WriteConfig(ModConfig.Value));
                GenericModConfigMenuAPI.Value.AddBoolOption(Manifest, () => ModConfig.Value.ColorBlindnessChanges, value => ModConfig.Value.ColorBlindnessChanges = value, () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Name"), () => Helper.Translation.Get("GMCM.ColorBlindnessChanges.Desc"));
                GenericModConfigMenuAPI.Value.AddBoolOption(Manifest, () => ModConfig.Value.DeveloperOrTestingMode, value => ModConfig.Value.DeveloperOrTestingMode = value, () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Name"), () => Helper.Translation.Get("GMCM.DeveloperOrTestingMode.Desc"));
                GenericModConfigMenuAPI.Value.AddBoolOption(Manifest, () => ModConfig.Value.MasteryCaveChanges, value => ModConfig.Value.MasteryCaveChanges = value, () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Name"), () => Helper.Translation.Get("GMCM.MasteryCaveChanges.Desc"));
                GenericModConfigMenuAPI.Value.AddTextOption(Manifest, () => ModConfig.Value.TalentHintLevel, value => ModConfig.Value.TalentHintLevel = value, () => Helper.Translation.Get("GMCM.TalentHintLevel.Name"), () => Helper.Translation.Get("GMCM.TalentHintLevel.Desc"), new string[] {"Hidden", "Partial", "Full"}, option => Helper.Translation.Get($"GMCM.TalentHintLevel.Options.{option}"));
            }
            else
                ModMonitor.Log("Generic Mod Config Menu is either not installed or there was a problem while requesting the API. The config menu wont be created.", LogLevel.Info);
            if (SpaceCoreAPI.Value is null)
                ModMonitor.Log("SpaceCore is either not installed or there was a problem while requesting the API. If its the latter, custom skill mod integrations will not work.", LogLevel.Info);
            else
            {
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(ParrotPerch));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(TrinketRing));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(SlingshotEnchantment));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(ThriftyEnchantment));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(BatKillerEnchantment));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(AutoFireEnchantment));
                SpaceCoreAPI.Value.RegisterSerializerType(typeof(RapidEnchantment));
            }

            if (WearMoreRingsAPI.Value is null)
            {
                ModMonitor.Log("Wear More Rings is either not installed or there was a problem while requesting the API. If its the latter, custom ring slots will not be recognized by this mod.", LogLevel.Info);
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
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                var data = e.NewLocation?.GetData()?.CustomFields ?? new();
                if (e.NewLocation is MineShaft shaft)
                {
                    if (CoreUtility.CurrentPlayerHasProfession(73, useThisInstead: e.Player) && Game1.random.NextBool(0.15) && shaft.getMineArea(shaft.mineLevel) is 80 && !shaft.rainbowLights.Value)
                    {
                        shaft.rainbowLights.Value = true;
                    }
                }
                if (e.NewLocation is SlimeHutch hutch && hutch.Objects.Pairs.Count() > 0)
                {
                    List<Vector2> KeysToRemove = new();
                    foreach (var item in Game1.player.currentLocation.Objects.Pairs)
                    {
                        if (item.Value.QualifiedItemId.StartsWith("(O)Kedi.VPP.") && item.Value.QualifiedItemId.EndsWith("Slime") && item.Value.IsSpawnedObject)
                        {
                            KeysToRemove.Add(item.Key);
                        }
                    }
                    for (int i = 0; i < KeysToRemove.Count; i++)
                    {
                        hutch.Objects.Remove(KeysToRemove[i]);
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent("Mining_Fallout") && e.NewLocation is MineShaft shaft45 && shaft45?.getMineArea() is 80 or 121)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && tileobjpair.Value.IsBlandStone()
                                                 select tileobjpair.Key).ToList();
                    for (int i = 0; i < validcoords.Count; i++)
                    {
                        if (Game1.random.NextBool(0.15))
                        {
                            e.NewLocation.Objects[validcoords[i]] = ItemRegistry.Create<StardewValley.Object>("95");
                        }
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("Combat_Fortified", who: e.Player) && e.NewLocation is not null)
                {
                    int monsters = e.NewLocation.characters.Where(item => item.IsMonster).Count();
                    if (monsters > 0)
                    {
                        BuffEffects buffEffects = new();
                        buffEffects.Defense.Value = Math.Min(8, monsters);
                        Buff buff = new("VPP.Fortified.Defense", "Fortified", "Fortified", -2, Game1.buffsIcons, 10, buffEffects, false, Game1.parseText(Helper.Translation.Get("Buff.Fortified.Name")), Game1.parseText(Helper.Translation.Get("Buff.Fortified.Desc")));
                        e.Player.buffs.Apply(buff);
                    }
                    else
                    {
                        e.Player.buffs.Remove("VPP.Fortified.Defense");
                    }
                }
                if (TalentUtility.AllPlayersHaveTalent("Mining_Down_In_The_Depths") && e.NewLocation is MineShaft shaft6)
                    if (shaft6.modData.ContainsKey(TalentCore.Key_DownInTheDepths))
                        shaft6.modData[TalentCore.Key_DownInTheDepths] = "0";
                if (TalentUtility.AllPlayersHaveTalent("Mining_Room_And_Pillar") && e.NewLocation is MineShaft shaft5 && shaft5.isQuarryArea)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && tileobjpair.Value.IsBlandStone()
                                                 select tileobjpair.Key).ToList();
                    for (int i = 0; i < validcoords.Count; i++)
                        if (Game1.random.NextBool(0.15))
                            e.NewLocation.Objects[validcoords[i]] = ItemRegistry.Create<StardewValley.Object>(TalentUtility.GetNodeForRoomAndPillar());
                }
                if ((data?.ContainsKey(TalentCore.Key_CrystalCavern) is true || data?.ContainsKey(TalentCore.Key_Upheaval) is true) && e.NewLocation is MineShaft or VolcanoDungeon)
                {
                    List<Vector2> validcoords = (from tileobjpair in e.NewLocation.Objects.Pairs
                                                 where tileobjpair.Value is not null && tileobjpair.Value.IsBlandStone()
                                                 select tileobjpair.Key).ToList();
                    
                    if (TalentUtility.AllPlayersHaveTalent("Mining_Crystal_Cavern") && (data?.ContainsKey(TalentCore.Key_CrystalCavern) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(0.15))
                    {
                        TalentUtility.GemAndGeodeNodes(true, validcoords);
                    }
                    else if (TalentUtility.AllPlayersHaveTalent("Mining_Upheaval") && (data?.ContainsKey(TalentCore.Key_Upheaval) is true || e.NewLocation is MineShaft or VolcanoDungeon) && Game1.random.NextBool(0.15))
                    {
                        TalentUtility.GemAndGeodeNodes(false, validcoords);
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
                        else if (Game1.player.ActiveObject is not null and Trinket trinket && trinket.QualifiedItemId == "(TR)FairyBox" && TalentUtility.CurrentPlayerHasTalent("Combat_HiddenBenefits"))
                        {
                            if (value is HoeDirt dirt && dirt.crop is not null && !dirt.crop.fullyGrown.Value && dirt.crop.currentPhase.Value != dirt.crop.phaseDays.Count - 1)
                            {
                                //here we go, buff here
                                bool shouldGrow = false;
                                if (!trinket.modData.TryAdd(TalentCore.Key_HiddenBenefit_FairyBox, "1"))
                                {
                                    if (trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox] is not "3")
                                    {
                                        shouldGrow = true;
                                        trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox] = (int.Parse(trinket.modData[TalentCore.Key_HiddenBenefit_FairyBox]) + 1).ToString();
                                    }
                                    else
                                        Game1.pauseThenMessage(250, Helper.Translation.Get("Message.FairyBreak"));
                                }
                                else
                                    shouldGrow = true;
                                if (shouldGrow)
                                {
                                    if (!dirt.crop.modData.TryAdd(TalentCore.Key_HiddenBenefit_Crop, "true"))
                                    {
                                        if (dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] == "true")
                                            Game1.pauseThenMessage(250, Helper.Translation.Get("Message.AlreadyFertilized"));
                                        else
                                        {
                                            Game1.playSound("wand");
                                            dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] = "true";
                                            Game1.pauseThenMessage(250, Helper.Translation.Get("Message.Fertilized"));
                                        }
                                    }
                                    else
                                    {

                                        Game1.playSound("wand");
                                        Game1.pauseThenMessage(250, Helper.Translation.Get("Message.Fertilized"));
                                        dirt.crop.modData[TalentCore.Key_HiddenBenefit_Crop] = "true";
                                    }
                                }
                            }
                        }
                    }
                    else if (Game1.player.CurrentTool is FishingRod rod && TalentUtility.CurrentPlayerHasTalent("Fishing_Take_It_Slow"))
                    {
                        if (rod.CanUseTackle() && rod.GetTackleQualifiedItemIDs().Contains("(O)Kedi.VPP.SnailTackle"))
                        {
                            rod.castingTimerSpeed = 0.0007f;
                        }
                    }
                    else if (Game1.player.ActiveObject?.QualifiedItemId == "(TR)BasiliskPaw" && TalentUtility.CurrentPlayerHasTalent("Combat_HiddenBenefits") && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.Tile, out var value2))
                    {
                        if (Game1.player.couldInventoryAcceptThisItem(value2))
                        {
                            Game1.player.addItemByMenuIfNecessary(value2);
                        }
                        else
                        {
                            Game1.hudMessages.Add(new("Full inventory", HUDMessage.error_type));
                        }
                    }
                    else
                    {
                        foreach (var item in Game1.player.currentLocation.resourceClumps)
                        {
                            if (item is GiantCrop crop && crop.getBoundingBox().Contains(e.Cursor.GrabTile * 64))
                            {
                                TerrainFeatureTapper(crop, e);
                                break;
                            }
                        }
                    }
                }
                else if (e.Button.IsActionButton())
                {
                    if (Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object value))
                    {
                        if (CoreUtility.CurrentPlayerHasProfession(70) && value.IsTapper())
                            value.shakeTimer = 0;
                        else if (value.heldObject.Value is null && value.IsSprinkler() && Game1.player.ActiveObject?.QualifiedItemId is "(O)Kedi.VPP.Fertigator" && TalentUtility.CurrentPlayerHasTalent("Fertigation"))
                        {
                            Chest chest = new()
                            {
                                SpecialChestType = Chest.SpecialChestTypes.Enricher
                            };
                            Game1.player.ActiveObject.heldObject.Value = chest;
                            value.heldObject.Value = Game1.player.ActiveObject;
                            Game1.player.ActiveObject.Stack--;
                            if (Game1.player.ActiveObject.Stack == 0)
                            {
                                Game1.player.ActiveObject = null;
                            }
                        }
                        else if (Game1.player.ActiveItem is TrinketRing ring && value.QualifiedItemId == "(BC)Kedi.VPP.Accessorise.TrinketWorkbench")
                        {
                            Item result = TalentUtility.AccessoriseMachineRule(value, ring, false, null, out int? minuteOverride);
                            if (result is Trinket trinket)
                            {
                                Game1.player.addItemToInventory(trinket);
                                for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
                                {
                                    Item item = Game1.player.Items[i];
                                    if (item != null && item == Game1.player.ActiveItem)
                                    {
                                        Game1.player.Items[i] = null;                                        
                                    }
                                }
                            }
                        }
                    }
                    else if (Game1.player.ActiveObject?.QualifiedItemId is "(O)96" or "(O)97" or "(O)98" or "(O)99" && Game1.player.canUnderstandDwarves && TalentUtility.CurrentPlayerHasTalent("Mining_Elder_Scrolls"))
                    {
                        Game1.player.gainExperience(3, 250);
                        Game1.player.currentLocation.playSound("shwip");
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("Message.ReadDwarfScroll")));
                        Game1.player.ActiveObject.Stack--;
                        if (Game1.player.ActiveObject.Stack == 0)
                        {
                            Game1.player.ActiveObject = null;
                        }
                    }
                    else if (Game1.player.CurrentTool is not null and Slingshot slingshot && TalentUtility.CurrentPlayerHasTalent("TripleShot"))
                    {
                        foreach (var item in Game1.player.enchantments)
                            if (item is AutoFireEnchantment)
                                return;

                        slingshot.beginUsing(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
                        slingshot.lastUser = Game1.player;
                        Game1.player.usingSlingshot = true;
                        TalentCore.IsActionButtonUsed.Value = true;
                    }
                }
            }
            else if (Game1.activeClickableMenu is GeodeMenu menu)
            {
                if (menu.heldItem is not null && menu.heldItem.Stack != GeodeStackSize.Value)
                {
                    TalentUtility.DetermineGeodeDrop(menu.heldItem, true);
                    GeodeStackSize.Value = menu.heldItem.Stack;
                }
            }
            else if (Game1.activeClickableMenu is GameMenu menu2)
            {
                if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
                {
                    if (menu2.pages[menu2.currentTab] is SkillsPage page)
                    {
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.LittleArrow.Value.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            DisplayHandler.IsOverlayActive.Value = !DisplayHandler.IsOverlayActive.Value;
                            page.skillBars = DisplayHandler.IsOverlayActive.Value ? DisplayHandler.MyCustomSkillBars.Value.ToList() : DisplayHandler.VanillaSkillBars.Value.ToList();
                        }
                    }
                    else if (menu2.pages[menu2.currentTab] is NewSkillsPage pagee)
                    {
                        if (CoreUtility.IsOverlayValid() && DisplayHandler.LittleArrow.Value.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                        {
                            DisplayHandler.IsOverlayActive.Value = !DisplayHandler.IsOverlayActive.Value;
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
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
                {
                    if (menu.pages[menu.currentTab] is SkillsPage page)
                    {
                        for (int i = 0; i < page.skillAreas.Count; i++)
                        {
                            if (page.skillAreas[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i, menu);
                            }
                        }
                    }
                    else if (menu.pages[menu.currentTab] is NewSkillsPage pagee)
                    {
                        for (int i = 0; i < pagee.skillAreas.Count; i++)
                        {
                            if (pagee.skillAreas[i].containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i, menu);
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
            else if (TreeOrCrop is not FruitTree and GiantCrop)
            {
                return;
            }

            if (Game1.player.CurrentTool is Pickaxe or Axe && Game1.player.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object obj) && obj.IsTapper())
            {
                TreeOrCrop.modData[Key_TFHasTapper] = "false";
            }
            else if (CoreUtility.CurrentPlayerHasProfession(70) && TreeOrCrop is not null)
            {
                if (Game1.player.ActiveObject?.IsHeldOverHead() == true && Game1.player.ActiveObject?.IsTapper() == true && e.Button.IsUseToolButton())
                {
                    FruitTreeData fruitTreeData = (TreeOrCrop as FruitTree)?.GetData() ?? null;
                    GiantCropData giantCropData = (TreeOrCrop as GiantCrop)?.GetData() ?? null;
                    var obsj = Game1.player.ActiveObject;

                    obsj.modData[Key_TFTapperDaysLeft] = "0";

                    StardewValley.Object dsdsd;
                    if (fruitTreeData?.CustomFields?.TryGetValue(Key_FruitTreeOrGiantCrop, out string value) is true && value is not null)
                    {
                        dsdsd = ItemRegistry.Create<StardewValley.Object>(value);
                        dsdsd.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                    }
                    else if (giantCropData?.CustomFields?.TryGetValue(Key_FruitTreeOrGiantCrop, out string value2) is true && value2 is not null)
                    {
                        dsdsd = ItemRegistry.Create<StardewValley.Object>(value2);
                        dsdsd.modData?.TryAdd("Kedi.VPP.CurrentPreserveType", "Other");
                    }
                    else
                    {
                        _ = ManagerUtility.GetProduceTimeBasedOnPrice(TreeOrCrop, out StardewValley.Object produce);
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
                else if (TreeOrCrop is FruitTree tree && tree.fruit.Count > 0)
                {
                    tree.shake(TreeOrCrop.Tile, true);
                }
            }
        }
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            TalentCore.IsDayStartOrEnd = true;
            Utility.ForEachLocation(loc =>
            {
                if (!loc.modData.TryAdd(TalentCore.Key_WasRainingHere, loc.IsRainingHere().ToString().ToLower()))
                {
                    loc.modData[TalentCore.Key_WasRainingHere] = loc.IsRainingHere().ToString().ToLower();
                }

                if (TalentUtility.HostHasTalent("Foraging_Local_Knowledge"))
                {
                    var locdata = loc.GetData()?.CustomFields ?? new();
                    if (locdata?.ContainsKey(TalentCore.Key_LocalKnowledge) is true || loc is Forest or Mountain or Beach)
                    {
                        foreach (var item in loc.Objects.Pairs)
                        {
                            if (item.Value.isForage())
                            {
                                loc.Objects.Remove(item.Key);
                            }
                        }
                    }
                }
                foreach (var item in loc.terrainFeatures.Pairs)
                {
                    if (item.Value is HoeDirt hoeDirt && hoeDirt.crop is not null and Crop crop && !crop.dead.Value)
                    {
                        if (TalentUtility.HostHasTalent("Farming_Nourishing_Rain") && loc.modData.TryGetValue(TalentCore.Key_WasRainingHere, out string value2) && value2 is "true")
                        {
                            crop.dayOfCurrentPhase.Value++;
                        }
                        else if (TalentUtility.HostHasTalent("Farming_Efflorescence") && !ItemRegistry.GetData(crop.GetData().HarvestItemId).IsErrorItem && ItemRegistry.GetData(crop.GetData().HarvestItemId).Category == StardewValley.Object.flowersCategory)
                        {
                            if (!crop.modData.ContainsKey(TalentCore.Key_Efflorescence) || (crop.modData.TryGetValue(TalentCore.Key_Efflorescence, out string value3) && value3 is "false"))
                            {
                                if (crop.dayOfCurrentPhase.Value is 0 or 1 or 2)
                                {
                                    if (!crop.modData.TryAdd(TalentCore.Key_Efflorescence, "true"))
                                    {
                                        crop.modData[TalentCore.Key_Efflorescence] = "true";
                                    }
                                    crop.dayOfCurrentPhase.Value++;
                                }
                            }
                            else if (!crop.modData.TryAdd(TalentCore.Key_Efflorescence, "false"))
                            {
                                crop.modData[TalentCore.Key_Efflorescence] = "false";
                            }
                        }
                        else if (TalentUtility.HostHasTalent("Farming_Tropical_Bliss") && loc.InIslandContext() && (crop.GetData()?.Seasons.Contains(Season.Summer) is true || crop.GetData()?.Seasons.Count > 1))
                        {
                            if (!crop.modData.ContainsKey(TalentCore.Key_Tropical_Bliss) || (crop.modData.TryGetValue(TalentCore.Key_Tropical_Bliss, out string value3) && value3 is "false"))
                            {
                                if (crop.dayOfCurrentPhase.Value is 0 or 1 or 2)
                                {
                                    if (!crop.modData.TryAdd(TalentCore.Key_Tropical_Bliss, "true"))
                                    {
                                        crop.modData[TalentCore.Key_Tropical_Bliss] = "true";
                                    }
                                    crop.dayOfCurrentPhase.Value++;
                                }
                            }
                            else if (!crop.modData.TryAdd(TalentCore.Key_Tropical_Bliss, "false"))
                            {
                                crop.modData[TalentCore.Key_Tropical_Bliss] = "false";
                            }
                        }
                    }
                }

                return true;

            }, true, false);
            
            Utility.ForEachItem(item =>
            {
                if (item is not null and StardewValley.Object obj)
                {
                    if (obj is CrabPot crabPot && crabPot.heldObject.Value is not null)
                    {
                        if (TalentUtility.AnyPlayerHasTalent("Fishing_Bait_And_Switch"))
                        {
                            var list = crabPot.Location.GetData().Fish;

                            ItemQueryContext context = new(crabPot.Location, Game1.player, Game1.random);

                            var normalData = (from keyvaluepair in DataLoader.Fish(Game1.content)
                                              where !keyvaluepair.Value.Contains("trap")
                                              select keyvaluepair.Key).ToList();

                            List<string> locFishList = new();
                            foreach (var fesh in list)
                            {
                                ItemQueryResult result = ItemQueryResolver.TryResolve(fesh, context).FirstOrDefault();
                                if  (fesh?.ItemId is not null or "" && result?.Item?.QualifiedItemId is not null)
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
                                if (crabPot.heldObject.Value.HasContextTag("fish_crab_pot") && endList.Count > 0)
                                {
                                    crabPot.heldObject.Value = ItemRegistry.Create<StardewValley.Object>(Game1.random.ChooseFrom(endList), quality: crabPot.heldObject.Value.Quality);
                                    crabPot.heldObject.Value.Quality++;
                                    crabPot.heldObject.Value.FixQuality();
                                }
                            }
                        }
                    }
                    if (obj.IsTapper() is true && obj.heldObject.Value is not null)
                    {
                        if (CoreUtility.AnyPlayerHasProfession(49) && Game1.random.NextBool(0.25) && obj.Location.terrainFeatures.TryGetValue(obj.TileLocation, out TerrainFeature terrainFeature) && terrainFeature is Tree or FruitTree or GiantCrop)
                        {
                            obj.heldObject.Value.Stack += Game1.random.Next(1, 3);
                        }
                        if (TalentUtility.AnyPlayerHasTalent("Foraging_Spring_Thaw") && Game1.random.NextBool(0.15))
                        {
                            obj.heldObject.Value = TalentUtility.GetBigTapperOutput(obj.heldObject.Value);
                        }
                        if (TalentUtility.AnyPlayerHasTalent("Foraging_Accumulation"))
                        {
                            if (Game1.random.NextBool(0.15) )
                            {
                                obj.heldObject.Value.Quality++;
                                obj.heldObject.Value.FixQuality();
                            }
                        }
                    }
                    if (obj.QualifiedItemId == "(BC)10" && obj.heldObject.Value is not null)
                    {
                        int tiles = TalentUtility.FlowersInBeeHouseRange(obj.Location, obj.TileLocation);
                        if (TalentUtility.AnyPlayerHasTalent("Farming_Abundance") && Game1.random.NextBool(tiles * 0.05))
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
                if (building.GetIndoors() is SlimeHutch slimeHutch && CoreUtility.CurrentPlayerHasProfession(78, farmerID: building.owner.Value))
                {
                    var str = (!slimeHutch.waterSpots.Contains(true)).ToString().ToLower();
                    if (!slimeHutch.modData.TryAdd(Key_IsSlimeHutchWatered, str))
                    {
                        slimeHutch.modData[Key_IsSlimeHutchWatered] = str;
                    }
                }
                else if (building is FishPond fishPond && TalentUtility.CurrentPlayerHasTalent("Fishing_Exsquidsite", farmerID: fishPond.owner.Value))
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
            if (!Game1.player.modData.TryAdd(TalentCore.Key_TalentPoints, TalentCore.TalentPointCount.Value.ToString()))
                Game1.player.modData[TalentCore.Key_TalentPoints] = TalentCore.TalentPointCount.Value.ToString();
        }
        private void OnLevelChanged(object sender, LevelChangedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.NewLevel > 10 && (int)e.Skill < 5)
                {
                    DisplayHandler.ShouldHandleSkillPage.Value = true;
                }
                TalentCore.AddTalentPoint();
            }
        }
    }
}
