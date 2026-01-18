using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Utilities;
using StardewValley.TerrainFeatures;
using HarmonyLib;
using StardewValley.Buildings;
using VanillaPlusProfessions.Talents.Patchers;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using SpaceCore.Interface;
using VanillaPlusProfessions.Craftables;
using VanillaPlusProfessions.Compatibility;

namespace VanillaPlusProfessions
{
    internal class DisplayHandler
    {
        internal readonly static PerScreen<DisplayHandler> CoreDisplayHandler = new(createNewState: () => new());

        internal ClickableTextureComponent[] MyCustomSkillBars;
        internal ClickableTextureComponent[] VanillaSkillBars;
        internal ClickableTextureComponent LittlePlus;
        internal ClickableTextureComponent GiveFrogEggBack;
        internal bool IsOverlayActive;
        internal bool WasSkillMenuRaised;
        internal bool ShouldHandleSkillPage;
        internal bool OpenTalentMenuCooldown;
        internal Texture2D SkillIcons;
        internal Texture2D ProfessionIcons;

        internal int lossAmount;
        internal bool XPDisplayInstalled = false;
        internal int TalentMenuHintApplied;

        internal void Initialize(ModEntry modEntry)
        {
            CoreDisplayHandler.Value = this;
            SkillIcons = ModEntry.CoreModEntry.Value.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["SkillBars"]);
            ProfessionIcons = ModEntry.CoreModEntry.Value.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ProfessionIcons"]);

            modEntry.Helper.Events.Display.MenuChanged += OnMenuChanged;
            modEntry.Helper.Events.Display.WindowResized += OnWindowResized;
            modEntry.Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            modEntry.Helper.Events.Display.RenderedStep += OnRenderedStep;
            modEntry.Helper.Events.Display.RenderedHud += OnRenderedHud;
            modEntry.Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        public static DisplayHandler GetMe()
        {
            return CoreDisplayHandler.Value;
        }

        internal void InitializeBetterGameMenu()
        {
            if (ModEntry.CoreModEntry.Value.BetterGameMenuAPI is not null)
            {
                ModEntry.CoreModEntry.Value.BetterGameMenuAPI?.OnPageCreated(OnPageCreated);
            }
        }
        private void OnPageCreated(IPageCreatedEvent e)
        {
            if (e.Page is SkillsPage or NewSkillsPage && CoreUtility.IsOverlayValid() && ShouldHandleSkillPage)
                HandleSkillPage(e.Page, e.Menu);
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is PondQueryMenu pondMenu)
            {
                if ((e.Button.IsActionButton() || e.Button.IsUseToolButton()) && GiveFrogEggBack?.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)) is true)
                {
                    string s = (AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(pondMenu) as FishPond).modData[Constants.Key_HiddenBenefit_FrogEggs];
                    if (s.Length > 0)
                    {
                        Game1.player.addItemByMenuIfNecessary(s.StringToTrinket());
                        (AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(pondMenu) as FishPond).modData[Constants.Key_HiddenBenefit_FrogEggs] = "";
                        GiveFrogEggBack.visible = false;
                    }
                }
            }
            if (Game1.activeClickableMenu is ForgeMenu forgeMenu && forgeMenu.startTailoringButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)) && e.Button.IsUseToolButton() && true)
            {
                if (forgeMenu.rightIngredientSpot.item is not null and StardewValley.Object obj && obj.QualifiedItemId == "(O)82" && obj.Stack >= 4)
                {
                    obj.Stack -= 4;
                    if (obj.Stack <= 0)
                        forgeMenu.rightIngredientSpot.item = null;
                }
            }
        }
        private void OnRenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (e.Step is StardewValley.Mods.RenderSteps.World_Sorted)
            {
                MachineryEventHandler.ThisIsMe.OnWorldDrawn(e.SpriteBatch);
                foreach (var item in Game1.player.currentLocation.Objects.Pairs)
                {
                    if (item.Value.IsSprinkler() && item.Value.heldObject.Value is not null && item.Value.heldObject.Value.QualifiedItemId is "(O)Kedi.VPP.PressureNozzleEnricher")
                    {
                        var data = ItemRegistry.GetData(item.Value.heldObject.Value.ItemId);
                        var rect = data.GetSourceRect();
                        e.SpriteBatch.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(item.Key.X * 64f, item.Key.Y * 64f - 8f)), rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, item.Key.Y - 10000000 / 8700000f);
                    }
                    else if (item.Value.IsTapper() && Game1.player.currentLocation.terrainFeatures.TryGetValue(item.Key, out TerrainFeature terrainFeature) && terrainFeature is FruitTree or GiantCrop)
                    {
                        if (terrainFeature.modData.TryGetValue(Constants.Key_TFHasTapper, out string value) && value.ToLower() is "true")
                        {
                            var data = ItemRegistry.GetData(terrainFeature.modData[Constants.Key_TFTapperID]);
                            var rect = data.GetSourceRect();
                            rect.Height /= 2;
                            e.SpriteBatch.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(terrainFeature.Tile.X * 64f, terrainFeature.Tile.Y * 64f - 64f)), rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (terrainFeature.getBoundingBox().Bottom + 10000) / 87000f - terrainFeature.Tile.X / 1000000f);
                        }
                    }
                }
                
            }
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is LevelUpMenu or SkillLevelUpMenu)
            {
                if (!WasSkillMenuRaised)
                    ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
                HandleLevelUpMenu(e.NewMenu);
            }
            else if (Game1.player.newLevels.Count == 0 && TalentCore.IsDayStartOrEnd)
            {
                WasSkillMenuRaised = true;
                ModEntry.CoreModEntry.Value.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
            }

            // not using ModEntry.GetGameMenuPage() because BetterGameMenu won't have a SkillsPage initialized at MenuChanged
            if (e.NewMenu is GameMenu menu1)
            {
                TalentMenuHintApplied = 0;
                if (menu1.pages[1] is SkillsPage or NewSkillsPage)
                {
                    if (CoreUtility.IsOverlayValid() && ShouldHandleSkillPage)
                    {
                        OpenTalentMenuCooldown = true;
                        HandleSkillPage(menu1.pages[1], menu1);
                    }
                }
            }

            if (e.OldMenu is BobberBar bobberBar)
            {
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_HereFishyFishy))
                {
                    if (bobberBar.distanceFromCatching is 0f)
                        lossAmount++;
                    else
                        lossAmount = 0;
                }
            }
            else if (e.NewMenu is BobberBar bobberBar1)
            {
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_HereFishyFishy))
                {
                    if (lossAmount > 1)
                    {
                        bobberBar1.bobberBarHeight += lossAmount * 20;
                        bobberBar1.bobberBarPos -= lossAmount * 20;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_OneFishTwoFish))
                {
                    if (bobberBar1.challengeBaitFishes > -1)
                    {
                        bobberBar1.challengeBaitFishes = 4;
                    }
                }
            }

            if (ModEntry.IsGameMenu(e.OldMenu) || ModEntry.IsGameMenu(e.NewMenu))
            {
                IsOverlayActive = false;
            }

            if (e.OldMenu is CraftingPage craftingPage && craftingPage.cooking && e.NewMenu is null)
                TalentCore.TalentCoreEntry.Value.IsCookoutKit = false;

            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu is null)
                    return;
                if (menu.context is Chest chest && TalentUtility.AnyPlayerHasTalent(Constants.Talent_MiniFridgeBigSpace))
                {
                    if (chest?.QualifiedItemId == "(BC)216")
                    {
                        menu.source = 1;
                        menu.setSourceItem(chest);

                    }
                }
                if (menu.context is FishingRod && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Buccaneer) is true)
                {
                    for (int i = 0; i < menu.ItemsToGrabMenu.actualInventory.Count; i++)
                    {
                        if (menu.ItemsToGrabMenu.actualInventory[i] is null)
                            continue;

                        if (ContentEditor.BuccaneerData.TryGetValue(menu.ItemsToGrabMenu.actualInventory[i].QualifiedItemId, out string value))
                        {
                            string[] strings = value.Split('/');
                            Item item;
                            if (strings.Length > 1)
                                item = ItemRegistry.Create(strings[1], int.TryParse(strings[2], out int result) ? result : 1);
                            else
                                item = ItemRegistry.Create(strings[0]);

                            if (strings[0] is "Artifact")
                            {
                                if (LibraryMuseum.HasDonatedArtifact(menu.ItemsToGrabMenu.actualInventory[i].QualifiedItemId))
                                {
                                    if (strings[1].StartsWith("(TR)") && !Trinket.CanSpawnTrinket(Game1.player))
                                    {
                                        continue;
                                    }
                                    menu.ItemsToGrabMenu.inventory[i].item = item;
                                    menu.ItemsToGrabMenu.actualInventory[i] = item;
                                }
                                continue;
                            }

                            menu.ItemsToGrabMenu.actualInventory[i] = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                            menu.ItemsToGrabMenu.inventory[i].item = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                        }
                    }
                }
            }
            if (e.NewMenu is PondQueryMenu querye && AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(querye) is FishPond pond)
            {
                GiveFrogEggBack = pond.modData.TryGetValue(Constants.Key_HiddenBenefit_FrogEggs, out string value) && value != ""
                    ? new(new((querye.xPositionOnScreen * 9 / 10) - ((querye.xPositionOnScreen * 9 / 10) % 4) + 4, (querye.yPositionOnScreen * 14 / 5) - ((querye.yPositionOnScreen * 9 / 10) % 4) + 4, 64, 64), SkillIcons, new(0, 27, 16, 16), 4f, false)
                    : null;
                if (GiveFrogEggBack is not null)
                {
                    GiveFrogEggBack.myID = 46780;
                    querye.populateClickableComponentList();
                    querye.emptyButton.leftNeighborID = GiveFrogEggBack.myID;
                    querye.changeNettingButton.leftNeighborID = GiveFrogEggBack.myID;
                    querye.okButton.leftNeighborID = GiveFrogEggBack.myID;
                    querye.allClickableComponents.Add(GiveFrogEggBack);
                }
            }

            MachineryEventHandler.ThisIsMe.OnMenuChanged(e);
        }
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu is null && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_ForageCombat))
            {
                foreach (NPC @char in Game1.player.currentLocation.characters)
                {
                    if (@char is Monster monster && !Utility.isOnScreen(monster.Tile * 64f + new Vector2(32f, 32f), 64))
                    {
                        Rectangle bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
                        Vector2 onScreenPos;
                        float rotation = 0f;
                        int x = Convert.ToInt32(monster.Tile.X * 64f);
                        int y = Convert.ToInt32(monster.Tile.Y * 64f);

                        if (x > Game1.viewport.MaxCorner.X - 64)
                        {
                            onScreenPos.X = bounds.Right - 8;
                            rotation = 0.5f;
                        }
                        else if (x < Game1.viewport.X)
                        {
                            onScreenPos.X = 8f;
                            rotation = -0.5f;
                        }
                        else
                            onScreenPos.X = x - Game1.viewport.X;

                        if (y > Game1.viewport.MaxCorner.Y - 64)
                        {
                            onScreenPos.Y = bounds.Bottom - 8;
                            rotation = 1f;
                        }
                        else onScreenPos.Y = y < Game1.viewport.Y ? 8f : y - Game1.viewport.Y;

                        if (onScreenPos.X == 8f && (onScreenPos.Y == 8f || onScreenPos.Y == bounds.Bottom - 8))
                            rotation += 0.25f;

                        if (onScreenPos.X == bounds.Right - 8 && (onScreenPos.Y == 8f || onScreenPos.Y == bounds.Bottom - 8))
                            rotation -= 0.25f;

                        e.SpriteBatch.Draw(Game1.mouseCursors, Utility.makeSafe(onScreenPos, new(20f, 16f)), new(412, 495, 5, 4), Color.Red, rotation * (float)Math.PI, new(2f, 2f), 4f, SpriteEffects.None, 1f);
                    }
                }
            }
            if (Game1.player.CurrentTool is not null and Slingshot slingshot && TalentCore.TalentCoreEntry.Value.IsActionButtonUsed)
            {
                slingshot.draw(e.SpriteBatch);
            }
        }
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (ModEntry.IsGameMenu(Game1.activeClickableMenu))
            {
                var menuPage = ModEntry.GetGameMenuPage(Game1.activeClickableMenu);
                if (menuPage is SkillsPage page)
                {
                    //Yes, this is practically dead code because of SpaceCore, but I prefer to keep this block in case this changes again.
                    if (IsOverlayActive)
                    {
                        string hoverText = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<string>(page, "hoverText").GetValue();
                        string hoverTitle = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<string>(page, "hoverTitle").GetValue();
                        for (int FF = 0; FF < 5; FF++)
                        {
                            int standartIndex = StandardizeSkillIndexes(FF);
                            int skillLevel = Game1.player.GetSkillLevel(standartIndex);
                            if (TalentMenuHintApplied < 4)
                            {
                                page.skillAreas[FF].hoverText += "(Click to open talent menu)"; //TODO Make this into i18n
                                TalentMenuHintApplied++;
                            }
                            for (int SS = 0; SS < 10; SS++)
                            {
                                if (SS is 4 or 9)
                                {
                                    if (skillLevel < (11 + SS))
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS is 9 ? 24 : 0), page.skillAreas[FF].bounds.Y), new Rectangle(16, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                }
                                else
                                {
                                    if (skillLevel < (11 + SS))
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS > 3 ? 24 : 0), page.skillAreas[FF].bounds.Y), new Rectangle(0, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    else
                                    {
                                        if (skillLevel == (11 + SS))
                                        {
                                            int XPcurrent = Game1.player.experiencePoints[standartIndex];
                                            int XPprior = ModEntry.CoreModEntry.Value.levelExperiences[SS];
                                            int XPnext = ModEntry.CoreModEntry.Value.levelExperiences[SS + 1];
                                            float progress = Math.Clamp((float)(XPcurrent - XPprior) / (XPnext - XPprior), 0f, 1f); // Current progress through level (0.00 to 1.00 float, proportion)
                                            int fillHeight = (int)(9 * progress); // Height of fill as portion of progress * 9 (height of standard bar)
                                            int yOffset = ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0; // Offset of texture (depending on colourblind)
                                            int fillY = yOffset + (9 - fillHeight); // Pixel coordinate of fill
                                            Vector2 pos = new(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS > 3 ? 24 : 0), page.skillAreas[FF].bounds.Y);
                                            e.SpriteBatch.Draw(SkillIcons, pos, new Rectangle(0, yOffset, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f); //draws empty
                                            if (fillHeight > 0) // only draw if at least 1 pixel to avoid 0-draw
                                            {
                                                Rectangle src = new Rectangle(8, fillY, 7, fillHeight);
                                                Vector2 dest = pos + new Vector2(0, 9 - fillHeight) * 4f;

                                                e.SpriteBatch.Draw(SkillIcons, dest, src, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.51f); // draws partial fill
                                            }
                                        }
                                        e.SpriteBatch.Draw(SkillIcons,
                                        new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS > 3 ? 24 : 0), page.skillAreas[FF].bounds.Y),
                                        new Rectangle(8, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    }
                                }
                            }
                        }
                        if (hoverText.Length > 0)
                        {
                            StringBuilder sb = new();
                            sb.Append(hoverText);
                            drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                        }
                        else if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.hoverText.Length > 0)
                        {
                            StringBuilder sb = new();
                            sb.Append(gameMenu.hoverText);
                            drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                        }


                        foreach (ClickableTextureComponent c in MyCustomSkillBars)
                        {
                            if (c.scale == 0f)
                            {
                                IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), c.bounds.X - 16 - 8, c.bounds.Y - 16 - 16, 96, 96, Color.White, drawShadow: false);
                                e.SpriteBatch.Draw(ProfessionIcons, new Vector2(c.bounds.X - 8, c.bounds.Y - 16), new Rectangle((Convert.ToInt32(c.name) - 467830) % 6 * 16, (Convert.ToInt32(c.name) - 467830) / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                            }
                        }
                    }
                    if (CoreUtility.IsOverlayValid())
                    {
                        LittlePlus.draw(e.SpriteBatch);
                        page.drawMouse(e.SpriteBatch);
                    }
                }
                else if (menuPage is NewSkillsPage page2)
                {
                    string hoverText = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<string>(page2, "hoverText").GetValue();
                    string hoverTitle = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<string>(page2, "hoverTitle").GetValue();
                    Dictionary<int, int> skillAreaSkillIndexes = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<Dictionary<int, int>>(page2, "skillAreaSkillIndexes").GetValue();
                    int skillScrollOffset = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<int>(page2, "skillScrollOffset").GetValue();
                    int LastVisibleSkillIndex = ModEntry.CoreModEntry.Value.Helper.Reflection.GetProperty<int>(page2, "LastVisibleSkillIndex").GetValue();
                    bool enableShadow = false;
                    foreach (var item in page2.skillAreas)
                    {
                        if (skillAreaSkillIndexes.TryGetValue(item.myID, out int skillIndex) && (skillIndex < skillScrollOffset || skillIndex > LastVisibleSkillIndex))
                            continue;
                        //4, because we need to exclude the Misc/Daily Life tree.
                        if (TalentMenuHintApplied < 5 + ModEntry.CoreModEntry.Value.VanillaPlusProfessionsAPI.CustomTalentTrees.Count)
                        {
                            if (!item.name.StartsWith('C') || ModEntry.CoreModEntry.Value.VanillaPlusProfessionsAPI.CustomTalentTrees.ContainsKey(item.name))
                            {
                                item.hoverText += "\n(Click to open talent menu)";
                                TalentMenuHintApplied++;
                            }
                        }
                        if (IsOverlayActive)
                        {                            
                            if (item.name.StartsWith('C'))
                            {
                                string thisSkillId = ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetCustomSkills().First(s => item.name[1..] == ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetDisplayNameOfCustomSkill(s));
                                int level = ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetLevelForCustomSkill(Game1.player, thisSkillId);

                                for (int i = 0; i < 10; i++)
                                {
                                    if (i is 4 or 9)
                                    {
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i is 9 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(level < (1 + i) ? 16 : 30, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    }
                                    else
                                    {
                                        if (level < (1 + i))
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(0, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        else
                                        {
                                            e.SpriteBatch.Draw(SkillIcons,
                                            new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)),
                                            new Rectangle(8, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int standartIndex = StandardizeSkillIndexes(int.Parse(item.name));
                                int level = Game1.player.GetUnmodifiedSkillLevel(standartIndex); 
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i is 4 or 9)
                                    {
                                        e.SpriteBatch.Draw(SkillIcons, new(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i is 9 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new(level < (11 + i) == true ? 16 : 30, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    }
                                    else
                                    {
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(level >= (i + 11) ? 8 : 0, ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    }
                                }
                                if (CoreDisplayHandler.Value.XPDisplayInstalled)
                                {
                                    for (int i = 1; i < 10; i++)
                                    {
                                        if (level == (i + 10))
                                        {
                                            int XPcurrent = Game1.player.experiencePoints[standartIndex];
                                            int XPprior = ModEntry.CoreModEntry.Value.levelExperiences[i - 1];
                                            int XPnext = ModEntry.CoreModEntry.Value.levelExperiences[i];
                                            float progress = Math.Clamp((float)(XPcurrent - XPprior) / (XPnext - XPprior), 0f, 1f); // Current progress through level (0.00 to 1.00 float, proportion)
                                            int fillHeight = (int)(9 * progress); // Height of fill as portion of progress * 9 (height of standard bar)
                                            int yOffset = ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges ? 9 : 0; // Offset of texture (depending on colourblind)
                                            int fillY = yOffset + (9 - fillHeight); // Pixel coordinate of fill
                                            Vector2 pos = new(page2.skillAreas[standartIndex].bounds.X + page2.skillAreas[standartIndex].bounds.Width + (36 * i) + (i > 4 ? 48 : 24), page2.skillAreas[standartIndex].bounds.Y);

                                            e.SpriteBatch.Draw(SkillIcons, pos, new Rectangle(i is 4 or 9 ? 16 : 0, yOffset, i is 4 or 9 ? 13 : 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f); //draws empty

                                            int cursorY = Game1.getMouseY(true) + (skillScrollOffset * 56);
                                            if (item.bounds.Y < cursorY && cursorY < item.bounds.Height + item.bounds.Y && Game1.getMouseX(true) > item.bounds.X && Game1.getMouseX(true) < menuPage.width - 100 + menuPage.xPositionOnScreen && hoverText?.Length == 0)
                                            {
                                                hoverText = $"{XPcurrent} / {XPnext} ({(int)(progress * 100)}%)";
                                                enableShadow = true;
                                            }
                                            if (fillHeight > 0) // only draw if at least 1 pixel to avoid 0-draw
                                            {
                                                Rectangle src = new(i is 4 or 9 ? 30 : 8, fillY, i is 4 or 9 ? 13 : 7, fillHeight);
                                                Vector2 dest = pos + new Vector2(0, 9 - fillHeight) * 4f;

                                                e.SpriteBatch.Draw(SkillIcons, dest, src, Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.51f); // draws partial fill
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            foreach (ClickableTextureComponent c in MyCustomSkillBars)
                            {
                                if (c.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true) + (skillScrollOffset * 56)) && c.hoverText.Length > 0 && !c.name.Equals("-1"))
                                {
                                    IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), c.bounds.X - 24, c.bounds.Y - 32 - (skillScrollOffset * 56), 96, 96, Color.White, drawShadow: false);
                                    if (c.name.StartsWith('C'))
                                    {
                                        Texture2D profIcon = SpaceCore.Skills.GetSkillList()
                                            .SelectMany(s => SpaceCore.Skills.GetSkill(s).Professions)
                                            .Single(p => p.Id == c.name[1..])
                                            .Icon;
                                        e.SpriteBatch.Draw(profIcon, new Vector2(c.bounds.X - 8, c.bounds.Y - 16 - (skillScrollOffset * 56)), new Rectangle(0, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                                    }
                                    else
                                    {
                                        e.SpriteBatch.Draw(ProfessionIcons, new Vector2(c.bounds.X - 8, c.bounds.Y - 16 - (skillScrollOffset * 56)), new Rectangle((Convert.ToInt32(c.name) - 467830) % 6 * 16, (Convert.ToInt32(c.name) - 467830) / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                                    }
                                }
                            }
                        }
                    }
                    if (CoreUtility.IsOverlayValid())
                    {
                        LittlePlus.draw(e.SpriteBatch);
                        page2.drawMouse(e.SpriteBatch);
                    }
                    if (hoverText.Length > 0)
                    {
                        StringBuilder sb = new();
                        sb.Append(hoverText);
                        if (enableShadow)
                        {
                            IClickableMenu.drawHoverText(e.SpriteBatch, sb, Game1.smallFont);
                        }
                        else
                        {
                            drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                        }
                    }
                    else if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.hoverText.Length > 0)
                    {
                        StringBuilder sb = new();
                        sb.Append(gameMenu.hoverText);
                        drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                    }
                }
            }
            if (TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects is not null)
            {
                if (TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects is true)
                {
                    Game1.activeClickableMenu = null;
                    Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)434"), null);
                    TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects = null;
                }
                else
                {
                    Game1.player.maxStamina.Value -= 34;
                    if (Game1.player.Stamina > Game1.player.maxStamina.Value)
                    {
                        Game1.player.Stamina = Game1.player.maxStamina.Value;
                    }
                    TalentCore.TalentCoreEntry.Value.GiveOrTakeStardropEffects = null;
                }
            }
            if (Game1.activeClickableMenu is BobberBar bobberBar)
            {
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_TakeABreak))
                {
                    bobberBar.distanceFromCatchPenaltyModifier = !bobberBar.treasureCaught && bobberBar.treasurePosition != 0 && bobberBar.treasurePosition + 12f <= bobberBar.bobberBarPos - 32f + bobberBar.bobberBarHeight && bobberBar.treasurePosition - 16f >= bobberBar.bobberBarPos - 32f
                        ? 0f
                        : 1f;
                }
            }
            if (Game1.activeClickableMenu is PondQueryMenu)
            {
                if (GiveFrogEggBack?.visible is true)
                {
                    GiveFrogEggBack?.draw(e.SpriteBatch);
                }
                Game1.activeClickableMenu.drawMouse(e.SpriteBatch);
            }
        }
        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            var menu = ModEntry.GetGameMenuPage(Game1.activeClickableMenu);
            if (menu is NewSkillsPage or SkillsPage && CoreUtility.IsOverlayValid())
                HandleSkillPage(menu, Game1.activeClickableMenu);
            if (ModEntry.GetGameMenuPage(Game1.activeClickableMenu) is SkillsPage page && CoreUtility.IsOverlayValid())
                HandleSkillPage(page, Game1.activeClickableMenu);
        }
        public void HandleSkillPage(IClickableMenu page, IClickableMenu menu)
        {
            ShouldHandleSkillPage = false;
            if (page is NewSkillsPage newSkillsPage)
            {
                MyCustomSkillBars = newSkillsPage.skillBars.ToArray();
                NewSkillsPage skillsPage2 = new(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width + ((LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.it) ? 64 : 0), menu.height);
                VanillaSkillBars = skillsPage2.skillBars.ToArray();

                List<(int, int)> IndexAndProfessions = new();
                List<int> AlreadyPickedProfessions = new();

                for (int i = 0; i < MyCustomSkillBars.Length; i++)
                {
                    foreach (var item in ModEntry.Professions)
                    {
                        if (IsInCorrectLine(MyCustomSkillBars[i].bounds, newSkillsPage.skillAreas, item.Value.Skill.ToString()))
                        {
                            if (CoreUtility.CurrentPlayerHasProfession(item.Key, ignoreMode: true) && !AlreadyPickedProfessions.Contains(item.Value.ID))
                            {
                                IndexAndProfessions.Add((i, item.Value.ID));
                                AlreadyPickedProfessions.Add(item.Value.ID);
                                break;
                            }
                            else
                            {
                                MyCustomSkillBars[i].name = "-1";
                            }
                        }
                    }
                }

                for (int i = 0; i < IndexAndProfessions.Count; i++)
                {
                    int index = i;

                    MyCustomSkillBars[IndexAndProfessions[index].Item1].name = IndexAndProfessions[index].Item2.ToString();

                    List<string> description = LevelUpMenu.getProfessionDescription(IndexAndProfessions[index].Item2);
                    description.RemoveAt(0);

                    MyCustomSkillBars[IndexAndProfessions[index].Item1].hoverText = Game1.parseText(description.Join(delimiter: "\n"), Game1.smallFont, (int)Game1.dialogueFont.MeasureString(LevelUpMenu.getProfessionTitleFromNumber(IndexAndProfessions[index].Item2)).X + 100);
                }
            }
            LittlePlus = new(menu.upperRightCloseButton.bounds, Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f, false);
            LittlePlus.bounds.Y += 100;
            LittlePlus.bounds.X += 16;
            LittlePlus.visible = true;
            LittlePlus.myID = 46780;
            if (page.allClickableComponents is null)
            {
                page.populateClickableComponentList();
            }
            page.allClickableComponents.Add(LittlePlus);
            IsOverlayActive = false;
        }
        private void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, string boldTitleText)
        {
            //I know, I know. You hate me, its because I had a double shadow.
            if (text == null || text.Length == 0)
                return;

            if (boldTitleText != null && boldTitleText.Length == 0)
                boldTitleText = null;

            int width = Math.Max((int)font.MeasureString(text).X, boldTitleText is not null ? ((int)Game1.dialogueFont.MeasureString(boldTitleText).X) : 0) + 32;
            int height = Math.Max(60, (int)font.MeasureString(text).Y + 40 + (int)(boldTitleText is not null ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f));
            int x = Game1.getOldMouseX() + 32;
            int y = Game1.getOldMouseY() + 32;
            if (x + width > Utility.getSafeArea().Right)
            {
                x = Utility.getSafeArea().Right - width;
                y += 16;
            }
            if (y + height > Utility.getSafeArea().Bottom)
            {
                x += 16;
                if (x + width > Utility.getSafeArea().Right)
                    x = Utility.getSafeArea().Right - width;
                y = Utility.getSafeArea().Bottom - height;
            }
            width += 4;
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new(0, 256, 60, 60), x, y, width, height, Color.White, 1f, drawShadow: false);
            if (boldTitleText != null)
            {
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new(0, 256, 60, 60), x, y, width + (false ? 21 : 0), (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)(false ? font.MeasureString("asd").Y : 0f) - 4, Color.White, 1f, drawShadow: false);
                b.Draw(Game1.menuTexture, new Rectangle(x + 12, y + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((false && false) ? font.MeasureString("asd").Y : 0f) - 4, width - 4 * ((false) ? 1 : 6), 4), new Rectangle(44, 300, 4, 4), Color.White);
                b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor);
                b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
                y += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y;
            }

            y += (boldTitleText != null) ? 16 : 0;
            if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' '))
            {
                if (text.ToString().Contains("[line]"))
                {
                    string[] textSplit;
                    textSplit = text.ToString().Split("[line]");
                    b.DrawString(font, textSplit[0], new Vector2(x + 16, y + 20) + new Vector2(2f, 2f), Game1.textShadowColor);
                    b.DrawString(font, textSplit[0], new Vector2(x + 16, y + 20) + new Vector2(0f, 2f), Game1.textShadowColor);
                    b.DrawString(font, textSplit[0], new Vector2(x + 16, y + 20) + new Vector2(2f, 0f), Game1.textShadowColor);
                    b.DrawString(font, textSplit[0], new Vector2(x + 16, y + 20), Game1.textColor * 0.9f);
                    y += (int)font.MeasureString(textSplit[0]).Y - 16;
                    Utility.drawLineWithScreenCoordinates(x + 12, y + 20, x + 16 + width - 28, y + 20, b, Game1.textShadowColor);
                    Utility.drawLineWithScreenCoordinates(x + 12, y + 20, x + 16 + width - 28, y + 21, b, Game1.textShadowColor);
                    if (textSplit.Length > 1)
                    {
                        y -= 16;
                        b.DrawString(font, textSplit[1], new Vector2(x + 16, y + 20) + new Vector2(2f, 2f), Game1.textShadowColor);
                        b.DrawString(font, textSplit[1], new Vector2(x + 16, y + 20) + new Vector2(0f, 2f), Game1.textShadowColor);
                        b.DrawString(font, textSplit[1], new Vector2(x + 16, y + 20) + new Vector2(2f, 0f), Game1.textShadowColor);
                        b.DrawString(font, textSplit[1], new Vector2(x + 16, y + 20), Game1.textColor * 0.9f);
                    }
                }
                else
                {
                    b.DrawString(font, text, new Vector2(x + 16, y + 20) + new Vector2(2f, 2f), Game1.textShadowColor);
                    b.DrawString(font, text, new Vector2(x + 16, y + 20) + new Vector2(0f, 2f), Game1.textShadowColor);
                    b.DrawString(font, text, new Vector2(x + 16, y + 20) + new Vector2(2f, 0f), Game1.textShadowColor);
                    b.DrawString(font, text, new Vector2(x + 16, y + 20), Game1.textColor * 0.9f);
                }
            }
        }
        public void HandleLevelUpMenu(IClickableMenu levelUpMenu)
        {
            if (levelUpMenu is LevelUpMenu or SkillLevelUpMenu)
            {
                int currentLevel = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<int>(levelUpMenu, "currentLevel").GetValue();
                int currentskill_int = -1;
                string currentskill_string = null;

                if (levelUpMenu is LevelUpMenu)
                {
                    currentskill_int = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<int>(levelUpMenu, "currentSkill").GetValue();
                    if (currentLevel is 15 or 20)
                    {
                        (levelUpMenu as LevelUpMenu).isProfessionChooser = true;
                    }
                }
                else
                {
                    currentskill_string = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<string>(levelUpMenu, "currentSkill").GetValue();
                    if (currentLevel is 15 or 20)
                    {
                        (levelUpMenu as SkillLevelUpMenu).isProfessionChooser = true;
                    }
                }
                List<int> _professionsToChoose = new();
                if (currentLevel is 15)
                {
                    foreach (var item in ModEntry.Professions)
                    {
                        if (Game1.player.professions.Contains(item.Value.Requires) && AreSkillConditionsMet(currentskill_string, currentskill_int) == item.Value.Skill.ToString())
                            _professionsToChoose.Add(item.Value.ID);
                    }
                    ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
                }
                else if (currentLevel is 20)
                {
                    foreach (var item in ModEntry.Professions)
                    {
                        if (currentLevel == item.Value.LevelRequirement && AreSkillConditionsMet(currentskill_string, currentskill_int) == item.Value.Skill.ToString())
                            _professionsToChoose.Add(item.Value.ID);
                    }
                    ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
                }
               
                if (_professionsToChoose.Count > 0 && levelUpMenu is LevelUpMenu lvlupMenu)
                {
                    levelUpMenu.gameWindowSizeChanged(new(), new());
                    lvlupMenu.leftProfession = new ClickableComponent(new Rectangle(lvlupMenu.xPositionOnScreen, lvlupMenu.yPositionOnScreen + 128, lvlupMenu.width / 2, lvlupMenu.height), "")
                    {
                        myID = 102,
                        rightNeighborID = 103
                    };
                    lvlupMenu.rightProfession = new ClickableComponent(new Rectangle(lvlupMenu.width / 2 + lvlupMenu.xPositionOnScreen, lvlupMenu.yPositionOnScreen + 128, lvlupMenu.width / 2, lvlupMenu.height), "")
                    {
                        myID = 103,
                        leftNeighborID = 102
                    };
                    levelUpMenu.allClickableComponents.Clear();
                    levelUpMenu.populateClickableComponentList();
                }
            }
        }
        public string AreSkillConditionsMet(string str, int integer) => integer is -1 ? str : str is null ? integer.ToString() : null;

        public bool IsInCorrectLine(Rectangle IconBounds, List<ClickableTextureComponent> skillAreas, string skill)
        {
            foreach (var area in skillAreas)
            {
                if (IconBounds.Y == area.bounds.Y && area.name == skill)
                {
                    return true;
                }
            }
            return false;
        }

        public int StandardizeSkillIndexes(int FF)
        {
            return FF switch
            {
                1 => 3,
                3 => 1,
                _ => FF
            };
        }
    }
}