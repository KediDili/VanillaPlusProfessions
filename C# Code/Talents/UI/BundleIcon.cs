using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents.UI
{
    public class BundleIcon
    {
        internal ClickableTextureComponent button;

        internal Talent talent;

        internal SkillTree Tree;

        Color? BundleColor;

        internal int XPos;

        internal int YPos;

        internal string LockedName;

        internal string LockedDesc;

        internal string NumberLocked;

        internal string Disabled;

        int TalentsBought;

        Rectangle LockedRect;

        Rectangle BoughtRect;

        Rectangle AvailableRect;

        internal bool Availability {
            get
            {
                if (talent.Requirements.Length is 0)
                {
                    return talent.AmountToBuyFirst <= TalentsBought;
                }
                else
                {
                    for (int i = 0; i < talent.Requirements.Length; i++)
                        if (TalentUtility.CurrentPlayerHasTalent(TalentCore.Talents[talent.Requirements[i]].MailFlag, ignoreDisabledTalents: false))
                            return true;
                    return false;
                }
            }
        }
        internal void UpdateSprite()
        {
            InitializeFully();
            if (Tree.MainMenu is not null)
            {
                if (Tree.MainMenu.AnyActiveBranches)
                {
                    button.visible = false;
                    return;
                }
                if (!Availability)
                {
                    //locked
                    button.sourceRect = LockedRect;
                    button.texture = Tree.BundleIcon;
                }
                else if (TalentUtility.CurrentPlayerHasTalent(talent.MailFlag, ignoreDisabledTalents: true))
                {
                    //bloom
                    button.texture = BundleColor.HasValue
                        ? Tree.BundleIcon
                        : Tree.MainMenu.JunimoNote;
                    button.sourceRect = BoughtRect;
                }
                else if (TalentUtility.CurrentPlayerHasTalent(talent.MailFlag, ignoreDisabledTalents: false))
                {
                    //available - disabled
                    button.texture = BundleColor.HasValue
                        ? Tree.BundleIcon
                        : Tree.MainMenu.JunimoNote;
                    button.sourceRect = AvailableRect;
                }
                else if (Availability)
                {
                    //available
                    button.texture = BundleColor.HasValue
                        ? Tree.BundleIcon
                        : Tree.MainMenu.JunimoNote;

                    button.sourceRect = AvailableRect;
                }
            }
        }
        internal string GetTalentDescription()
        {
            string desc = "";
            if (!Availability)
            {
                if (ModEntry.CoreModEntry.Value.ModConfig.TalentHintLevel == "Hidden")
                {
                    desc = LockedDesc;
                    return Game1.parseText(desc, Game1.smallFont, SpriteText.getWidthOfString(LockedName) + 100);
                }
                else if (ModEntry.CoreModEntry.Value.ModConfig.TalentHintLevel == "Partial")
                {
                    if (talent.Requirements.Length > 0)
                    {
                        foreach (var item in talent.Requirements)
                        {
                            if (TalentCore.Talents.TryGetValue(item, out Talent value) && value is not null)
                            {
                                if (value.Requirements.Length > 0)
                                {
                                    bool skip = false;
                                    foreach (var req2 in value.Requirements)
                                    {
                                        if (TalentCore.Talents.TryGetValue(req2, out Talent value2) && value2 is not null && TalentUtility.CurrentPlayerHasTalent(value2.MailFlag, ignoreDisabledTalents: false))
                                        {
                                            desc = button.hoverText + (Game1.player.mailReceived.Contains(talent.MailFlag + "_disabled") ? Disabled : "");
                                            skip = true;
                                            break;
                                        }
                                    }
                                    if (!skip)
                                    {
                                        desc = LockedDesc;
                                        return Game1.parseText(desc, Game1.smallFont, SpriteText.getWidthOfString(button.name) + 100);
                                    }
                                }
                                else 
                                {
                                    desc = button.hoverText;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        desc = LockedDesc + (talent.AmountToBuyFirst > 0 ? string.Format(NumberLocked, talent.AmountToBuyFirst) : "");
                    }
                }
                else
                {
                    desc = button.hoverText + (Game1.player.mailReceived.Contains(talent.MailFlag + "_disabled") ? Disabled : "");
                }
            }
            else
            {
                desc = button.hoverText + (Game1.player.mailReceived.Contains(talent.MailFlag + "_disabled") ? Disabled : "");
            }
            
            return Game1.parseText(desc, Game1.smallFont, SpriteText.getWidthOfString(button.name) + 100);
        }

        public void GameWindowChanged(int xPos, int yPos)
        {
            XPos = xPos;
            YPos = yPos;
            button.bounds.X = XPos + (int)(talent.Position.X * 4);
            button.bounds.Y = YPos + (int)(talent.Position.Y * 4);
        }
        internal BundleIcon(ClickableTextureComponent component, Talent talentData, int xPos, int yPos)
        {
            XPos = xPos;
            YPos = yPos;
            component.bounds.X += XPos;
            component.bounds.Y += YPos;
            button = component;
            talent = talentData;
            LockedName = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Talent.LockedTalent.Name");
            LockedDesc = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Talent.LockedTalent.Desc");
            NumberLocked = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Talent.LockedTalent.Numbered");
            Disabled = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Talent.DisabledTalent");
        }

        internal void InitializeFully()
        {
            BundleColor = Tree.BundleColor;
            TalentsBought = Tree.TalentsBought;
            LockedRect = Tree.LockedRect;
            BoughtRect = Tree.BoughtRect;
            AvailableRect = Tree.AvailableRect;
        }
    }
}
