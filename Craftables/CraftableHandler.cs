using StardewValley;
using StardewValley.GameData.LocationContexts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using xTile.Dimensions;
using StardewValley.TerrainFeatures;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using System.Xml.Linq;
using xTile;
using StardewValley.Menus;

namespace VanillaPlusProfessions.Craftables
{
    public class CraftableHandler
    {
        internal const string Key_MossyFertilizer = "KediDili.VanillaPlusProfessions/MossyFertilizer";

        internal const string Key_WildTotem = "KediDili.VanillaPlusProfessions/WildTotem";

        internal static void OnDayStarted()
        {
            foreach (var loc in Game1.locations)
            {
                foreach (var feature in loc.terrainFeatures.Values)
                {
                    if (feature is Tree tree && tree.modData.TryGetValue(Key_MossyFertilizer, out string val) && val == "true" && !tree.hasMoss.Value)
                    {
                        if (loc.IsRainingHere() || Game1.random.NextBool(0.70))
                        {
                            tree.hasMoss.Value = true;
                            tree.modData[Key_MossyFertilizer] = "false";
                        }
                    }
                }
            }
        }

        internal static void OnInteract(Farmer who, Item item)
        {
            if (item is StardewValley.Object obj)
            {
                GameLocation location = who.currentLocation;
                string contextId = location.GetLocationContextId();
                LocationContextData context = location.GetLocationContext();
                if (obj.QualifiedItemId == "(O)SnowTotem")
                {
                    TotemAnimation(who, obj, contextId, context, location, "Snow");
                }
                else if (obj.QualifiedItemId == "(O)SunTotem")
                {
                    TotemAnimation(who, obj, contextId, context, location, "Sun");
                }
                else if (obj.QualifiedItemId == "(O)WildTotem")
                {
                    TotemAnimation(who, obj, contextId, context, location, "Wild");
                }
                else if (obj.QualifiedItemId == "(O)MossyFertilizer")
                {
                    if (who.currentLocation.terrainFeatures.TryGetValue(Game1.getMousePosition(false).ToVector2(), out var feature) && feature is Tree tree)
                    {
                        if (!tree.modData.TryGetValue(Key_MossyFertilizer, out string val) || val is not null and "false")
                        {
                            if (!tree.modData.TryAdd(Key_MossyFertilizer, "true"))
                                tree.modData[Key_MossyFertilizer] = "true";
                        }
                        else
                            Game1.pauseThenMessage(1500, ModEntry.Helper.Translation.Get("Message.MossyFertilizer"));
                    }
                }
            }
        }

        internal static void TotemAnimation(Farmer who, StardewValley.Object obj, string ContextId, LocationContextData context, GameLocation location, string effect)
        {
            if (effect != "Wild")
            {
                if (!context.AllowRainTotem)
                {
                    Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
                    return;
                }
                if (context.RainTotemAffectsContext != null)
                {
                    ContextId = context.RainTotemAffectsContext;
                }
                bool applied = false;
                if (ContextId == "Default")
                {
                    if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
                    {
                        Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = effect); //???
                        applied = true;
                    }
                }
                else
                {
                    location.GetWeather().WeatherForTomorrow = effect;
                    applied = true;
                }
                if (applied)
                {
                    Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"));
                }
            }
            Game1.screenGlow = false;
            location.playSound("thunder");
            who.canMove = false;
            Game1.screenGlowOnce(Color.AliceBlue, hold: false);
            Game1.player.faceDirection(2);
            Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
            {
                new(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
            });
            for (int i = 0; i < 6; i++)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2(Game1.random.Next(-10, 11) / 10f, -2f),
                    delayBeforeAnimationStart = i * 200
                });
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2(Game1.random.Next(-30, -10) / 10f, -1f),
                    delayBeforeAnimationStart = 100 + i * 200
                });
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2(Game1.random.Next(10, 30) / 10f, -1f),
                    delayBeforeAnimationStart = 200 + i * 200
                });
            }
            TemporaryAnimatedSprite sprite = new(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
            {
                motion = new Vector2(0f, -7f),
                acceleration = new Vector2(0f, 0.1f),
                scaleChange = 0.015f,
                alpha = 1f,
                alphaFade = 0.0075f,
                shakeIntensity = 1f,
                initialPosition = Game1.player.Position + new Vector2(0f, -96f),
                xPeriodic = true,
                xPeriodicLoopTime = 1000f,
                xPeriodicRange = 4f,
                layerDepth = 1f
            };
            sprite.CopyAppearanceFromItemId(obj.QualifiedItemId);
            Game1.Multiplayer.broadcastSprites(location, sprite);
            DelayedAction.playSoundAfterDelay("rainsound", 2000);
            if (effect == "Wild")
            {
                LocationData data = location.GetData();
                Season season = location.GetSeason();
                int count = 0;
                if (location.GetData().Forage.Count > 0)
                {
                    var farmerTile = who.TilePoint.ToVector2();
                    Vector2[] vectors = new Vector2[]
                    {
                        new(farmerTile.X - 2, farmerTile.Y),
                        new(farmerTile.X + 2, farmerTile.Y),
                        new(farmerTile.X, farmerTile.Y - 2),
                        new(farmerTile.X, farmerTile.Y + 2),
                        new(farmerTile.X - 1, farmerTile.Y - 1),
                        new(farmerTile.X + 1, farmerTile.Y - 1),
                        new(farmerTile.X + 1, farmerTile.Y + 1),
                        new(farmerTile.X - 1, farmerTile.Y + 1),
                    };

                    
                    if (data != null)
                    {
                        for (int i = 0; i < vectors.Length; i++)
                        {
                            Random r = Utility.CreateDaySaveRandom();
                            List<SpawnForageData> possibleForage = new();
                            foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
                            {
                                if ((spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, r)) && (!spawn.Season.HasValue || spawn.Season == season))
                                {
                                    possibleForage.Add(spawn);
                                }
                            }
                            if (possibleForage.Any())
                            {
                                ItemQueryContext itemQueryContext = new(location, null, r, "location '" + location.NameOrUniqueName + "' > forage");

                                if (location.Objects.ContainsKey(vectors[i]) || location.IsNoSpawnTile(vectors[i]) || location.doesTileHaveProperty((int)vectors[i].X, (int)vectors[i].Y, "Spawnable", "Back") == null || location.doesEitherTileOrTileIndexPropertyEqual((int)vectors[i].X, (int)vectors[i].Y, "Spawnable", "Back", "F") || !location.CanItemBePlacedHere(vectors[i]) || location.hasTileAt((int)vectors[i].X, (int)vectors[i].Y, "AlwaysFront") || location.hasTileAt((int)vectors[i].X, (int)vectors[i].Y, "AlwaysFront2") || location.hasTileAt((int)vectors[i].X, (int)vectors[i].Y, "AlwaysFront3") || location.hasTileAt((int)vectors[i].X, (int)vectors[i].Y, "Front") || location.isBehindBush(vectors[i]) || (!r.NextBool(0.1) && location.isBehindTree(vectors[i])))
                                {
                                    continue;
                                }
                                SpawnForageData forage = r.ChooseFrom(possibleForage);
                                if (!r.NextBool(forage.Chance))
                                {
                                    continue;
                                }
                                StardewValley.Object forageItem = ItemQueryResolver.TryResolveRandomItem(forage, itemQueryContext) as StardewValley.Object;
                                if (forageItem == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    forageItem.IsSpawnedObject = true;
                                    if (location.dropObject(forageItem, vectors[i] * 64f, Game1.viewport, initialPlacement: true))
                                        count++;
                                }
                            }
                        }
                    }
                    
                }
                if (count == 0)
                {
                    Game1.activeClickableMenu = new DialogueBox(ModEntry.Helper.Translation.Get("Message.TotemFailed"));
                }
            }
        } 
    }
}
