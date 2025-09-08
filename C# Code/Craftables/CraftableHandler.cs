using StardewValley;
using StardewValley.GameData.LocationContexts;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using StardewValley.TerrainFeatures;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Menus;
using VanillaPlusProfessions.Utilities;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;

namespace VanillaPlusProfessions.Craftables
{
    public class CraftableHandler
    {
        internal static void OnDayStarted()
        {
            foreach (var loc in Game1.locations)
            {
                foreach (var feature in loc.terrainFeatures.Values)
                {
                    if (feature is Tree tree && tree.modData.TryGetValue(Constants.Key_MossyFertilizer, out string val) && val == "true" && !tree.hasMoss.Value)
                    {
                        if (loc.IsRainingHere() || Game1.random.NextBool(0.70))
                        {
                            tree.hasMoss.Value = true;
                            tree.modData[Constants.Key_MossyFertilizer] = "false";
                        }
                    }
                }
            }
        }

        internal static void OnInteract(Farmer who, Item item)
        {
            if (ModEntry.ShouldForageCraftablesWork() && item is StardewValley.Object obj)
            {
                GameLocation location = who.currentLocation;
                string contextId = location.GetLocationContextId();
                LocationContextData context = location.GetLocationContext();
                if (obj.ItemId == Constants.Id_SnowTotem)
                {
                    TotemAnimation(who, obj, contextId, context, location, "Snow");
                }
                else if (obj.ItemId == Constants.Id_SunTotem)
                {
                    TotemAnimation(who, obj, contextId, context, location, "Sun");
                }
                else if (obj.ItemId == Constants.Id_WildTotem)
                {
                    TotemAnimation(who, obj, contextId, context, location, "Wild");
                }
                else if (obj.ItemId == Constants.Id_MossyFertilizer)
                {
                    if (who.currentLocation.terrainFeatures.TryGetValue(ModEntry.CoreModEntry.Value.Helper.Input.GetCursorPosition().GrabTile, out var feature) && feature is Tree tree)
                    {
                        if (!tree.modData.TryGetValue(Constants.Key_MossyFertilizer, out string val) || val is not null and "false")
                        {
                            if (!tree.modData.TryAdd(Constants.Key_MossyFertilizer, "true"))
                                tree.modData[Constants.Key_MossyFertilizer] = "true";
                            item.ConsumeStack(1);
                        }
                        else
                            Game1.pauseThenMessage(250, ModEntry.CoreModEntry.Value.Helper.Translation.Get("Message.MossyFertilizer"));
                    }
                }
                else if (obj.ItemId == Constants.Id_NodeLifter)
                {
                    Vector2 tile = ModEntry.CoreModEntry.Value.Helper.Input.GetCursorPosition().GrabTile;
                    if (who.currentLocation.Objects.TryGetValue(tile, out var obj2) && obj2.Category == StardewValley.Object.litterCategory)
                    {
                        if (Utility.canItemBeAddedToThisInventoryList(obj2, Game1.player.Items))
                        {
                            Game1.player.addItemByMenuIfNecessary(obj2);
                            who.currentLocation.Objects.Remove(tile);
                            obj = obj.ConsumeStack(1) as StardewValley.Object;
                        }
                    }
                }
            }
        }
        internal static string GetSuccessString(Season season, string effect)
        {
            if (effect == "Sun")
            {
                return $"Message.SunTotemSuccess.{(season is Season.Summer ? "Summer" : "Normal")}";
            }
            else if (effect == "Snow")
            {
                return $"Message.SnowTotemSuccess.{(season is Season.Winter ? "Winter" : "Normal")}";
            }
            return "Message.NotFound";
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
                    if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season) && Game1.netWorldState.Value.WeatherForTomorrow != effect)
                    {
                        Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = effect); //???
                        applied = true;
                    }
                }
                else if (location.GetWeather().WeatherForTomorrow != effect)
                {
                    location.GetWeather().WeatherForTomorrow = effect;
                    applied = true;
                }
                if (applied)
                {
                    Game1.pauseThenMessage(2000, ModEntry.CoreModEntry.Value.Helper.Translation.Get(GetSuccessString(location.GetSeason(), effect)));
                    obj.ConsumeStack(1);
                }
            }
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
            Game1.screenGlow = false;
            location.playSound("thunder");
            who.canMove = false;
            Game1.screenGlowOnce(Color.AliceBlue, hold: false);
            who.faceDirection(2);
            who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
            {
                new(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
            });
            
            switch (effect)
            {
                case "Sun":
                    TemporaryAnimatedSprite TotemSprite1 = new(0, 50f, 1, 60, Game1.player.Position + new Vector2(-16f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -7f),
                        acceleration = new Vector2(0f, 0.15f),
                        scaleChange = 0.005f,
                        stopAcceleratingWhenVelocityIsZero = true,
                        alpha = 1f,
                        alphaFade = 0.00085f,
                        shakeIntensity = 0.1f,
                        initialPosition = Game1.player.Position + new Vector2(-16f, -96f),
                        xPeriodic = true,
                        xPeriodicLoopTime = 800f,
                        xPeriodicRange = 5f,
                        layerDepth = 1f,
                    };
                    TotemSprite1.CopyAppearanceFromItemId(obj.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, TotemSprite1);

                    TemporaryAnimatedSprite TotemSprite11 = new(0, 80f, 1, 100, Game1.player.Position + new Vector2(-16f, -262f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        alpha = 1f,
                        shakeIntensity = 0.1f,
                        alphaFade = 0.004f,
                        initialPosition = Game1.player.Position + new Vector2(-16f, -262f),
                        layerDepth = 1f,
                        xPeriodic = true,
                        xPeriodicLoopTime = 800f,
                        xPeriodicRange = 5f,
                        scale = 1.2f,
                        scaleChange = 0.0005f,
                        delayBeforeAnimationStart = 2400,
                        drawAboveAlwaysFront = true
                    };
                    
                    TotemSprite11.CopyAppearanceFromItemId(obj.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, TotemSprite11);

                    for (int i = 0; i < Game1.random.Next(5, 11); i++)
                    {
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets//Projectiles", new(48, 16, 16, 16), 9999f, 1, 0, who.Position + new Vector2(0f, -244f), flicker: false, flipped: false, 1.1f, 0.00085f, Color.White, 4f, 0f, 0f, 0f)
                        {
                            delayBeforeAnimationStart = 3000 + i * 150,
                            motion= new Vector2((Game1.random.NextSingle() + Game1.random.NextSingle() + Game1.random.NextSingle()) * (Game1.random.NextBool() ? 1 : -1), -Game1.random.NextSingle()),
                        });
                    }
                    DelayedAction.playSoundAfterDelay("rainsound", 2000);
                    break;
                case "Snow":
                    TemporaryAnimatedSprite TotemSprite2 = new(0, 50f, 1, 65, Game1.player.Position + new Vector2(-16f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -7f), acceleration = new Vector2(0f, 0.15f), scaleChange = 0.005f,
                        stopAcceleratingWhenVelocityIsZero = true, alpha = 1f, alphaFade = 0.00085f,
                        shakeIntensity = 0.1f, initialPosition = Game1.player.Position + new Vector2(-16f, -96f),
                        xPeriodic = true, xPeriodicLoopTime = 800f, xPeriodicRange = 5f, layerDepth = 1f,
                    };
                    TotemSprite2.CopyAppearanceFromItemId(obj.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, TotemSprite2);

                    TemporaryAnimatedSprite TotemSprite22 = new(0, 9999f, 1, 9, Game1.player.Position + new Vector2(+16, -262f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        alpha = 1f, shakeIntensity = 0.1f, alphaFade = 0.004f,
                        initialPosition = Game1.player.Position + new Vector2(-16f, -262f),
                        layerDepth = 1f, xPeriodic = true, xPeriodicLoopTime = 800f, xPeriodicRange = 5f,
                        scale = 1.2f, scaleChange = 0.0005f, delayBeforeAnimationStart = 2400, drawAboveAlwaysFront = true
                    };
                    TotemSprite22.CopyAppearanceFromItemId(obj.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, TotemSprite22);
                    
                    for (int i = 0; i < 10; i++)
                    {
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(ContentEditor.ContentPaths["Animations"], new(16 + Game1.random.Choose(0, 16, 32, 48), 32 + Game1.random.Choose(0, 16), 16, 16), 9999f, 1, 0, who.Position + new Vector2(0f, -244f), flicker: false, flipped: false, 1.1f, 0.00085f, Color.White, 4f, 0f, 0f, (float)(System.Math.PI / 180))
                        {
                            motion = new Vector2((Game1.random.NextSingle() + Game1.random.NextSingle() + Game1.random.NextSingle()) * (Game1.random.NextBool() ? 1 : -1), -Game1.random.NextSingle()),
                            delayBeforeAnimationStart = 3000 + i * 150
                        });
                    }

                    DelayedAction.playSoundAfterDelay("rainsound", 1000);
                    break;
                case "Wild":
                    TemporaryAnimatedSprite TotemSprite3 = new(0, 50f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                    {
                        motion = new Vector2(0f, -4f), acceleration = new Vector2(0f, 0.05f), scaleChange = 0.005f,
                        alpha = 1f, alphaFade = 0.0075f, shakeIntensity = 1f,
                        initialPosition = Game1.player.Position + new Vector2(0f, -96f),
                        xPeriodic = true, xPeriodicLoopTime = 1000f, xPeriodicRange = 4f, layerDepth = 1f,
                    };
                    TotemSprite3.CopyAppearanceFromItemId(obj.QualifiedItemId);
                    Game1.Multiplayer.broadcastSprites(location, TotemSprite3);
                    DelayedAction.playSoundAfterDelay("rainsound", 2000);

                    if (!location.IsRainingHere() && !location.IsSnowingHere())
                    {
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new(648, 1045, 52, 33), 1210f, 1, 1, who.Position + new Vector2(0f, -192f), flicker: false, flipped: false, 2f, 0f, Color.White, 1f, 0.07f, 0f, 0f)
                        {
                            motion = new(-1.72f, -2.60f),
                            delayBeforeAnimationStart = 1500,
                        });
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new(648, 1045, 52, 33), 9999f, 1, 1, who.Position + new Vector2(-128f, -384f), flicker: false, flipped: false, 2f, 0.0015f, Color.White, 6.2f, 0f, 0f, 0f)
                        {
                            delayBeforeAnimationStart = 2710,
                        });
                        for (int i = 0; i < 20; i++)
                        {
                            //RAINDROPS
                            int delay = Game1.random.Next(200, 401);
                            Vector2 tile = Game1.random.ChooseFrom(vectors);
                            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(ContentEditor.ContentPaths["Animations"], new(16, 16, 16, 16), 300, 1, 0, tile * 64 - new Vector2(0f, 128f), flicker: false, flipped: false, tile.Y / 149f, 0f, Color.White, 4f, 0f, 0f, 0f)
                            {
                                motion = new Vector2(0f, 5f),
                                delayBeforeAnimationStart = 1000 + (i * delay),
                            });
                            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(ContentEditor.ContentPaths["Animations"], new(32, 16, 16, 16), 250, 3, 0, tile * 64, flicker: false, flipped: false, tile.Y / 149f, 0f, Color.White, 4f, 0f, 0f, 0f)
                            {
                                delayBeforeAnimationStart = 1000 + (i * delay) + 250
                            });
                        }
                    }

                    //PLANTS
                    for (int i = 0; i < vectors.Length; i++)
                    {
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(ContentEditor.ContentPaths["Animations"], new(0, 0, 16, 16), 200, 5, 0, vectors[i] * 64, flicker: false, flipped: false, vectors[i].Y / 149f, 0f, Color.White, 4f, 0f, 0f, 0f)
                        {
                            delayBeforeAnimationStart = 2500 + (i * Game1.random.Next(100, 200)),
                            extraInfoForEndBehavior = i,
                            endFunction = info => ActivateWildTotem(info)
                        });
                    }

                    break;
            }
        }
        internal static void ActivateWildTotem(int extraInfo)
        {
            if (extraInfo is not 7)
            {
                return;
            }
            GameLocation location = Game1.player.currentLocation;
            LocationData data = location.GetData();
            Season season = location.GetSeason();
            int count = 0;
            var farmerTile = Game1.player.TilePoint.ToVector2();
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

            if (location.GetData().Forage.Count > 0)
            {
                if (data != null)
                {
                    for (int i = 0; i < vectors.Length; i++)
                    {
                        List<SpawnForageData> possibleForage = new();
                        ItemQueryContext itemQueryContext = new(location, null, Game1.random, "location '" + location.NameOrUniqueName + "' > forage");
                        foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
                        {
                            if ((spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, Game1.random)) && (!spawn.Season.HasValue || spawn.Season == season))
                            {
                                if (TalentUtility.EligibleForForagePerks(ItemQueryResolver.TryResolveRandomItem(spawn, itemQueryContext)?.ItemId, Constants.Id_WildTotem))
                                    possibleForage.Add(spawn);
                            }
                        }
                        if (possibleForage.Any())
                        {
                            if (location.Objects.ContainsKey(vectors[i]) || location.IsNoSpawnTile(vectors[i]) || !location.CanItemBePlacedHere(vectors[i]))
                            {
                                continue;
                            }
                            SpawnForageData forage = Game1.random.ChooseFrom(possibleForage);
                            if (ItemQueryResolver.TryResolveRandomItem(forage, itemQueryContext) is not StardewValley.Object forageItem)
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
                Game1.activeClickableMenu = new DialogueBox(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Message.TotemFailed"));
            }
            else
            {
                Game1.player.ActiveObject = Game1.player.ActiveObject.ConsumeStack(1) as StardewValley.Object;
            }
        }
    }
}
