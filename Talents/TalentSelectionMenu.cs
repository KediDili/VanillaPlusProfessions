using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using System.Linq;

namespace VanillaPlusProfessions.Talents
{
    public class TalentSelectionMenu : IClickableMenu
    {
        internal static int CurrentSkill;

        internal GameMenu menu;

        internal ClickableTextureComponent RightArrow;

        internal ClickableTextureComponent LeftArrow;

        internal static int XPos;

        internal static int YPos;

        internal static List<SkillTree> skillTrees = new();

        internal List<string> skillNames = new();

        internal static Texture2D SkillIconBG;

        internal static Texture2D TalentPointBG;

        internal static Texture2D JunimoNote;

        public TalentSelectionMenu(int skill, GameMenu gamemenu)
        {
            CurrentSkill = skill;
            JunimoNote = ModEntry.Helper.GameContent.Load<Texture2D>("LooseSprites\\JunimoNote");
            menu = gamemenu;

            SkillIconBG = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\SkillIconBG.png");
            TalentPointBG = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\TalentPointBackGround.png");

            XPos = xPositionOnScreen + 213;
            YPos = yPositionOnScreen + 154;

            RightArrow = new(new Rectangle(XPos + 1116, YPos + 332, 64, 64), Game1.mouseCursors, new Rectangle(0, 192, 64, 64), 1f, true);
            LeftArrow = new(new(XPos + 92, YPos + 332, 64, 64), Game1.mouseCursors, new Rectangle (0, 256, 64, 64), 1f, true);
            skillTrees = new();

            //Remove this before release, or SpaceCore skills stuff will break
            TalentCore.Talents = ModEntry.Helper.ModContent.Load<List<Talent>>("assets\\talents.json");

            var farmingList = (from talent in TalentCore.Talents where talent.Skill is "Farming" select talent).ToList();
            var miningList = (from talent in TalentCore.Talents where talent.Skill is "Mining" select talent).ToList();
            var foragingList = (from talent in TalentCore.Talents where talent.Skill is "Foraging" select talent).ToList();
            var fishingList = (from talent in TalentCore.Talents where talent.Skill is "Fishing" select talent).ToList();
            var combatList = (from talent in TalentCore.Talents where talent.Skill is "Combat" select talent).ToList();

            Texture2D farmingTree = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\FarmingSkillTree.png");
            Texture2D miningTree = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\MiningSkillTree.png");
            // Texture2D foragingTree = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\ForagingSkillTree.png");
            // Texture2D fishingTree = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\FishingSkillTree.png");
            // Texture2D combatTree = ModEntry.Helper.ModContent.Load<Texture2D>("assets\\CombatSkillTree.png");

            skillTrees.Add(new("Farming", 0, ModEntry.Helper.Translation.Get("Talent.Farming.Title"), farmingTree, farmingList, new(), new()));
            skillTrees.Add(new("Mining", 1, ModEntry.Helper.Translation.Get("Talent.Mining.Title"), miningTree, miningList, new(), new()));
            // skillTrees.Add(new("Foraging", 2, ModEntry.Helper.Translation.Get("Talent.Foraging.Title"), foragingTree, null, foragingList));
            // skillTrees.Add(new("Fishing", 3, ModEntry.Helper.Translation.Get("Talent.Fishing.Title"), fishingTree, null, fishingList));
            // skillTrees.Add(new("Combat", 4, ModEntry.Helper.Translation.Get("Talent.Combat.Title"), combatTree, null, combatList));
            skillTrees.AddRange(ModEntry.VanillaPlusProfessionsAPI.CustomTalentTrees.Values);

            skillNames = new()
            {
                "Farming",
                "Mining"
            };
            skillNames.AddRange(ModEntry.SpaceCoreAPI.Value.GetCustomSkills());
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            //main bg
            b.Draw(JunimoNote, new Vector2(XPos, YPos), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);

            b.Draw(SkillIconBG, new Vector2(xPositionOnScreen + (JunimoNote.Width * 2) - 129, yPositionOnScreen + (JunimoNote.Height * 2) - 5), new Rectangle(0, 0, 60, 41), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            //skill icon
            b.Draw(JunimoNote, new Vector2(xPositionOnScreen + (JunimoNote.Width * 2) - 73, yPositionOnScreen + (JunimoNote.Height * 2) + 11), getSourceRectForSkill(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);

            skillTrees[CurrentSkill].draw(b);

            RightArrow.draw(b);
            LeftArrow.draw(b);

            b.Draw(TalentPointBG, new Vector2(xPositionOnScreen + (JunimoNote.Width * 2) - 30, yPositionOnScreen + (JunimoNote.Height - 50)), new Rectangle(0, 0, 35, 25), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            SpriteText.drawString(b, TalentCore.TalentPointCount.Value.ToString(), xPositionOnScreen + (JunimoNote.Width * 2) + 10, yPositionOnScreen + (JunimoNote.Height - 25), scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);

            for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
            {
                if (skillTrees[CurrentSkill].Bundles[i].button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    drawHoverText(b, skillTrees[CurrentSkill].Bundles[i].Availability ? skillTrees[CurrentSkill].Bundles[i].button.hoverText : "Gain other talents\nfirst!", Game1.smallFont, boldTitleText: skillTrees[CurrentSkill].Bundles[i].Availability ? skillTrees[CurrentSkill].Bundles[i].button.name : "Locked");
            }
            
            drawMouse(b);
        }

        public override void update(GameTime gameTime)
        {
            for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
            {
                if (skillTrees[CurrentSkill].Bundles[i].animIsUp)
                {
                    skillTrees[CurrentSkill].Bundles[i].animatedSprite.update(gameTime);
                    EndOfAnimation(i);
                }
            }
        }

        internal static Rectangle getSourceRectByIndex(int index)
        {
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
        private Rectangle getSourceRectForSkill()
        {
            return CurrentSkill switch
            {
                0 => new(64, 180, 32, 32),
                1 => new(0, 212, 32, 32),
                2 => new(412, 180, 32, 32),
                3 => new(224, 180, 32, 32),
                4 => new(64, 212, 32, 32),
                _ => (Rectangle)skillTrees[CurrentSkill].Rectangle,
            };
        }

        public override void performHoverAction(int x, int y)
        {
            RightArrow.tryHover(x, y);
            LeftArrow.tryHover(x, y);
            for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
            {
                skillTrees[CurrentSkill].Bundles[i].button.tryHover(x, y);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (RightArrow.containsPoint(x, y))
            {
                if (CurrentSkill + 1 == skillNames.Count)
                {
                    CurrentSkill = 0;
                }
                else
                {
                    CurrentSkill++;
                }
            }
            else if (LeftArrow.containsPoint(x, y))
            {
                if (CurrentSkill - 1 < 0)
                {
                    CurrentSkill = skillNames.Count - 1;
                }
                else
                {
                    CurrentSkill--;
                }
            }
            else
            {
                for (int i = 0; i < skillTrees[CurrentSkill].Bundles.Count; i++)
                {
                    if (skillTrees[CurrentSkill].Bundles[i].button.containsPoint(x, y))
                    {
                        if (skillTrees[CurrentSkill].Bundles[i].Availability && !TalentCore.GainedTalents.Value.Contains(skillTrees[0].Bundles[i].talent.Name) && TalentCore.TalentPointCount.Value > 0)
                            skillTrees[CurrentSkill].Bundles[i].animIsUp = true;
                        break;
                    }
                }
            }
        }
        internal static void EndOfAnimation(int i)
        {
            if (skillTrees[CurrentSkill].Bundles[i].Availability && !TalentCore.GainedTalents.Value.Contains(skillTrees[CurrentSkill].Bundles[i].talent.Name) && TalentCore.TalentPointCount.Value > 0)
            {
                TalentCore.TalentPointCount.Value--;
                TalentCore.GainedTalents.Value.Add(skillTrees[CurrentSkill].Bundles[i].talent.Name);
                skillTrees[CurrentSkill].Bundles[i].button.sourceRect.X += 240;
                skillTrees[CurrentSkill].Bundles[i].animIsUp = false;
            }
        }
    }
}
