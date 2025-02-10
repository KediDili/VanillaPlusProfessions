using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using SpaceCore.UI;

namespace VanillaPlusProfessions.Talents.UI
{
    public class BranchDisplayer : IClickableMenu
    {
        readonly List<BranchButton> Branches = new();

        readonly Talent Original;

        readonly List<ClickableComponent> buttonList = new();

        int Index;

        const int startingID = 467900;

        public BranchDisplayer(Talent original, int index)
        {
            for (int i = 0; i < original.Branches.Length; i++)
            {
                BranchButton branchButton = new()
                {
                    branch = original.Branches[i],
                    button = new((int)original.Branches[i].Position.X + TalentSelectionMenu.XPos, (int)original.Branches[i].Position.Y + TalentSelectionMenu.YPos, 60 + SpriteText.getWidthOfString(original.Branches[i].Name), 60 + SpriteText.characterHeight + 40)
                };
                buttonList.Add(new(branchButton.button, "") 
                {
                    fullyImmutable = true
                });
                Branches.Add(branchButton);
            }
            Original = original;
            Index = index;
            int a = 0;
            foreach (var item in buttonList)
            {
                item.myID = startingID + a;
                a++;
            }
            foreach (var item in buttonList)
            {
                Dictionary<ClickableComponent, Vector2> AllCandidates = new();
                List<(ClickableComponent, Vector2)> UpQuadrant = new();
                List<(ClickableComponent, Vector2)> DownQuadrant = new();
                List<(ClickableComponent, Vector2)> RightQuadrant = new();
                List<(ClickableComponent, Vector2)> LeftQuadrant = new();

                Point bundlePoint = item.bounds.Location;

                foreach (var button in buttonList)
                {
                    if (button != item)
                    {
                        AllCandidates.Add(button, (button.bounds.Location - item.bounds.Location).ToVector2());
                    }
                }

                foreach ((var button, var point) in AllCandidates)
                {
                    if (point.Y < point.X && point.Y < -point.X)
                        UpQuadrant.Add((button, point));
                    else if (point.Y < point.X && point.Y > -point.X)
                        RightQuadrant.Add((button, point));
                    else if (point.Y > point.X && point.Y > -point.X)
                        DownQuadrant.Add((button, point));
                    else if (point.Y > point.X && point.Y < -point.X)
                        LeftQuadrant.Add((button, point));
                }
                float previous_distance = float.MaxValue;

                for (int u = 0; u < UpQuadrant.Count; u++)
                {
                    float candidate_distance = UpQuadrant[u].Item2.Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        item.upNeighborID = UpQuadrant[u].Item1.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int d = 0; d < DownQuadrant.Count; d++)
                {
                    float candidate_distance = DownQuadrant[d].Item2.Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        item.downNeighborID = DownQuadrant[d].Item1.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int r = 0; r < RightQuadrant.Count; r++)
                {
                    float candidate_distance = RightQuadrant[r].Item2.Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        item.rightNeighborID = RightQuadrant[r].Item1.myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int l = 0; l < LeftQuadrant.Count; l++)
                {
                    float candidate_distance = LeftQuadrant[l].Item2.Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        item.leftNeighborID = LeftQuadrant[l].Item1.myID;
                        previous_distance = candidate_distance;
                    }
                }
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void populateClickableComponentList()
        {
            allClickableComponents ??= new();
            allClickableComponents.AddRange(buttonList);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key is Keys.Escape or Keys.E && Game1.activeClickableMenu is not null and TalentSelectionMenu menu)
            {
                menu.AnyActiveBranches = false;
                menu.SetChildMenu(null);
            }

            if (key is Keys.Up or Keys.W)
            {
                applyMovementKey(0);
            }
            else if (key is Keys.Down or Keys.S)
            {
                applyMovementKey(2);
            }
            else if (key is Keys.Right or Keys.D)
            {
                applyMovementKey(1);
            }
            else if (key is Keys.Left or Keys.A)
            {
                applyMovementKey(3);
            }
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            for (int i = 0; i < Branches.Count; i++)
            {
                Branches[i].draw(b);
            }
            for (int i = 0; i < Branches.Count; i++)
            {
                int index = i;
                if (Branches[i].button.Contains(Game1.getMouseX(), Game1.getMouseY()))
                {
                    drawHoverText(b, Branches[i].branch.Desc.Invoke(Branches[index].branch.Name), Game1.smallFont);
                    break;
                }
            }
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true) 
        {
            if (Game1.activeClickableMenu is not null and TalentSelectionMenu menu)
            {
                for (int i = 0; i < Branches.Count; i++)
                {
                    if (Branches[i].button.Contains(x, y))
                    {
                        menu.ReducePoints();
                        Game1.player.mailReceived.Add(Branches[i].branch.Flag);
                        Game1.player.mailReceived.Add(Original.MailFlag);
                        foreach (var item in TalentSelectionMenu.skillTrees[TalentSelectionMenu.CurrentSkill].Bundles)
                        {
                            item.UpdateSprite();
                        }
                        menu.SetChildMenu(null);                        
                        break;
                    }
                }
            }
        }
    }
}