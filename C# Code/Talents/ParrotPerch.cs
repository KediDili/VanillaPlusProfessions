using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
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
            if (dropInItem.QualifiedItemId is "(TR)ParrotEgg" && lastInputItem.Value is null)
            {
                if (!probe)
                {
                    readyForHarvest.Value = false;
                    lastInputItem.Value = dropInItem as Object;
                    CreateParrotAnimation();
                }
                return true;
            }
            return false;
        }
        private void CreateParrotAnimation()
        {
            ParrotAnim.spriteTexture = ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]);
            ParrotAnim.SourceRect = new(0, 64, 16, 16);
            ParrotAnim.SpriteHeight = 16;
            ParrotAnim.SpriteWidth = 16;
            ParrotAnim.UpdateSourceRect();
        }
        
        public override void performRemoveAction()
        {
            if (lastInputItem.Value is not null)
            {
                Location.debris.Add(new(lastInputItem.Value.getOne(), new(TileLocation.X * 64, TileLocation.Y * 64)));
            }
        }
         
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            base.draw(spriteBatch, x, y, alpha);
            if (lastInputItem.Value is not null)
            {
                ParrotAnim?.draw(spriteBatch, Game1.GlobalToLocal(Game1.uiViewport, new Vector2(TileLocation.X, TileLocation.Y - 1.32f) * 64), System.Math.Max(0f, ((TileLocation.Y + 1) * 64) / 10000f));
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
                    ParrotAnim.Animate(time, 24, 3, 500);
                    Location.playSound("parrot", TileLocation);
                }
                else
                {
                    ParrotAnim.Animate(time, 24, 1, 500);
                }
            }
            else
            {
                ParrotAnim.Animate(time, 24, 2, 500);
            }
        }

        //Remove this
        public override bool performToolAction(Tool t)
        {
            return true;
        }

        public override void DayUpdate()
        {
            GameLocation location = Location;
            health = 10;
            if (location is not AnimalHouse || heldObject.Value is null)
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
