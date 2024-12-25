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

namespace VanillaPlusProfessions
{
    internal class DisplayHandler
    {
        internal static PerScreen<ClickableTextureComponent[]> MyCustomSkillBars = new();
        internal static PerScreen<ClickableTextureComponent[]> VanillaSkillBars = new();
        internal static PerScreen<ClickableTextureComponent> LittleArrow = new();
        internal static PerScreen<ClickableTextureComponent> GiveFrogEggBack = new();
        internal static readonly PerScreen<bool> IsOverlayActive = new();
        internal static readonly PerScreen<bool> WasSkillMenuRaised = new();
        internal static readonly PerScreen<bool> ShouldHandleSkillPage = new();
        internal static Texture2D SkillIcons;
        internal static Texture2D ProfessionIcons;

        internal static readonly PerScreen<int> lossAmount = new();

        internal static void Initialize()
        {
            SkillIcons = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["SkillBars"]);
            ProfessionIcons = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ProfessionIcons"]);

            ModEntry.Helper.Events.Display.MenuChanged += OnMenuChanged;
            ModEntry.Helper.Events.Display.WindowResized += OnWindowResized;
            ModEntry.Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            ModEntry.Helper.Events.Display.RenderedStep += OnRenderedStep;
            ModEntry.Helper.Events.Display.RenderedHud += OnRenderedHud;
            ModEntry.Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }
        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is PondQueryMenu pondMenu)
            {
                if (GiveFrogEggBack?.Value?.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)) is true)
                {
                    string s = (AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(pondMenu) as FishPond).modData[TalentCore.Key_HiddenBenefit_FrogEggs];
                    Game1.player.addItemByMenuIfNecessary(s.StringToTrinket()); //trinket here
                    (AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(pondMenu) as FishPond).modData[TalentCore.Key_HiddenBenefit_FrogEggs] = "";
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
        private static void OnRenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (e.Step is StardewValley.Mods.RenderSteps.World_Sorted)
            {
                foreach (var item in Game1.currentLocation.Objects.Pairs)
                {
                    if (item.Value.IsSprinkler() && item.Value.heldObject.Value is not null && item.Value.heldObject.Value.QualifiedItemId is "(O)Kedi.VPP.PressureNozzleEnricher")
                    {
                        var data = ItemRegistry.GetData(item.Value.heldObject.Value.ItemId);
                        var rect = data.GetSourceRect();
                        e.SpriteBatch.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(item.Key.X * 64f, item.Key.Y * 64f - 8f)), rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, item.Key.Y - 10000000 / 8700000f);
                    }
                    else if (item.Value.IsTapper() && Game1.currentLocation.terrainFeatures.TryGetValue(item.Key, out TerrainFeature terrainFeature) && terrainFeature is FruitTree or GiantCrop)
                    {
                        if (terrainFeature.modData.TryGetValue(ModEntry.Key_TFHasTapper, out string value) && value.ToLower() is "true")
                        {
                            var data = ItemRegistry.GetData(terrainFeature.modData[ModEntry.Key_TFTapperID]);
                            var rect = data.GetSourceRect();
                            rect.Height /= 2;
                            e.SpriteBatch.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(terrainFeature.Tile.X * 64f, terrainFeature.Tile.Y * 64f - 64f)), rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (terrainFeature.getBoundingBox().Bottom + 10000) / 87000f - terrainFeature.Tile.X / 1000000f);
                        }
                    }
                }
            }
        }
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is LevelUpMenu or SkillLevelUpMenu)
            {
                if (!WasSkillMenuRaised.Value)
                    ModEntry.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
                HandleLevelUpMenu(e.NewMenu);
            }
            else if (Game1.player.newLevels.Count == 0 && TalentCore.IsDayStartOrEnd)
            {
                WasSkillMenuRaised.Value = true;
                ModEntry.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
            }

            if (e.NewMenu is GameMenu menu1 && menu1.pages[1] is SkillsPage or NewSkillsPage && CoreUtility.IsOverlayValid() && ShouldHandleSkillPage.Value)
                HandleSkillPage(menu1.pages[1], menu1);

            if (e.NewMenu is GeodeMenu)
            {
                ModEntry.UpdateGeodeInMenu ??= new(() => false);
                ModEntry.UpdateGeodeInMenu.Value = true;
            }
            if (e.OldMenu is BobberBar bobberBar)
            {
                if (TalentUtility.CurrentPlayerHasTalent("Fishing_Here_Fishy_Fishy"))
                {
                    if (bobberBar.distanceFromCatching is 0f)
                        lossAmount.Value++;
                    else
                        lossAmount.Value = 0;
                }
            }
            else if (e.NewMenu is BobberBar bobberBar1)
            {
                if (TalentUtility.CurrentPlayerHasTalent("Fishing_Here_Fishy_Fishy"))
                {
                    if (lossAmount.Value > 1)
                    {
                        bobberBar1.bobberBarHeight += lossAmount.Value * 20;
                        bobberBar1.bobberBarPos -= lossAmount.Value * 20;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("Fishing_One_Fish_Two_Fish"))
                {
                    if (bobberBar1.challengeBaitFishes > -1)
                    {
                        bobberBar1.challengeBaitFishes = 4;
                    }
                }
            }

            if (e.OldMenu is GameMenu || e.NewMenu is GameMenu)
                IsOverlayActive.Value = false;

            if (e.OldMenu is CraftingPage craftingPage && craftingPage.cooking && e.NewMenu is null)
                TalentCore.IsCookoutKit.Value = false;

            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu is null)
                    return;
                if (menu.context is Chest chest && TalentUtility.AnyPlayerHasTalent("MiniFridgeBigSpace"))
                {
                    if (chest?.QualifiedItemId == "(BC)216")
                    {
                        menu.source = 1;
                        menu.setSourceItem(chest);

                    }
                }
                if (menu.context is FishingRod && CoreUtility.CurrentPlayerHasProfession("Buccaneer") is true)
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
                            
                            if (strings[0] is "Artifact" && LibraryMuseum.HasDonatedArtifact(menu.ItemsToGrabMenu.actualInventory[i].QualifiedItemId))
                            {
                                if (strings[1].StartsWith("(TR)"))
                                {
                                    if (!Trinket.CanSpawnTrinket(Game1.player))
                                    {
                                        continue;
                                    }
                                }
                                menu.ItemsToGrabMenu.inventory[i].item = item;
                                menu.ItemsToGrabMenu.actualInventory[i] = item;
                                continue;
                            }

                            menu.ItemsToGrabMenu.actualInventory[i] = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                            menu.ItemsToGrabMenu.inventory[i].item = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                        }
                    }
                }
            }
            if (e.NewMenu is PondQueryMenu querye)
            {
                GiveFrogEggBack.Value = (AccessTools.Field(typeof(PondQueryMenu), "_pond").GetValue(querye) as FishPond).modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out string value)
                    ? new( new(querye.xPositionOnScreen, querye.yPositionOnScreen, 64, 64), SkillIcons, new(0, 27, 16, 16), 4f, false)
                    : null;
            }

            MachineryEventHandler.OnMenuChanged(e);
        }
        private static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu is null && CoreUtility.CurrentPlayerHasProfession("Forage-Combat"))
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
            if (Game1.player.CurrentTool is not null and Slingshot slingshot && TalentCore.IsActionButtonUsed.Value)
            {
                slingshot.draw(e.SpriteBatch);
            }
        }
        private static void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                if (menu.pages[menu.currentTab] is SkillsPage page)
                {
                    if (IsOverlayActive.Value)
                    {
                        string hoverText = ModEntry.Helper.Reflection.GetField<string>(page, "hoverText").GetValue();
                        string hoverTitle = ModEntry.Helper.Reflection.GetField<string>(page, "hoverTitle").GetValue();
                        for (int FF = 0; FF < 5; FF++)
                        {
                            for (int SS = 0; SS < 10; SS++)
                            {
                                if (SS is 4 or 9)
                                {
                                    if (Game1.player.GetSkillLevel(StandardizeSkillIndexes(FF)) < (11 + SS))
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS is 9 ? 24 : 0), page.skillAreas[FF].bounds.Y), new Rectangle(16, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                }
                                else
                                {
                                    if (Game1.player.GetSkillLevel(StandardizeSkillIndexes(FF)) < (11 + SS))
                                        e.SpriteBatch.Draw(SkillIcons, new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS > 3 ? 24 : 0), page.skillAreas[FF].bounds.Y), new Rectangle(0, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                    else
                                    {
                                        e.SpriteBatch.Draw(SkillIcons,
                                        new Vector2(page.skillAreas[FF].bounds.X + page.skillAreas[FF].bounds.Width + 24 + (36 * SS) + (SS > 3 ? 24 : 0), page.skillAreas[FF].bounds.Y),
                                        new Rectangle(8, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
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
                        else if (menu.hoverText.Length > 0)
                        {
                            StringBuilder sb = new();
                            sb.Append(menu.hoverText);
                            drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                        }

                        foreach (ClickableTextureComponent c in MyCustomSkillBars.Value)
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
                        LittleArrow.Value.draw(e.SpriteBatch);
                        page.drawMouse(e.SpriteBatch);
                    }
                }
                else if (menu.pages[menu.currentTab] is NewSkillsPage page2)
                {
                    string hoverText = ModEntry.Helper.Reflection.GetField<string>(page2, "hoverText").GetValue();
                    string hoverTitle = ModEntry.Helper.Reflection.GetField<string>(page2, "hoverTitle").GetValue();
                    Dictionary<int, int> skillAreaSkillIndexes = ModEntry.Helper.Reflection.GetField<Dictionary<int, int>>(page2, "skillAreaSkillIndexes").GetValue();
                    int skillScrollOffset = ModEntry.Helper.Reflection.GetField<int>(page2, "skillScrollOffset").GetValue();
                    int LastVisibleSkillIndex = ModEntry.Helper.Reflection.GetProperty<int>(page2, "LastVisibleSkillIndex").GetValue();
                    if (IsOverlayActive.Value)
                    {
                        foreach (var item in page2.skillAreas)
                        {
                            if (skillAreaSkillIndexes.TryGetValue(item.myID, out int skillIndex) && (skillIndex < skillScrollOffset || skillIndex > LastVisibleSkillIndex))
                                continue;
                            if (item.name.StartsWith("C"))
                            {
                                int level = ModEntry.SpaceCoreAPI.Value.GetLevelForCustomSkill(Game1.player, item.name) - ModEntry.SpaceCoreAPI.Value.GetBuffLevelForCustomSkill(Game1.player, item.name) - 10;
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i is 4 or 9)
                                    {
                                        if (level < (1 + i))
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i is 9 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(16, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);

                                    }
                                    else
                                    {
                                        if (level < (1 + i))
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(0, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        else
                                        {
                                            e.SpriteBatch.Draw(SkillIcons,
                                            new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)),
                                            new Rectangle(8, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int level = Game1.player.GetUnmodifiedSkillLevel(int.Parse(item.name));
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i is 4 or 9)
                                    {
                                        if (level < (11 + i))
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i is 9 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(16, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        else
                                        {
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i is 9 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(30, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 13, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                        }
                                    }
                                    else
                                    {
                                       if (level < (11 + i))
                                            e.SpriteBatch.Draw(SkillIcons, new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)), new Rectangle(0, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                       else
                                       {
                                           e.SpriteBatch.Draw(SkillIcons,
                                           new Vector2(item.bounds.X + item.bounds.Width + 24 + (36 * i) + (i > 3 ? 24 : 0), item.bounds.Y - (skillScrollOffset * 56)),
                                           new Rectangle(8, ModEntry.ModConfig.Value.ColorBlindnessChanges ? 9 : 0, 7, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
                                       }
                                    }
                                }
                            }
                        }
                        if (MyCustomSkillBars.Value is not null && MyCustomSkillBars.Value.Length > 0)
                        {
                            foreach (ClickableTextureComponent c in MyCustomSkillBars.Value)
                            {
                                if (c.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true) + (skillScrollOffset * 56)) && c.hoverText.Length > 0 && !c.name.Equals("-1"))
                                {
                                    IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), c.bounds.X - 24, c.bounds.Y - 32 - (skillScrollOffset * 56), 96, 96, Color.White, drawShadow: false);
                                    e.SpriteBatch.Draw(ProfessionIcons, new Vector2(c.bounds.X - 8, c.bounds.Y - 16 - (skillScrollOffset * 56)), new Rectangle((Convert.ToInt32(c.name) - 467830) % 6 * 16, (Convert.ToInt32(c.name) - 467830) / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                                }
                            }
                        }
                    }

                    if (CoreUtility.IsOverlayValid())
                    {
                        LittleArrow.Value.draw(e.SpriteBatch);
                        page2.drawMouse(e.SpriteBatch);
                    }
                    if (hoverText.Length > 0)
                    {
                        StringBuilder sb = new();
                        sb.Append(hoverText);
                        drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                    }
                    else if (menu.hoverText.Length > 0)
                    {
                        StringBuilder sb = new();
                        sb.Append(menu.hoverText);
                        drawHoverText(e.SpriteBatch, sb, Game1.smallFont, hoverTitle);
                    }
                }
            }
            if (Game1.activeClickableMenu is BobberBar bobberBar)
            {
                if (TalentUtility.CurrentPlayerHasTalent("Fishing_Take_A_Break"))
                {
                    bobberBar.distanceFromCatchPenaltyModifier = !bobberBar.treasureCaught && bobberBar.treasurePosition + 12f <= bobberBar.bobberBarPos - 32f + bobberBar.bobberBarHeight && bobberBar.treasurePosition - 16f >= bobberBar.bobberBarPos - 32f
                        ? 0f
                        : 1f;
                }
            }
            if (Game1.activeClickableMenu is PondQueryMenu)
            {
                GiveFrogEggBack.Value?.draw(e.SpriteBatch);
                Game1.activeClickableMenu.drawMouse(e.SpriteBatch);
            }
        }
        private static void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu1 && menu1.pages[1] is SkillsPage page && CoreUtility.IsOverlayValid())
                HandleSkillPage(page, menu1);
        }
        public static void HandleSkillPage(IClickableMenu page, GameMenu menu)
        {
            ShouldHandleSkillPage.Value = false;
            
            if (page is NewSkillsPage newSkillsPage && Game1.activeClickableMenu is GameMenu menu2)
            {
                MyCustomSkillBars.Value = newSkillsPage.skillBars.ToArray();
                NewSkillsPage skillsPage2 = new(menu2.xPositionOnScreen, menu2.yPositionOnScreen, menu2.width + ((LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.it) ? 64 : 0), menu2.height);
                VanillaSkillBars.Value = skillsPage2.skillBars.ToArray();

                List<(int, int)> IndexAndProfessions = new();
                List<int> AlreadyPickedProfessions = new();


                for (int i = 0; i < MyCustomSkillBars.Value.Length; i++)
                {
                    foreach (var item in ModEntry.Professions)
                    {
                        if (IsInCorrectLine(MyCustomSkillBars.Value[i].bounds, newSkillsPage.skillAreas, item.Value.Skill.ToString()))
                        {
                            if (CoreUtility.CurrentPlayerHasProfession(item.Key, ignoreMode: true) && !AlreadyPickedProfessions.Contains(item.Value.ID))
                            {
                                IndexAndProfessions.Add((i, item.Value.ID));
                                AlreadyPickedProfessions.Add(item.Value.ID);
                                break;
                            }
                            else
                            {
                                MyCustomSkillBars.Value[i].name = "-1";
                            }
                        }
                    }
                }

                for (int i = 0; i < IndexAndProfessions.Count; i++)
                {
                    int index = i;

                    MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].name = IndexAndProfessions[index].Item2.ToString();

                    List<string> description = LevelUpMenu.getProfessionDescription(IndexAndProfessions[index].Item2);
                    description.RemoveAt(0);

                    MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].hoverText = Game1.parseText(description.Join(delimiter: "\n"), Game1.smallFont, (int)Game1.dialogueFont.MeasureString(LevelUpMenu.getProfessionTitleFromNumber(IndexAndProfessions[index].Item2)).X + 100);
                }
            }
            LittleArrow.Value = new(menu.upperRightCloseButton.bounds, Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f, false);
            LittleArrow.Value.bounds.Y += 100;
            LittleArrow.Value.bounds.X += 16;
            LittleArrow.Value.visible = true;
            IsOverlayActive.Value = false;
        }
        private static void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, string boldTitleText)
        {
            //I know, I know. You want to incinerate me for this and you're right to do so, but its because I had a double shadow.
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
        public static void HandleLevelUpMenu(IClickableMenu levelUpMenu)
        {
            if (levelUpMenu is LevelUpMenu or SkillLevelUpMenu)
            {
                int currentLevel = ModEntry.Helper.Reflection.GetField<int>(levelUpMenu, "currentLevel").GetValue();
                int currentskill_int = -1;
                string currentskill_string = null;

                if (levelUpMenu is LevelUpMenu)
                {
                    currentskill_int = ModEntry.Helper.Reflection.GetField<int>(levelUpMenu, "currentSkill").GetValue();
                    if (currentLevel is 15 or 20)
                    {
                        (levelUpMenu as LevelUpMenu).isProfessionChooser = true;
                    }
                }
                else
                {
                    currentskill_string = ModEntry.Helper.Reflection.GetField<string>(levelUpMenu, "currentSkill").GetValue();
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
                    ModEntry.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
                }
                else if (currentLevel is 20)
                {
                    foreach (var item in ModEntry.Professions)
                    {
                        if (currentLevel == item.Value.LevelRequirement && AreSkillConditionsMet(currentskill_string, currentskill_int) == item.Value.Skill.ToString())
                            _professionsToChoose.Add(item.Value.ID);
                    }
                    ModEntry.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
                }
                /*else if (currentLevel < 10)
                {
                    List<CraftingRecipe> newCraftingRecipes = ModEntry.Helper.Reflection.GetField<List<CraftingRecipe>>(levelUpMenu, "newCraftingRecipes").GetValue();
                    var extraInfoForLevel = ModEntry.Helper.Reflection.GetField<List<string>>(levelUpMenu, "extraInfoForLevel").GetValue();
                    int newHeight = 0;
                    for (int i = 0; i < newCraftingRecipes.Count; i++)
                    {
                        Game1.player.craftingRecipes.Remove(newCraftingRecipes[i].name);
                        Game1.player.cookingRecipes.Remove(newCraftingRecipes[i].name);
                    }
                    newCraftingRecipes.Clear();
                    foreach (var v in CraftingRecipe.craftingRecipes)
                    {
                        string[] conditions = ArgUtility.Get(v.Value.Split('/'), 4, "").Split(" ");
                        if (conditions[0] == (currentskill_string ?? Farmer.getSkillNameFromIndex(currentskill_int)) && conditions[1] == (currentLevel.ToString() ?? ""))
                        {
                            CraftingRecipe recipe = new CraftingRecipe(v.Key, isCookingRecipe: false);
                            newCraftingRecipes.Add(recipe);
                            Game1.player.craftingRecipes.TryAdd(v.Key, 0);
                            newHeight += (recipe.bigCraftable ? 128 : 64);
                        }
                    }
                    foreach (var v in CraftingRecipe.cookingRecipes)
                    {
                        string[] conditions = ArgUtility.Get(v.Value.Split('/'), 3, "").Split(" ");
                        if (conditions[0] == (currentskill_string ?? Farmer.getSkillNameFromIndex(currentskill_int)) && conditions[1] == (currentLevel.ToString() ?? ""))
                        {
                            CraftingRecipe recipe = new CraftingRecipe(v.Key, isCookingRecipe: true);
                            newCraftingRecipes.Add(recipe);
                            Game1.player.cookingRecipes.TryAdd(v.Key, 0);
                            newHeight += (recipe.bigCraftable ? 128 : 64);
                        }
                    }
                    int val = newHeight + 256 + extraInfoForLevel.Count * 64 * 3 / 4;
                    ModEntry.Helper.Reflection.GetField<int>(levelUpMenu, "height").SetValue(val);
                    ModEntry.Helper.Reflection.GetField<List<CraftingRecipe>>(levelUpMenu, "newCraftingRecipes").SetValue(newCraftingRecipes);
                    levelUpMenu.gameWindowSizeChanged(new(), new());
                }*/
            }
        }
        public static string AreSkillConditionsMet(string str, int integer) => integer is -1 ? str : str is null ? integer.ToString() : null;

        public static bool IsInCorrectLine(Rectangle IconBounds, List<ClickableTextureComponent> skillAreas, string skill)
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

        public static int StandardizeSkillIndexes(int FF)
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
