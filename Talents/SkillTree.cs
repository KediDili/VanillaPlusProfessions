using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace VanillaPlusProfessions.Talents
{
    internal class SkillTree
    {
        readonly string SkillIndex;

        readonly string TreeTitle;

        readonly Texture2D Texture;

        internal List<BundleIcon> Bundles = new();

        internal Rectangle? Rectangle;

        internal Rectangle LockedRect;

        public SkillTree(string skillIndex, int bundleID, string treeTitle, Texture2D texture, List<Talent> talents, Rectangle? rectangle, int lockedID)
        {
            if (talents?.Count == 0)
            {
                return;
            }
            foreach (var tal in talents)
            {
                //Because fuck delegates capturing variables.
                Talent talent = tal;
                ClickableTextureComponent button = new(talent.DisplayName is null ? ModEntry.Helper.Translation.Get("Talent." + talent.Name + ".Name") : talent.DisplayName.Invoke(), new((int)(talent.Position.X * 4), ((int)talent.Position.Y * 4), 64, 64), "", talent.Description is null? ModEntry.Helper.Translation.Get("Talent." + talent.Name + ".Desc") : talent.Description.Invoke(), TalentSelectionMenu.JunimoNote, TalentSelectionMenu.getSourceRectByIndex(bundleID), 4f, false);
                Bundles.Add(new(button, talent));
            }
            SkillIndex = skillIndex;
            TreeTitle = treeTitle;
            Texture = texture;
            Rectangle = rectangle;
            LockedRect = TalentSelectionMenu.getSourceRectByIndex(lockedID);
        }

        public void draw(SpriteBatch b)
        {
            SpriteText.drawString(b, TreeTitle, TalentSelectionMenu.XPos + 320 + (SpriteText.getWidthOfString(TreeTitle) / 2), (int)(TalentSelectionMenu.JunimoNote.Height / 1.8), scroll_text_alignment: SpriteText.ScrollTextAlignment.Center);
            b.Draw(Texture, new Vector2(TalentSelectionMenu.XPos, TalentSelectionMenu.YPos), null, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);

            for (int i = 0; i < Bundles.Count; i++)
            {
                if (!Bundles[i].animIsUp)
                    Bundles[i].button.draw(b);
                //else
                    //Bundles[i].animatedSprite.draw(b, true);
            }
            //openingAnim.draw(b, new(button.bounds.X, button.bounds.Y), 0.5f);
        }
    }
}
