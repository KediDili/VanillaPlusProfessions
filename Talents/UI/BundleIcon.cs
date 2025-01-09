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

        internal static string LockedName;

        internal static string LockedDesc;

        internal static string NumberLocked;

       // internal static string Disabled;

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
            Rectangle rect = new();
            if (Game1.activeClickableMenu is TalentSelectionMenu { AnyActiveBranches: true })
            {
                button.visible = false;
                return;
            }
            if (!Availability)
            {
                //locked
                rect = LockedRect;
                button.texture = TalentSelectionMenu.BundleIcon;
                button.sourceRect = rect;
            }
            else if (TalentUtility.CurrentPlayerHasTalent(talent.MailFlag, ignoreDisabledTalents: true))
            {
                //bloom
                button.texture = BundleColor.HasValue
                    ? TalentSelectionMenu.BundleIcon
                    : TalentSelectionMenu.JunimoNote;
                rect = BoughtRect;
            }
            else if (TalentUtility.CurrentPlayerHasTalent(talent.MailFlag, ignoreDisabledTalents: false))
            {
                //available - disabled
                button.texture = BundleColor.HasValue
                    ? TalentSelectionMenu.BundleIcon
                    : TalentSelectionMenu.JunimoNote;
                rect = AvailableRect;
            }
            else if (Availability)
            {
                //available
                button.texture = BundleColor.HasValue
                    ? TalentSelectionMenu.BundleIcon
                    : TalentSelectionMenu.JunimoNote;

                rect = AvailableRect;
            }
            button.sourceRect = rect;
        }
        internal string GetTalentDescription()
        {
            string desc = "";
            if (!Availability)
            {
                if (ModEntry.ModConfig.Value.TalentHintLevel == "Hidden")
                {
                    desc = LockedDesc;
                    return Game1.parseText(desc, Game1.smallFont, SpriteText.getWidthOfString(LockedName) + 100);
                }
                else if (ModEntry.ModConfig.Value.TalentHintLevel == "Partial")
                {
                    if (talent.Requirements.Length > 0)
                    {
                        foreach (var item in talent.Requirements)
                        {
                            if (TalentCore.Talents.TryGetValue(item, out Talent value) && value is not null)
                            {
                                if (value.Requirements.Length > 0)
                                {
                                    foreach (var req2 in value.Requirements)
                                    {
                                        if (TalentCore.Talents.TryGetValue(req2, out Talent value2) && value2 is not null && TalentUtility.CurrentPlayerHasTalent(value2.MailFlag, ignoreDisabledTalents: false))
                                        {
                                            desc = button.hoverText + (TalentCore.DisabledTalents.Contains(talent.MailFlag) ? ModEntry.Helper.Translation.Get("Talent.DisabledTalent") : "");
                                            break;
                                        }
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
                        desc = button.hoverText;
                        return desc;
                    }
                    desc = LockedDesc + (talent.AmountToBuyFirst > 0 ? string.Format(NumberLocked, talent.AmountToBuyFirst) : "");
                }
            }
            desc = button.hoverText + (TalentCore.DisabledTalents.Contains(talent.MailFlag) ? " (Disabled. Click to re-enable.)" : "");
            return Game1.parseText(desc, Game1.smallFont, SpriteText.getWidthOfString(button.name) + 100);
        }

        public void GameWindowChanged()
        {
            button.bounds.X = TalentSelectionMenu.XPos + (int)(talent.Position.X * 4);
            button.bounds.Y = TalentSelectionMenu.YPos + (int)(talent.Position.Y * 4);
        }
        internal BundleIcon(ClickableTextureComponent component, Talent talentData)
        {
            component.bounds.X += TalentSelectionMenu.XPos;
            component.bounds.Y += TalentSelectionMenu.YPos;
            button = component;
            talent = talentData;
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
