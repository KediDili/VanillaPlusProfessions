using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;

namespace VanillaPlusProfessions.Talents
{
    public class VoidButterfly : Butterfly
    {
        float captureTimer = 0f;

        bool startedCapturing = false;

        public VoidButterfly(GameLocation location, Vector2 position, bool islandButterfly) : base(location, position, islandButterfly, true, 62, false)
        {
            editAnim();
        }

        void editAnim()
        {
            sprite.textureName.Value = ContentEditor.ContentPaths["ItemSpritesheet"];
            baseFrame = 62;
            sprite.UpdateSourceRect();
            sprite.loop = false;
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f, 0, 0, Color.White, flip, 4f);
        }

        public override void draw(SpriteBatch b)
        {
            if (sprite != null)
            {
                sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f + position.X / 1000000f, 0, 0, Color.White, flip, 4f);
                b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White * (1f - Math.Min(1f, Math.Abs((yJumpOffset + yOffset) / 64f))), 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 64f), SpriteEffects.None, (position.Y - 1f) / 10000f);
            }
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            if (Utility.distance(position.X, Game1.player.position.X, position.Y, Game1.player.position.Y) < 128f)
            {
                if (captureTimer == 0f && !startedCapturing)
                {
                    captureTimer = 2000f;
                    startedCapturing = true;
                }
                else if (captureTimer > 0f && startedCapturing)
                {
                    captureTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                }
                else if (captureTimer < 0f && startedCapturing)
                {
                    Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (position.Y + 64f - 24f) / 10000f) + base.position.X / 64f * 1E-05f, 0f, Color.Black, 4f, 0f, 0f, 0f)
                    {
                        drawAboveAlwaysFront = true
                    }, environment, 16);
                    BuffEffects buffEffects = new();

                    buffEffects.LuckLevel.Value = 10;

                    Game1.player.buffs.Apply(new("Kedi.VPP.VoidButterfly", "VoidButterfly", "Void Butterfly", -2, Game1.buffsIcons, 5, buffEffects, false, "Luck", "The Void Butterfly's unlikely blessing."));

                    return true;
                }
            }
            else
            {
                startedCapturing = false;
                captureTimer = 0f;
            }
            
            return base.update(time, environment);
        }
    }
}
