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

namespace VanillaPlusProfessions
{
    internal class DisplayHandler
    {
        internal static PerScreen<ClickableTextureComponent[]> MyCustomSkillBars = new();
        internal static PerScreen<ClickableTextureComponent[]> VanillaSkillBars = new();
        internal static PerScreen<ClickableTextureComponent> LittleArrow = new();
        internal static readonly PerScreen<bool> IsOverlayActive = new();
        internal static readonly PerScreen<bool> WasSkillMenuRaised = new();
        internal static readonly PerScreen<bool> ShouldHandleSkillPage = new();
        internal static Texture2D SkillIcons;
        internal static Texture2D ProfessionIcons;

        internal static void Initialize()
        {
            SkillIcons = ModEntry.Helper.ModContent.Load<Texture2D>("assets/skillbars.png");
            ProfessionIcons = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\ProfessionIcons.png");

            ModEntry.Helper.Events.Display.MenuChanged += OnMenuChanged;
            ModEntry.Helper.Events.Display.WindowResized += OnWindowResized;
            ModEntry.Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            //ModEntry.Helper.Events.Display.RenderedWorld += OnRenderedWorld;
            ModEntry.Helper.Events.Display.RenderedHud += OnRenderedHud;
            ModEntry.Helper.Events.Input.ButtonPressed += OnButtonPressed;

        }
        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {            
            if (Game1.activeClickableMenu is GameMenu menu && menu.pages[menu.currentTab] is SkillsPage page)
            {
                if ( (e.Button.IsUseToolButton() || e.Button.IsActionButton()))
                {
                    if (CoreUtility.IsOverlayValid() && LittleArrow.Value.containsPoint((int)e.Cursor.GetScaledScreenPixels().X, (int)e.Cursor.GetScaledScreenPixels().Y))
                    {
                        IsOverlayActive.Value = !IsOverlayActive.Value;
                        page.skillBars = IsOverlayActive.Value ? MyCustomSkillBars.Value.ToList() : VanillaSkillBars.Value.ToList();
                    }
                    else 
                    {
                        for (int i = 0; i < page.skillAreas.Count; i++)
                        {
                            if (page.skillAreas[i].containsPoint((int)e.Cursor.GetScaledScreenPixels().X, (int)e.Cursor.GetScaledScreenPixels().Y))
                            {
                                //Do not standardize the skill index!!
                                Game1.activeClickableMenu = new TalentSelectionMenu(i, menu);
                            }
                        }
                    }
                }
            }
            if (Game1.activeClickableMenu is ForgeMenu forgeMenu && forgeMenu.startTailoringButton.containsPoint((int)e.Cursor.GetScaledScreenPixels().X, (int)e.Cursor.GetScaledScreenPixels().Y) && e.Button.IsUseToolButton() && true)
            {
                if (forgeMenu.rightIngredientSpot.item is not null and StardewValley.Object obj && obj.QualifiedItemId == "(O)82" && obj.Stack >= 4)
                {
                    obj.Stack -= 4;
                    if (obj.Stack <= 0)
                        forgeMenu.rightIngredientSpot.item = null;
                }
            }
        }
        private static void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            
        }
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is LevelUpMenu Menu1)
            {
                if (!WasSkillMenuRaised.Value)
                    ModEntry.Helper.GameContent.InvalidateCache("LooseSprites/Cursors");
                WasSkillMenuRaised.Value = true;
                HandleLevelUpMenu(Menu1);
            }

            if (e.NewMenu is GameMenu menu1 && menu1.pages[1] is SkillsPage page && CoreUtility.IsOverlayValid() && ShouldHandleSkillPage.Value)
                HandleSkillPage(page, menu1);

            if (Game1.activeClickableMenu is GameMenu menu23 && e.OldMenu is TalentSelectionMenu talentTree)
                Game1.activeClickableMenu = talentTree.menu;

            if (e.OldMenu is GameMenu)
                IsOverlayActive.Value = false;
            
            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu is null)
                    return;
                if (menu.context is FishingRod && CoreUtility.CurrentPlayerHasProfession(41) is true)
                {
                    for (int i = 0; i < menu.ItemsToGrabMenu.actualInventory.Count; i++)
                    {
                        if (menu.ItemsToGrabMenu.actualInventory[i] is null)
                            continue;

                        if (ContentEditor.BuccaneerData.TryGetValue(menu.ItemsToGrabMenu.actualInventory[i].QualifiedItemId, out string value))
                        {
                            IEnumerable<LibraryMuseum> loc =
                                from locat in Game1.locations
                                where locat is LibraryMuseum
                                select locat as LibraryMuseum;

                            if ((value == "Artifact" && !loc.First().isItemSuitableForDonation(menu.ItemsToGrabMenu.actualInventory[i])) || value is null)
                            {
                                menu.ItemsToGrabMenu.inventory[i].item = null;
                                menu.ItemsToGrabMenu.actualInventory[i] = null;
                                continue;
                            }

                            menu.ItemsToGrabMenu.actualInventory[i] = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                            menu.ItemsToGrabMenu.inventory[i].item = ItemRegistry.Create(value, menu.ItemsToGrabMenu.actualInventory[i].Stack, menu.ItemsToGrabMenu.actualInventory[i].Quality);
                        }
                    }
                }
            }
        }
        private static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu is null && CoreUtility.CurrentPlayerHasProfession(75))
            {
                foreach (NPC @char in Game1.player.currentLocation.characters)
                {
                    if (@char is Monster monster && !Utility.isOnScreen(monster.Tile * 64f + new Vector2(32f, 32f), 64))
                    {
                        Rectangle bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
                        Vector2 onScreenPos = default;
                        float rotation = 0f;
                        if (monster.Tile.X * 64f > (Game1.viewport.MaxCorner.X - 64))
                        {
                            onScreenPos.X = bounds.Right - 8;
                            rotation = (float)Math.PI / 2f;
                        }
                        else if (monster.Tile.X * 64f < Game1.viewport.X)
                        {
                            onScreenPos.X = 8f;
                            rotation = -(float)Math.PI / 2f;
                        }
                        else
                            onScreenPos.X = monster.Tile.X * 64f - Game1.viewport.X;
                        if (monster.Tile.Y * 64f > (Game1.viewport.MaxCorner.Y - 64))
                        {
                            onScreenPos.Y = bounds.Bottom - 8;
                            rotation = (float)Math.PI;
                        }
                        else onScreenPos.Y = monster.Tile.Y * 64f < Game1.viewport.Y ? 8f : monster.Tile.Y * 64f - Game1.viewport.Y;

                        if (onScreenPos.X == 8f)
                        {
                            if (onScreenPos.Y == 8f)
                                rotation += (float)Math.PI / 4f;

                            if (onScreenPos.Y == bounds.Bottom - 8)
                                rotation += (float)Math.PI / 4f;
                        }
                        if (onScreenPos.X == bounds.Right - 8)
                        {
                            if (onScreenPos.Y == 8f)
                                rotation -= (float)Math.PI / 4f;

                            if (onScreenPos.Y == bounds.Bottom - 8)
                                rotation -= (float)Math.PI / 4f;
                        }
                        Vector2 safePos = Utility.makeSafe(onScreenPos, new Vector2(20f, 16f));
                        e.SpriteBatch.Draw(Game1.mouseCursors, safePos, new(412, 495, 5, 4), Color.Red, rotation, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
                    }
                }
            }
        }
        private static void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu && menu.pages[menu.currentTab] is SkillsPage page)
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

                    foreach (ClickableTextureComponent c in MyCustomSkillBars.Value)
                    {
                        if (c.scale == 0f)
                        {
                            IClickableMenu.drawTextureBox(e.SpriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), c.bounds.X - 16 - 8, c.bounds.Y - 16 - 16, 96, 96, Color.White, drawShadow: false);
                            e.SpriteBatch.Draw(ProfessionIcons, new Vector2(c.bounds.X - 8, c.bounds.Y - 16), new Rectangle((Convert.ToInt32(c.name) - 30) % 6 * 16, (Convert.ToInt32(c.name) - 30) / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        }
                    }
                }
                if (CoreUtility.IsOverlayValid())
                {
                    LittleArrow.Value.draw(e.SpriteBatch);
                    page.drawMouse(e.SpriteBatch);
                }
                if (page.GetChildMenu() is not null)
                {
                    page.GetChildMenu().draw(e.SpriteBatch);
                }
            }
        }
        private static void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu1 && menu1.pages[1] is SkillsPage page && CoreUtility.IsOverlayValid())
                HandleSkillPage(page, menu1);
        }
        public static void HandleSkillPage(SkillsPage page, GameMenu menu)
        {
            ShouldHandleSkillPage.Value = false;
            MyCustomSkillBars.Value = page.skillBars.ToArray();
            SkillsPage skillsPage = new(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width + ((LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.it) ? 64 : 0), menu.height);
            VanillaSkillBars.Value = skillsPage.skillBars.ToArray();
            for (int x = 0; x < MyCustomSkillBars.Value.Length; x++)
            {
                for (int a = 0; a < page.skillAreas.Count; a++)
                {
                    if (page.skillAreas[a].bounds.Y == MyCustomSkillBars.Value[x].bounds.Y)
                    {
                        MyCustomSkillBars.Value[x].label = a.ToString();
                        break;
                    }
                    else if (x is 0)
                    {
                        continue;
                    }
                    if (MyCustomSkillBars.Value[x].bounds.Y == MyCustomSkillBars.Value[x - 1].bounds.Y)
                    {
                        MyCustomSkillBars.Value[x].label = MyCustomSkillBars.Value[x - 1].label;
                        break;
                    }
                }
            }
            for (int i = 0; i < MyCustomSkillBars.Value.Length; i++)
            {
                foreach (var item in ModEntry.Professions)
                {
                    MyCustomSkillBars.Value[i].texture = SkillIcons;
                    if (CoreUtility.CurrentPlayerHasProfession(item.Value.ID, ignoreMode: true) || Game1.player.newLevels.Contains(new(item.Value.Skill, item.Value.LevelRequirement)))
                    {
                        if (CoreUtility.CurrentPlayerHasProfession(item.Value.ID, ignoreMode: true) && IsPreProfessionsMet(item, ref MyCustomSkillBars.Value[i]))
                        {
                            MyCustomSkillBars.Value[i].name = item.Value.ID.ToString();
                            MyCustomSkillBars.Value[i].hoverText = ModEntry.Helper.Translation.Get("Profession." + item.Key + ".Desc").ToString().Replace("_", "\n").ReplaceLineEndings();
                        }
                        MyCustomSkillBars.Value[i].sourceRect = ModEntry.ModConfig.Value.ColorBlindnessChanges ? new(30, 9, 13, 9) : new(30, 0, 13, 9);
                        MyCustomSkillBars.Value[i].label = null;
                        if (Game1.player.newLevels.Contains(new(item.Value.Skill, item.Value.LevelRequirement)))
                            MyCustomSkillBars.Value[i].name = "-1";
                        break;
                    }
                }
            }
            for (int i = 0; i < MyCustomSkillBars.Value.Length; i++)
            {
                if (!CoreUtility.DoesDictHaveID(MyCustomSkillBars.Value[i].name, out var result))
                    MyCustomSkillBars.Value[i].name = "-1";

                else if (result.Value is not null && !(Game1.player.newLevels.Contains(new(result.Value.Skill, result.Value.LevelRequirement)) || CoreUtility.CurrentPlayerHasProfession(result.Value.ID, ignoreMode: true)))
                    MyCustomSkillBars.Value[i].sourceRect = ModEntry.ModConfig.Value.ColorBlindnessChanges ? new(16, 9, 13, 9) : new(16, 0, 13, 9);

                else                        
                    MyCustomSkillBars.Value[i].sourceRect = ModEntry.ModConfig.Value.ColorBlindnessChanges ? new(30, 9, 13, 9) : new(30, 0, 13, 9);
            }
            LittleArrow.Value = new(menu.upperRightCloseButton.bounds, Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f, false);
            LittleArrow.Value.bounds.Y += 100;
            LittleArrow.Value.bounds.X += 16;
            LittleArrow.Value.visible = true;
            IsOverlayActive.Value = false;

            ((Game1.activeClickableMenu as GameMenu).pages[1] as SkillsPage).skillBars = VanillaSkillBars.Value.ToList();
        }

        private static void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, string boldTitleText)
        {
            //I know, I know. You want to burn me entirely for this and you're right to do so, but its only because I had a double shadow.
            //I know. It's an idiotic reason to do this
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
        public static void HandleLevelUpMenu(LevelUpMenu levelUpMenu)
        {
            int currentLevel = ModEntry.Helper.Reflection.GetField<int>(levelUpMenu, "currentLevel").GetValue();
            int currentSkill = ModEntry.Helper.Reflection.GetField<int>(levelUpMenu, "currentSkill").GetValue();
            List<int> _professionsToChoose = new();
            if (currentLevel is 15)
            {
                levelUpMenu.isProfessionChooser = true;
                foreach (var item in ModEntry.Professions)
                {
                    if (Game1.player.professions.Contains(item.Value.Requires) && currentSkill == item.Value.Skill)
                        _professionsToChoose.Add(item.Value.ID);
                    
                }
                ModEntry.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
            }
            else if (currentLevel is 20)
            {
                levelUpMenu.isProfessionChooser = true;
                foreach (var item in ModEntry.Professions)
                {
                    if (currentLevel == item.Value.LevelRequirement && currentSkill == item.Value.Skill)
                        _professionsToChoose.Add(item.Value.ID);
                }
                ModEntry.Helper.Reflection.GetField<List<int>>(levelUpMenu, "professionsToChoose").SetValue(_professionsToChoose);
            }
        }

        public static bool IsPreProfessionsMet(KeyValuePair<string, Profession> keyValuePair, ref ClickableTextureComponent component)
        {
            if (keyValuePair.Value.LevelRequirement is 15)
            {
                return component.name == keyValuePair.Value.FirstRequires.ToString();
            }
            else
            {
                string lab = component.label;
                component.label = null;
                return lab == keyValuePair.Value.Skill.ToString();
            }
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
