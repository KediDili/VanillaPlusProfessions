using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents.UI
{
    internal class SkillTree
    {
        internal readonly string SkillIndex;

        readonly string TreeTitle;

        readonly Texture2D Texture;

        internal List<BundleIcon> Bundles = new();

        internal Rectangle Rectangle;

        internal Rectangle LockedRect;

        internal Rectangle AvailableRect;

        internal Rectangle BoughtRect;

        internal Color? BundleColor = null;

        internal static TalentSelectionMenu MainMenu = null;

        internal int TalentsBought = 0;

        internal List<ClickableTextureComponent> buttons = new();

        public SkillTree(string skillIndex, string treeTitle, Texture2D texture, List<Talent> talents, Rectangle rectangle, int bundleID = -1, Color? tintColor = null)
        {
            if (talents?.Count == 0)
            {
                return;
            }
            SkillIndex = skillIndex;
            TreeTitle = treeTitle;
            Texture = texture;
            Rectangle = rectangle;
            if (bundleID > -1)
            {
                Rectangle lockedRect = TalentSelectionMenu.getSourceRectByIndex(bundleID, true);
                LockedRect = lockedRect;
                Rectangle availableRect = TalentSelectionMenu.getSourceRectByIndex(bundleID, false);
                AvailableRect = availableRect;
                BoughtRect = availableRect;
                BoughtRect.X += 240;
            }
            else
            {
                LockedRect = new(0, 16, 16, 16);
                AvailableRect = new(16, 16, 16, 16);
                BoughtRect = new(32, 16, 16, 16);
                BundleColor = tintColor;
            }
            foreach (var tal in talents)
            {
                //Because fuck delegates capturing variables.
                Talent talent = tal;
                ClickableTextureComponent button = new(talent.DisplayName is null ? ModEntry.Helper.Translation.Get("Talent." + talent.Name + ".Name") : talent.DisplayName.Invoke(talent.Name), new((int)(talent.Position.X * 4), ((int)talent.Position.Y * 4), 64, 64), "", talent.Description is null? ModEntry.Helper.Translation.Get("Talent." + talent.Name + ".Desc") : talent.Description.Invoke(talent.Name), TalentSelectionMenu.JunimoNote, TalentSelectionMenu.getSourceRectByIndex(bundleID, !TalentUtility.CurrentPlayerHasTalent(talent.MailFlag)), 4f, false);
                TalentSelectionMenu.ID_Prefix++;
                button.myID = TalentSelectionMenu.ID_Prefix;
                button.fullyImmutable = true;
                Bundles.Add(new(button, talent));
                buttons.Add(button);
            }
            AssignAllNeighbors();
        }

        internal void UpdateIconVisibility(bool isVisible)
        {
            for (int i = 0; i < Bundles.Count; i++)
            {
                Bundles[i].button.visible = isVisible;
            }
        }

        private void AssignAllNeighbors()
        {
            for (int i = 0; i < Bundles.Count; i++)
            {
                Dictionary<string, Vector2> AllCandidates = new();
                List<string> UpQuadrant = new();
                List<string> DownQuadrant = new();
                List<string> RightQuadrant = new();
                List<string> LeftQuadrant = new();

                Point bundlePoint = Bundles[i].button.bounds.Location;

                for (int x = 0; x < Bundles.Count; x++)
                {
                    if (x != i)
                    {
                        AllCandidates.Add(Bundles[x].talent.Name, (Bundles[x].button.bounds.Location - bundlePoint).ToVector2());
                    }
                }
                AllCandidates.Add("Menu_RightArrow", (MainMenu.RightArrow.bounds.Location - bundlePoint).ToVector2());
                AllCandidates.Add("Menu_LeftArrow", (MainMenu.LeftArrow.bounds.Location - bundlePoint).ToVector2());
                AllCandidates.Add("Menu_ResetItemOne", (MainMenu.ResetItemOne.bounds.Location - bundlePoint).ToVector2());
                AllCandidates.Add("Menu_ResetItemAll", (MainMenu.ResetItemAll.bounds.Location - bundlePoint).ToVector2());
                AllCandidates.Add("Menu_upperRightCloseButton", (MainMenu.upperRightCloseButton.bounds.Location - bundlePoint).ToVector2());

                ClickableTextureComponent GetComponent(string ID)
                {
                    return ID switch
                    {
                        "Menu_RightArrow" => MainMenu.RightArrow,
                        "Menu_LeftArrow" => MainMenu.LeftArrow,
                        "Menu_ResetItemOne" => MainMenu.ResetItemOne,
                        "Menu_ResetItemAll" => MainMenu.ResetItemAll,
                        "Menu_upperRightCloseButton" => MainMenu.upperRightCloseButton,
                        _ => null
                    };
                }

                foreach ((string name, Vector2 distance) in AllCandidates)
                {
                    if (distance.Y < distance.X && distance.Y < -distance.X)
                        UpQuadrant.Add(name);
                    else if (distance.Y < distance.X && distance.Y > -distance.X)
                        RightQuadrant.Add(name);
                    else if (distance.Y > distance.X && distance.Y > -distance.X)
                        DownQuadrant.Add(name); 
                    else if (distance.Y > distance.X && distance.Y < -distance.X)
                        LeftQuadrant.Add(name);
                }
                float previous_distance = float.MaxValue;

                for (int u = 0; u < UpQuadrant.Count; u++)
                {
                    string name = UpQuadrant[u]; //fuck delegates
                    BundleIcon icon = Bundles.Find(bundle => bundle.talent.Name == name);
                    float candidate_distance = AllCandidates[name].Length();
                    if (name.StartsWith("Menu_"))
                    {
                        if (previous_distance > candidate_distance && candidate_distance > 0)
                        {
                            Bundles[i].button.upNeighborID = GetComponent(name).myID;
                            previous_distance = candidate_distance;
                        }
                    }
                    else if (Bundles[i].talent.Requirements.Contains(icon.talent.Name) || icon.talent.Requirements.Contains(name))
                    {
                        Bundles[i].button.upNeighborID = icon.button.myID;
                        break;
                        //previous_distance = candidate_distance;
                    }
                    else if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        Bundles[i].button.upNeighborID = icon.button.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int d = 0; d < DownQuadrant.Count; d++)
                {
                    string name = DownQuadrant[d]; //fuck delegates
                    BundleIcon icon = Bundles.Find(bundle => bundle.talent.Name == name);
                    float candidate_distance = AllCandidates[name].Length();
                    if (name.StartsWith("Menu_"))
                    {
                        if (previous_distance > candidate_distance && candidate_distance > 0)
                        {
                            Bundles[i].button.downNeighborID = GetComponent(name).myID;
                            previous_distance = candidate_distance;
                        }
                    }
                    else if(Bundles[i].talent.Requirements.Contains(icon.talent.Name) || icon.talent.Requirements.Contains(name))
                    {
                        Bundles[i].button.downNeighborID = icon.button.myID;
                        break; //previous_distance = candidate_distance;
                    }
                    else if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        Bundles[i].button.downNeighborID = icon.button.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int r = 0; r < RightQuadrant.Count; r++)
                {
                    string name = RightQuadrant[r]; //fuck delegates
                    BundleIcon icon = Bundles.Find(bundle => bundle.talent.Name == name);
                    float candidate_distance = AllCandidates[name].Length();
                    if (name.StartsWith("Menu_"))
                    {
                        if (previous_distance > candidate_distance && candidate_distance > 0)
                        {
                            Bundles[i].button.rightNeighborID = GetComponent(name).myID;
                            previous_distance = candidate_distance;
                        }
                    }
                    else if(Bundles[i].talent.Requirements.Contains(icon.talent.Name) || icon.talent.Requirements.Contains(name))
                    {
                        Bundles[i].button.rightNeighborID = icon.button.myID;
                        break; //previous_distance = candidate_distance;
                    }
                    else if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        Bundles[i].button.rightNeighborID = icon.button.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int l = 0; l < LeftQuadrant.Count; l++)
                {
                    string name = LeftQuadrant[l]; //fuck delegates
                    BundleIcon icon = Bundles.Find(bundle => bundle.talent.Name == name);
                    float candidate_distance = AllCandidates[name].Length();
                    if (name.StartsWith("Menu_"))
                    {
                        if (previous_distance > candidate_distance && candidate_distance > 0)
                        {
                            Bundles[i].button.leftNeighborID = GetComponent(name).myID;
                            previous_distance = candidate_distance;
                        }
                    }
                    else if (Bundles[i].talent.Requirements.Contains(icon.talent.Name) || icon.talent.Requirements.Contains(name))
                    {
                        Bundles[i].button.leftNeighborID = icon.button.myID;
                        break; //previous_distance = candidate_distance;
                    }
                    else if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        Bundles[i].button.leftNeighborID = icon.button.myID;
                        previous_distance = candidate_distance;
                    }
                }
                buttons.Add(Bundles[i].button);
            }
        }

        public void GameWindowChanged()
        {
            foreach (var item in Bundles)
            {
                item.GameWindowChanged();
            }
        }

        public void draw(SpriteBatch b)
        {
            SpriteText.drawString(b, TreeTitle, TalentSelectionMenu.XPos + 640 - (SpriteText.getWidthOfString(TreeTitle) / 2), TalentSelectionMenu.YPos + (SpriteText.getHeightOfString(TreeTitle) / 2), scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            b.Draw(Texture, new Vector2(TalentSelectionMenu.XPos, TalentSelectionMenu.YPos), Rectangle == new Rectangle() ? null : Rectangle, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);

            for (int i = 0; i < Bundles.Count; i++)
            {
                if (BundleColor.HasValue)
                {
                    Bundles[i].button.draw(b, BundleColor.Value * (Bundles[i].Availability ? 1 : 0.8f), 0.5f);
                }
                else
                {
                    Bundles[i].button.draw(b);
                }
            }
        }

        public void RefundPoints()
        {
            int count = 0;
            foreach (var item in Bundles)
            {
                if (item.talent.Branches is not null)
                {
                    foreach (var branch in item.talent.Branches)
                    {
                        if (Game1.player.mailReceived.Remove(branch.Flag))
                        {
                            Game1.player.mailReceived.Remove(item.talent.MailFlag);
                            item.talent.OnTalentRemoved?.Invoke(item.talent.Name, false);
                            count++;
                            break;
                        }
                    }
                }
                else if (Game1.player.mailReceived.Remove(item.talent.MailFlag))
                {
                    item.talent.OnTalentRemoved?.Invoke(item.talent.Name, false);
                    count++;
                }         
            }
            TalentCore.TalentPointCount.Value += count;
            TalentsBought = 0;
            foreach (var item in Bundles)
            {
                item.UpdateSprite();
                item.Tree = this;
                item.InitializeFully();
            }
        }
    }
}
