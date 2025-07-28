using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;

namespace VanillaPlusProfessions.Craftables
{
    public class Parrot : Critter
    {
        public const int FullUpdateTimer = 200;

        public int updateTimer;

        public int jumpTimer;

        public float resetTimer;

        public float shakeTimer;

        public float squawkTimer;

        public Vector2 shake = Vector2.Zero;

        public Parrot(Vector2 position, bool isGold)
        {
            baseFrame = isGold ? 44 : Game1.random.Next(0, 4) * 11;
            this.position = position;
            sprite = new AnimatedSprite("LooseSprites\\parrots", baseFrame, 24, 24);
            startingPosition = position;
        }

        public override void draw(SpriteBatch b)
        {
            if (sprite != null)
            {
                sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-48f + shake.X, -84f + yJumpOffset + yOffset + shake.Y)), position.Y / 10000f + position.X / 1000000f, 0, 0, Color.White, flip, 4f);
                b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White * (1f - Math.Min(1f, Math.Abs((yJumpOffset + yOffset) / 64f))), 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 64f), SpriteEffects.None, (position.Y - 1f) / 10000f);
            }
        }
        public void hop()
        {
            gravityAffectedDY = -3f;
        }

        public void Squawk(Farmer who)
        {
            Game1.playSound("parrot");
            shakeTimer = 250;
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            updateTimer -= time.ElapsedGameTime.Milliseconds;
            jumpTimer -= time.ElapsedGameTime.Milliseconds;
            shakeTimer -= time.ElapsedGameTime.Milliseconds;

            if (gravityAffectedDY < 0)
            {
                position.X += flip ? -gravityAffectedDY : gravityAffectedDY;
            }
            if (updateTimer <= 0)
            {
                updateTimer = FullUpdateTimer;
                if (Game1.random.NextBool(0.02) && jumpTimer <= 0)
                {
                    sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new(baseFrame, 100, 0, flip: flip),
                        new(baseFrame + 1, 250, 0, false, flip: flip, frameBehavior: who => Squawk(who)),
                        new(baseFrame, 100, 0, flip: flip),
                    });
                    sprite.loop = false;
                    resetTimer = 501;
                }
                else if (Game1.random.NextBool(0.025) && shakeTimer <= 0)
                {
                    flip = !flip;
                }
                else if (Game1.random.NextBool(0.01) && jumpTimer <= 0 && shakeTimer <= 0)
                {
                    jumpTimer = FullUpdateTimer;
                    if (gravityAffectedDY >= 0)
                    {
                        hop();
                    }
                }
            }
            if (shakeTimer <= 0)
            {
                shake = Vector2.Zero;
                resetTimer -= time.ElapsedGameTime.Milliseconds;
                if (resetTimer <= 0)
                {
                    sprite.CurrentAnimation = null;
                    sprite.currentFrame = baseFrame;
                }
            }
            else
            {
                shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
                shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
            }
            
            return base.update(time, environment);
        }
    }
}
