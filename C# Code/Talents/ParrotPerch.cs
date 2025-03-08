using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects.Trinkets;
using StardewValley.Tools;
using System.Xml.Serialization;

namespace VanillaPlusProfessions.Talents
{
    [XmlType("Mods_Kedi.VPP.ParrotPerch")]
    public class ParrotPerch : Object
    {
        AnimatedSprite ParrotAnim = null;

        float interval = 10000f;

        public ParrotPerch()
        {
            ParrotAnim = new();
            ParrotAnim.CurrentAnimation = new();
            //add frames.
            CreateParrotAnimation();
        }

        public ParrotPerch(Vector2 tileLocation, string itemId, bool isRecipe) : base(tileLocation, itemId, isRecipe)
        {
            ParrotAnim = new();
            ParrotAnim.CurrentAnimation = new();
            //add frames.
            CreateParrotAnimation();
            var obj = ItemRegistry.Create(itemId, 1, 0, false);
            this.itemId.Value = itemId;
            name = obj.Name;
            bigCraftable.Value = true;
            
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
        {
            if (returnFalseIfItemConsumed)
            {
                return dropInItem.QualifiedItemId is "(TR)ParrotEgg" && heldObject.Value is null;
            }
            else
            {
                if (dropInItem is Trinket trinket && trinket.QualifiedItemId is "(TR)ParrotEgg" && heldObject.Value is null)
                {
                    heldObject.Value = trinket;
                    dropInItem.ConsumeStack(1);
                    CreateParrotAnimation();
                    return true;
                }
                else if (heldObject.Value is not null)
                {
                    who.addItemByMenuIfNecessary(heldObject.Value);
                    heldObject.Value = null;
                }
            }
            return false;
        }
        public override bool performDropDownAction(Farmer who)
        {
            if (Game1.player.currentLocation is AnimalHouse)
            {
                return false;
            }
            else
            {
                Game1.addHUDMessage(new(ModEntry.Helper.Translation.Get("Message.ParrotPerch"), HUDMessage.error_type));
                return false;
            }
            return false;
        }
        private void CreateParrotAnimation()
        {
            if (ParrotAnim is null)
            {
                ParrotAnim = new();
                ParrotAnim.spriteTexture = ModEntry.Helper.GameContent.Load<Texture2D>("TileSheets/Kedi.VPP.Items");
                ParrotAnim.SourceRect = new(64, 0, 48, 16);
                ParrotAnim.CurrentAnimation = new() { new(0, 150), new(2, 150) };
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            base.draw(spriteBatch, x, y, alpha);
            if (lastInputItem.Value is not null)
            {
                if (ParrotAnim is null)
                {
                    ParrotAnim.draw(spriteBatch, new(x / 64, (y / 64) - 6), 0.5f);
                }
                else
                {
                    CreateParrotAnimation();
                }
            }
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            interval -= time.ElapsedGameTime.Milliseconds;
            if (interval < 1)
            {
                if (Game1.timeOfDay < 2000)
                {
                    interval = 10000f;
                    ParrotAnim.animateOnce(time);
                    Location.playSound("parrot", TileLocation);
                }
                else if (ParrotAnim.CurrentAnimation.Count is 2)
                {
                    ParrotAnim.setCurrentAnimation(new() { new(1, 150) });
                }
            }
        }

        public override bool performToolAction(Tool t)
        {
            if (t.isHeavyHitter() && t is not MeleeWeapon)
            {
                if (lastInputItem.Value is not null)
                {
                    Game1.createItemDebris(lastInputItem.Value, TileLocation * 64, 0, Location);
                }
                else
                {
                    Game1.createObjectDebris(QualifiedItemId, (int)TileLocation.X, (int)TileLocation.Y, Location);
                }
                return false;
            }
            return true;
        }

        public override void DayUpdate()
        {
            GameLocation location = Location;
            health = 10;
            if (location is not AnimalHouse || lastInputItem.Value is null)
            {
                return;
            }
            else if (location is AnimalHouse animalHouse && animalHouse.animalsThatLiveHere.Count == 0)
            {
                return;
            }
            AnimalHouse animalHouse2 = location as AnimalHouse;
            foreach (var item in animalHouse2.Animals.Pairs)
            {
                item.Value.pet(Game1.player, true);
            }
        }
    }
}
