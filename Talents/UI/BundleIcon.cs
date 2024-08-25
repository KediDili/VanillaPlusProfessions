using Microsoft.Xna.Framework;
using StardewValley;
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
                        if (TalentUtility.CurrentPlayerHasTalent(TalentCore.Talents[talent.Requirements[i]].MailFlag))
                            return true;
                    return false;
                }
            }
        }
        internal void UpdateSprite()
        {
            InitializeFully();
            if (Game1.activeClickableMenu is TalentSelectionMenu { AnyActiveBranches: true })
            {
                button.visible = false;
                return;
            }
            if (!Availability)
            {
                //locked
                Rectangle rect = LockedRect;
                button.texture = TalentSelectionMenu.BundleIcon;
                button.sourceRect = rect;
            }
            else if (TalentUtility.CurrentPlayerHasTalent(talent.MailFlag))
            {
                //bloom
                button.texture = BundleColor.HasValue
                    ? TalentSelectionMenu.BundleIcon
                    : TalentSelectionMenu.JunimoNote;

                Rectangle rect = BoughtRect;
                button.sourceRect = rect;
            }
            else if (Availability)
            {
                //available
                button.texture = BundleColor.HasValue
                    ? TalentSelectionMenu.BundleIcon
                    : TalentSelectionMenu.JunimoNote;

                Rectangle rect =AvailableRect;
                button.sourceRect = rect;
            }
        }
        internal string GetTalentDescription()
        {
            if (!Availability)
            {
                if (ModEntry.ModConfig.Value.TalentHintLevel == "Hidden")
                {
                    return LockedDesc;
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
                                        if (TalentCore.Talents.TryGetValue(req2, out Talent value2) && value2 is not null && TalentUtility.CurrentPlayerHasTalent(value2.MailFlag))
                                        {
                                            return button.hoverText;
                                        }
                                    }
                                }
                                else
                                {
                                    return button.hoverText;
                                }
                            }
                        }
                    }
                    else
                    {
                        return button.hoverText;
                    }
                    return LockedDesc + (talent.AmountToBuyFirst > 0 ? string.Format(NumberLocked, talent.AmountToBuyFirst) : "");
                }
            }
            return button.hoverText;
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
