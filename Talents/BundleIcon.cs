using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace VanillaPlusProfessions.Talents
{
    internal class BundleIcon
    {
        internal ClickableTextureComponent button;

        internal Talent talent;

        internal AnimatedSprite openingAnim;

        internal TemporaryAnimatedSprite animatedSprite;

        internal bool animIsUp = false;

        internal bool Availability {
            get
            {
                if (talent.RequiresTalent.Length is 0)
                {
                    return true;
                }
                else
                {
                    if (talent.RequiresTalent2.Length is 0)
                    {
                        return TalentCore.GainedTalents.Value.Contains(talent.RequiresTalent);
                    }
                    else
                    {
                        return TalentCore.GainedTalents.Value.Contains(talent.RequiresTalent) || TalentCore.GainedTalents.Value.Contains(talent.RequiresTalent2);
                    }
                }
            }
        }
        internal BundleIcon(ClickableTextureComponent component, Talent talentData)
        {
            component.bounds.X += TalentSelectionMenu.XPos;
            component.bounds.Y += TalentSelectionMenu.YPos;
            button = component;
            talent = talentData;

            openingAnim = new()
            {
                spriteTexture = component.texture,
                sourceRect = component.sourceRect,
                ignoreSourceRectUpdates = false,
                interval = 200,
                loop = false,
                timer = 3200,
                framesPerAnimation = 16
            };
            openingAnim.sourceRect.X = 0;
            openingAnim.sourceRect.Y = 244;
            openingAnim.sourceRect.Width = 16;
            openingAnim.SpriteHeight = 16;
            openingAnim.SpriteWidth = 16;
            List<FarmerSprite.AnimationFrame> list = new();
            for (int i = 0; i < 16; i++)
            {
                int index = i;
                list.Add(new(index, 200));
            }
            openingAnim.setCurrentAnimation(list);
            if (TalentCore.GainedTalents.Value.Contains(talent.Name))
                button.sourceRect.X += 240;

            animatedSprite = new();
            animatedSprite.acceleration = Vector2.Zero;
            animatedSprite.alpha = 1f;
            animatedSprite.texture = component.texture;
            animatedSprite.position = component.getVector2();
            animatedSprite.local = true;
            animatedSprite.interval = 100;
            animatedSprite.scale = 4f;
            animatedSprite.totalNumberOfLoops = 1;
            animatedSprite.sourceRect.X = 0;
            animatedSprite.sourceRect.Y = 244;
            animatedSprite.sourceRect.Width = 256;
            animatedSprite.sourceRect.Height = 16;
            animatedSprite.animationLength = 16;
        }
    }
}
