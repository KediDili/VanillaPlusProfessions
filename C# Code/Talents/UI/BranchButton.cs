using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace VanillaPlusProfessions.Talents.UI
{
    public class BranchButton
    {
        public Rectangle button;

        public Talent.Branch branch;

        public void draw(SpriteBatch b)
        {
            string name = branch.DisplayName.Invoke(branch.Name);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new(384, 396, 15, 15), button.X + 16, button.Y + SpriteText.characterHeight, SpriteText.getWidthOfString(name) + 32, SpriteText.getHeightOfString(name) + 32, Color.White, 4f, false, 0.1f);
            SpriteText.drawStringHorizontallyCenteredAt(b, name, button.X + (SpriteText.getWidthOfString(name) / 2) + 32, button.Y + SpriteText.getHeightOfString(name) * 2 / 3);
        }
    }
}
