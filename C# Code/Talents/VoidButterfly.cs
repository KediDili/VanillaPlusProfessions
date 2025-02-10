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

        public VoidButterfly(GameLocation location, Vector2 position) : base(location, position, false, false, -1, false)
        {
            sprite.spriteTexture = null;
            sprite.sourceRect = new(0, 0, 16, 16);
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f, 0, 0, Color.DarkGray * 0.8f, flip, 4f);
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
                    Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, base.position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (base.position.Y + 64f - 24f) / 10000f) + base.position.X / 64f * 1E-05f, 0f, Color.Black, 4f, 0f, 0f, 0f)
                    {
                        drawAboveAlwaysFront = true
                    }, environment, 16);
                    BuffEffects buffEffects = new();

                    buffEffects.LuckLevel.Value = 10;

                    Game1.player.buffs.Apply(new("Kedi.VPP.VoidButterfly", "VoidButterfly", "Void Butterfly", -2, Game1.buffsIcons, 20, buffEffects, false, "Luck", "The Void Butterfly's unlikely blessing."));

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
