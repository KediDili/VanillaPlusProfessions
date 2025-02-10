using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using System.Linq;
using VanillaPlusProfessions.Utilities;
using Microsoft.Xna.Framework.Input;

namespace VanillaPlusProfessions.Talents.UI
{
    public class TalentSelectionMenu : IClickableMenu
    {
        internal static int ID_Prefix = 467800;

        internal static int CurrentSkill;

        internal ClickableTextureComponent RightArrow;

        internal ClickableTextureComponent LeftArrow;

        internal static int XPos;

        internal static int YPos;

        internal static List<SkillTree> skillTrees = new();

        internal static Texture2D TalentBG;

        internal static Texture2D BundleIcon;

        internal static Texture2D JunimoNote;

        internal bool AnyActiveBranches = false;

        internal ClickableTextureComponent ResetItemAll;

        internal ClickableTextureComponent ResetItemOne;

        internal bool ShouldConfirmFullReset = false;

        internal bool ShouldConfirmOneReset = false;

        internal string[] ResetMessages;

        internal Dictionary<string, string> TalentsToReset = new();

        internal Dictionary<string, string> TalentsBeforeMenuOpen = new();

        public TalentSelectionMenu(int skill)
        {
            JunimoNote = ModEntry.Helper.GameContent.Load<Texture2D>("LooseSprites\\JunimoNote");

            TalentBG = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["TalentBG"]);
            BundleIcon = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["BundleIcons"]);

            width = 640;
            height = 360;
            XPos = (Game1.uiViewport.Width / 2) - width;
            YPos = (Game1.uiViewport.Height / 2) - height;

            RightArrow = new(new Rectangle(XPos + 1116, YPos + 332, 64, 64), Game1.mouseCursors, new Rectangle(0, 192, 64, 64), 1f, true)
            {
                myID = 467801,
                fullyImmutable = true
            };

            LeftArrow = new(new(XPos + 92, YPos + 332, 64, 64), Game1.mouseCursors, new Rectangle(0, 256, 64, 64), 1f, true)
            {
                myID = 467802,
                fullyImmutable = true
            };

            ResetItemAll = new("", new Rectangle(XPos + 110, YPos + 110, 64, 64), "", "", TalentBG, new(0, 0, 16, 16), 4f, false)
            {
                item = ItemRegistry.Create<StardewValley.Object>("(O)74"),
                fullyImmutable = true,
                myID = 467803
            };

            ResetItemOne = new("", new(XPos + 190, YPos + 110, 64, 64), "", "", TalentBG, new(0, 0, 16, 16), 4f, false)
            {
                item = ItemRegistry.Create<StardewValley.Object>("(O)StardropTea"),
                fullyImmutable = true,
                myID = 467804
            };

            ID_Prefix += 4;

            ResetMessages = new string[]
            {
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.ItemMissing"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.SettleTreeFirst"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.SettleFullFirst"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.ResetAll"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.ResetTree"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.Confirm"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.BoughtNone"),
                ModEntry.Helper.Translation.Get("Talent.ResetMessages.AbortReset"),
            };
            skillTrees = new();

            foreach (var branch in TalentCore.Talents["MonsterSpecialist"].Branches)
            {
                Talent.Branch branch2 = branch;
                branch.DisplayName = str => ModEntry.Helper.Translation.Get($"Talent.Branch.{branch2.Name}.Name");
                branch.Desc = str => ModEntry.Helper.Translation.Get($"Talent.Branch.{branch2.Name}.Desc");
            }

            var farmingList = (from talent in TalentCore.Talents where talent.Value.Skill is "Farming" select talent.Value).ToList();
            var miningList = (from talent in TalentCore.Talents where talent.Value.Skill is "Mining" select talent.Value).ToList();
            var foragingList = (from talent in TalentCore.Talents where talent.Value.Skill is "Foraging" select talent.Value).ToList();
            var fishingList = (from talent in TalentCore.Talents where talent.Value.Skill is "Fishing" select talent.Value).ToList();
            var combatList = (from talent in TalentCore.Talents where talent.Value.Skill is "Combat" select talent.Value).ToList();
            var miscList = (from talent in TalentCore.Talents where talent.Value.Skill is "Misc" select talent.Value).ToList();

            Texture2D AllTrees = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["TalentSchema"]);
            SkillTree.MainMenu = this;

            foreach (var item in TalentCore.Talents.Values)
            {
                string talent = item.Name;
                if (TalentUtility.CurrentPlayerHasTalent(talent))
                {
                    TalentsBeforeMenuOpen.Add(talent, TalentUtility.ValidTalentStatuses[0]);
                }
                else if (TalentUtility.CurrentPlayerHasTalent(talent, ignoreDisabledTalents: false))
                {
                    TalentsBeforeMenuOpen.Add(talent, TalentUtility.ValidTalentStatuses[1]);
                }
                else
                {
                    TalentsBeforeMenuOpen.Add(talent, TalentUtility.ValidTalentStatuses[2]);
                }
            }

            initializeUpperRightCloseButton();

            skillTrees.Add(new("Farming", ModEntry.Helper.Translation.Get("Talent.Farming.Title"), AllTrees, farmingList, new(0, 0, 320, 180), 1));
            skillTrees.Add(new("Mining", ModEntry.Helper.Translation.Get("Talent.Mining.Title"), AllTrees, miningList, new(320, 0, 320, 180), 5));
            skillTrees.Add(new("Foraging", ModEntry.Helper.Translation.Get("Talent.Foraging.Title"), AllTrees, foragingList, new(0, 180, 320, 180), 0));
            skillTrees.Add(new("Fishing", ModEntry.Helper.Translation.Get("Talent.Fishing.Title"), AllTrees, fishingList, new(320, 180, 320, 180), 3));
            skillTrees.Add(new("Combat", ModEntry.Helper.Translation.Get("Talent.Combat.Title"), AllTrees, combatList, new(0, 360, 320, 180), 4));

            //This order is important, because there is no skill named Misc
            skillTrees.AddRange(ModEntry.VanillaPlusProfessionsAPI.CustomTalentTrees.Values);
            skillTrees.Add(new("Misc", ModEntry.Helper.Translation.Get("Talent.Misc.Title"), AllTrees, miscList, new(320, 360, 320, 180), 5));

            
            xPositionOnScreen = XPos;
            yPositionOnScreen = YPos;
            initializeUpperRightCloseButton();
            upperRightCloseButton.bounds.X += width;

            CurrentSkill = skill;
            if (CurrentSkill > skillTrees.Count - 1)
            {
                CurrentSkill = 0;
            }
            foreach (var tree in skillTrees)
            {
                foreach (var talentbutton in tree.Bundles)
                {
                    if (TalentUtility.CurrentPlayerHasTalent(talentbutton.talent.MailFlag, ignoreDisabledTalents: false))
                        tree.TalentsBought++;
                }
                foreach (var talentbutton in tree.Bundles)
                {
                    talentbutton.Tree = tree;
                    talentbutton.UpdateSprite();
                }
            }

            UI.BundleIcon.LockedName = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Name");
            UI.BundleIcon.LockedDesc = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Desc");
            UI.BundleIcon.NumberLocked = ModEntry.Helper.Translation.Get("Talent.LockedTalent.Numbered");
            UI.BundleIcon.Disabled = ModEntry.Helper.Translation.Get("Talent.DisabledTalent");

            FixBaseButtonNeighborIDs();
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void populateClickableComponentList()
        {
            allClickableComponents = new()
            {
                RightArrow,
                LeftArrow,
                ResetItemAll,
                ResetItemOne,
                upperRightCloseButton
            };

            foreach (var tree in skillTrees)
            {
                allClickableComponents.AddRange(tree.buttons);
            }
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            //main bg
            b.Draw(JunimoNote, new Vector2(XPos, YPos), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);

            b.Draw(TalentBG, new Vector2(XPos, YPos), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);

            skillTrees[CurrentSkill].draw(b);
            
            ResetItemAll.drawItem(b);
            ResetItemOne.drawItem(b);
            
            RightArrow.draw(b);
            LeftArrow.draw(b);

            SpriteText.drawString(b, TalentCore.TalentPointCount.Value.ToString(), XPos + (int)(JunimoNote.Width * 1.66), YPos + (int)(JunimoNote.Height * 0.42), scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            if (!AnyActiveBranches)
            {
                for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
                {
                    if (skillTrees[CurrentSkill].Bundles[i].button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                        drawHoverText(b, skillTrees[CurrentSkill].Bundles[i].GetTalentDescription(), Game1.smallFont, boldTitleText: skillTrees[CurrentSkill].Bundles[i].Availability ? skillTrees[CurrentSkill].Bundles[i].button.name : UI.BundleIcon.LockedName);
                }
                if (ResetItemAll.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    drawHoverText(b, GetHoverTextForAllReset(), Game1.smallFont);
                }
                else if (ResetItemOne.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    drawHoverText(b, GetHoverTextForOneReset(), Game1.smallFont);
                }
                else if (ShouldConfirmOneReset || ShouldConfirmFullReset)
                {
                    drawHoverText(b, ResetMessages[7], Game1.smallFont);
                }
                base.draw(b);
                drawMouse(b);
            }
            else
            {
                GetChildMenu()?.draw(b);
            }
        }

        internal static Rectangle getSourceRectByIndex(int index, bool locked)
        {
            if (locked)
            {
                return index switch
                {
                    0 => new(0, 0, 16, 16), //256
                    1 => new(16, 0, 16, 16),
                    2 => new(32, 0, 16, 16),
                    3 => new(48, 0, 16, 16),
                    4 => new(64, 0, 16, 16),
                    5 => new(80, 0, 16, 16),
                    6 => new(96, 0, 16, 16),
                    _ => new(),
                };
            }
            return index switch
            {
                0 => new(0, 244, 16, 16), //256
                1 => new(0, 260, 16, 16),
                2 => new(0, 276, 16, 16),
                3 => new(0, 292, 16, 16),
                4 => new(256, 244, 16, 16),
                5 => new(256, 260, 16, 16),
                6 => new(256, 276, 16, 16),
                _ => new(),
            };
        }

        public string GetHoverTextForAllReset()
        {
            if (ShouldConfirmOneReset)
            {
                return ResetMessages[1];
            }
            int count = 0;
            foreach (var tree in skillTrees)
            {
                count += tree.TalentsBought;
            }

            if (count is 0)
            {
                return ResetMessages[6];
            }
            else if (!Game1.player.Items.ContainsId(ResetItemAll.item.QualifiedItemId))
            {
                return ResetMessages[0];
            }
            else
            {
                return !ShouldConfirmFullReset ? ResetMessages[3] : ResetMessages[5];
            }
        }

        public string GetHoverTextForOneReset()
        {
            if (ShouldConfirmFullReset)
            {
                return ResetMessages[2];
            }
            else if (skillTrees[CurrentSkill].TalentsBought <= 0)
            {
                return ResetMessages[6];
            }
            else if (!Game1.player.Items.ContainsId(ResetItemOne.item.QualifiedItemId))
            {
                return ResetMessages[0];
            }
            else
            {
                return !ShouldConfirmOneReset ? ResetMessages[4] : ResetMessages[5];
            }
        }
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            RightArrow.tryHover(x, y);
            LeftArrow.tryHover(x, y);
            for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
            {
                skillTrees[CurrentSkill].Bundles[i].button.tryHover(x, y);
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) 
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            XPos = (newBounds.Width / 2) - width;
            YPos = (newBounds.Height / 2) - height;
            RightArrow.bounds.X = XPos + 1116;
            RightArrow.bounds.Y = YPos + 332;
            LeftArrow.bounds.X = XPos + 92;
            LeftArrow.bounds.Y = RightArrow.bounds.Y;
            ResetItemAll.bounds.X = XPos + 110;
            ResetItemAll.bounds.Y = YPos + 110;
            ResetItemOne.bounds.X = XPos + 190;
            ResetItemOne.bounds.Y = ResetItemAll.bounds.Y;
            for (int i = 0; i < skillTrees.Count; i++)
            {
                skillTrees[i].GameWindowChanged();
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (AnyActiveBranches)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            if (ResetItemAll.containsPoint(x, y) && !ShouldConfirmOneReset)
            {
                if (ShouldConfirmFullReset)
                {
                    for (int i = 0; i < skillTrees.Count; i++)
                    {
                        skillTrees[i].RefundPoints();
                    }
                    Game1.player.Items.ReduceId("(O)74", 1);
                    ShouldConfirmFullReset = false;
                }
                else
                {
                    if (Game1.player.Items.ContainsId(ResetItemAll.item.QualifiedItemId))
                    {
                        int count = 0;
                        foreach (var tree in skillTrees)
                        {
                            count += tree.TalentsBought;
                        }
                        if (count is not 0)
                        {
                            ShouldConfirmFullReset = true;
                        }
                    }
                }
            }
            else if (ResetItemOne.containsPoint(x, y) && !ShouldConfirmFullReset)
            {
                if (ShouldConfirmOneReset)
                {
                    skillTrees[CurrentSkill].RefundPoints();
                    Game1.player.Items.ReduceId("(O)StardropTea", 1);
                    ShouldConfirmOneReset = false;
                }
                else
                {
                    if (Game1.player.Items.ContainsId(ResetItemOne.item.QualifiedItemId))
                    {   
                        if (skillTrees[CurrentSkill].TalentsBought is not 0)
                        {
                            ShouldConfirmOneReset = true;
                        }
                    }
                }
            }
            else if (!ResetItemOne.containsPoint(x, y) && !ResetItemAll.containsPoint(x, y) && ShouldConfirmOneReset)
            {
                ShouldConfirmOneReset = false;
            }
            else if (!ResetItemAll.containsPoint(x, y) && !ResetItemOne.containsPoint(x, y) && ShouldConfirmFullReset)
            {
                ShouldConfirmFullReset = false;
            }

            if (RightArrow.containsPoint(x, y))
            {
                if (CurrentSkill + 1 == skillTrees.Count)
                {
                    CurrentSkill = 0;
                }
                else
                {
                    CurrentSkill++;
                }
                for (int i = 0; i < skillTrees.Count; i++)
                {
                    skillTrees[i].UpdateIconVisibility(CurrentSkill == i);
                }
                FixBaseButtonNeighborIDs();
            }
            else if (LeftArrow.containsPoint(x, y))
            {
                if (CurrentSkill - 1 < 0)
                {
                    CurrentSkill = skillTrees.Count - 1;
                }
                else
                {
                    CurrentSkill--;
                }
                for (int i = 0; i < skillTrees.Count; i++)
                {
                    skillTrees[i].UpdateIconVisibility(CurrentSkill == i);
                }
                FixBaseButtonNeighborIDs();
            }
            else
            {
                for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
                {
                    if (skillTrees[CurrentSkill].Bundles[i].button.containsPoint(x, y))
                    {
                        if (skillTrees[CurrentSkill].Bundles[i].Availability)
                        {
                            if (!Game1.player.mailReceived.Contains(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag) && TalentCore.TalentPointCount.Value > 0)
                            {
                                Game1.player.currentLocation.playSound("wand");

                                if (skillTrees[CurrentSkill].Bundles[i].talent.Branches is not null)
                                {
                                    BranchDisplayer branchDisplayer = new(skillTrees[CurrentSkill].Bundles[i].talent, i);
                                    SetChildMenu(branchDisplayer);
                                    AnyActiveBranches = true;
                                }
                                else
                                {
                                    AddOrRemoveTalent(skillTrees[CurrentSkill].Bundles[i].talent.Name, null, true);
                                    Game1.player.mailReceived.Add(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag);
                                    ReducePoints();
                                    skillTrees[CurrentSkill].Bundles.ForEach(bundle => bundle.UpdateSprite());
                                }
                            }
                            else if (Game1.player.mailReceived.Contains(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag))
                            {
                                Game1.player.currentLocation.playSound("wand");

                                if (!Game1.player.mailReceived.Contains(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag + "_disabled"))
                                {
                                    AddOrRemoveTalent(skillTrees[CurrentSkill].Bundles[i].talent.Name, false, null);
                                    Game1.player.mailReceived.Add(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag + "_disabled");
                                }
                                else
                                {
                                    AddOrRemoveTalent(skillTrees[CurrentSkill].Bundles[i].talent.Name, true, null);
                                    Game1.player.mailReceived.Remove(skillTrees[CurrentSkill].Bundles[i].talent.MailFlag + "_disabled");
                                }
                                skillTrees[CurrentSkill].Bundles.ForEach(bundle => bundle.UpdateSprite());
                            }
                        }
                        break;
                    }
                }
            }
        }

        internal void AddOrRemoveTalent(string talent, bool? EnableOrDisable, bool? BoughtOrRefunded)
        {
            string value = "";
            string flag = TalentUtility.GetFlag(talent);
            if (EnableOrDisable.HasValue)
            {
                if ((Game1.player.mailReceived.Contains(flag + "_disabled") && EnableOrDisable.Value) || (!Game1.player.mailReceived.Contains(flag + "_disabled") && !EnableOrDisable.Value))
                {
                    value = TalentUtility.ValidTalentStatuses[EnableOrDisable.Value ? 0 : 2];
                }
            }
            else if (BoughtOrRefunded.HasValue)
            {
                if ((Game1.player.mailReceived.Contains(flag) && !BoughtOrRefunded.Value) || (!Game1.player.mailReceived.Contains(flag) && BoughtOrRefunded.Value))
                {
                    value = TalentUtility.ValidTalentStatuses[BoughtOrRefunded.Value ? 0 : 1];
                }
            }
            if (value != "")
            {
                //This part wont filter the parts correctly.
                //The only solution left is to cache things first, then check here.
                TalentsToReset[talent] = value;
                if (TalentsBeforeMenuOpen.TryGetValue(talent, out string val) && value == val)
                {
                    TalentsToReset.Remove(talent);
                }
            }
        }

        protected override void cleanupBeforeExit()
        {
            if (ModEntry.VanillaPlusProfessionsAPI.RunBeforeTalentMenuCloses.Count > 0)
            {
                foreach (var item in ModEntry.VanillaPlusProfessionsAPI.RunBeforeTalentMenuCloses)
                {
                    IEnumerable<string> talents = item.Key.Intersect(TalentsToReset.Keys).ToList();
                    if (talents.Any())
                    {
                        Dictionary<string, string> pairs = new();

                        foreach (var talent in talents)
                        {
                            pairs.Add(talent, TalentsToReset[talent]);
                        }

                        try
                        {
                            item.Value.Invoke(pairs);
                        }
                        catch (System.Exception e)
                        {
                            ModEntry.ModMonitor.Log($"There was an unexpected error while updating talents before talent menu was exited: {e}\n\nDo NOT report this to VPP's page, as this is very unlikely to be caused by it. Details of the method that threw the error: {item.Value.Method.DeclaringType.Assembly.FullName ?? "<could not be found>"} assembly, {item.Value.Method.DeclaringType.FullName ?? "<could not be found>"}", StardewModdingAPI.LogLevel.Warn);
                        }
                    }
                }
            }
            var menu = new GameMenu(GameMenu.skillsTab);
            Game1.nextClickableMenu.Add(menu);
        }

        public void FixBaseButtonNeighborIDs()
        {
            List<string> UpQuadrant = new();
            List<string> DownQuadrant = new();
            List<string> RightQuadrant = new();
            List<string> LeftQuadrant = new();
            ClickableTextureComponent GetComponent(int ID, string name = null)
            {
                string name2 = name;
                if (int.TryParse(name2, out int result) && result < 5 && result > -1)
                {
                    ID = result;
                }
                return ID switch
                {
                    0 => RightArrow,
                    1 => LeftArrow,
                    2 => ResetItemAll,
                    3 => ResetItemOne,
                    4 => upperRightCloseButton,
                    _ => skillTrees[CurrentSkill].Bundles.Find(bundle => bundle.talent.Name == name2).button
                };
            }
            for (int i = 0; i < 5; i++)
            {
                Dictionary<string, Vector2> AllCandidates = new();
                foreach (var item in skillTrees[CurrentSkill].Bundles)
                {
                    AllCandidates.Add(item.talent.Name, (GetComponent(i).bounds.Center - item.button.bounds.Center).ToVector2());
                }
                for (int d = 0; d < 5; d++)
                {
                    AllCandidates.Add(d.ToString(), (GetComponent(i).bounds.Center - GetComponent(d).bounds.Center).ToVector2());
                }
                foreach ((string name, Vector2 distance) in AllCandidates)
                {
                    if (distance.Y < distance.X && distance.Y < -distance.X)
                        DownQuadrant.Add(name);
                    else if (distance.Y < distance.X && distance.Y > -distance.X)
                        LeftQuadrant.Add(name);
                    else if (distance.Y > distance.X && distance.Y > -distance.X)
                        UpQuadrant.Add(name); 
                    else if (distance.Y > distance.X && distance.Y < -distance.X)
                        RightQuadrant.Add(name); 
                }

                float previous_distance = float.MaxValue;
                for (int u = 0; u < UpQuadrant.Count; u++)
                {
                    string name = UpQuadrant[u]; //fuck delegates
                    float candidate_distance = AllCandidates[name].Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        GetComponent(i).upNeighborID = GetComponent(100, name).myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int d = 0; d < DownQuadrant.Count; d++)
                {
                    string name = DownQuadrant[d]; //fuck delegates
                    float candidate_distance = AllCandidates[name].Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        GetComponent(i).downNeighborID = GetComponent(100, name).myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int r = 0; r < RightQuadrant.Count; r++)
                {
                    string name = RightQuadrant[r]; //fuck delegates
                    float candidate_distance = AllCandidates[name].Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        GetComponent(i).rightNeighborID = GetComponent(100, name).myID;
                        previous_distance = candidate_distance;
                    }
                }
                previous_distance = float.MaxValue;
                for (int l = 0; l < LeftQuadrant.Count; l++)
                {
                    string name = LeftQuadrant[l]; //fuck delegates
                    float candidate_distance = AllCandidates[name].Length();
                    if (previous_distance > candidate_distance && candidate_distance > 0)
                    {
                        GetComponent(i).leftNeighborID = GetComponent(100, name).myID;
                        previous_distance = candidate_distance;
                    }
                }
            }
        }

        public void ReducePoints()
        {
            TalentCore.TalentPointCount.Value--;
            skillTrees[CurrentSkill].TalentsBought++;
            AnyActiveBranches = false;
        }
    }
}