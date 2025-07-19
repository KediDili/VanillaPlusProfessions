using HarmonyLib;
using System;
using System.Collections.Generic;
using StardewValley.Tools;
using StardewValley;
using StardewValley.Buffs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;
using VanillaPlusProfessions.Talents.Patchers;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Utilities;
using StardewValley.Locations;
using StardewValley.Objects.Trinkets;

namespace VanillaPlusProfessions.Managers
{
    public class ComboManager : IProfessionManager
    {
        public int SkillValue => 9;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        readonly static string PatcherName = nameof(ComboManager);
        readonly static System.Type PatcherType = typeof(ComboManager);

        public void ApplyPatches()
        {
            Type[] tools = { typeof(FishingRod), typeof(Slingshot) };

            for (int i = 0; i < tools.Length; i++)
            {
                CoreUtility.PatchMethod(
                    PatcherName, tools[i].Name + ".canThisBeAttached",
                    original: AccessTools.Method(tools[i], "canThisBeAttached", new Type[] { typeof(StardewValley.Object), typeof(int) }),
                    postfix: new HarmonyMethod(PatcherType, "canThisBeAttached_" + tools[i].Name + "_Postfix")
                );
            }
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.OnStoneDestroyed",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed)),
                postfix: new HarmonyMethod(PatcherType, nameof(OnStoneDestroyed_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.DrawIconBar",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DrawIconBar), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color) }),
                postfix: new HarmonyMethod(PatcherType, nameof(DrawIconBar_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.maximumStackSize",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize)),
                postfix: new HarmonyMethod(PatcherType, nameof(maximumStackSize_Postfix))
            );
            CoreUtility.PatchMethod(
                 PatcherName, "FishingRod.GetTackleQualifiedItemIDs",
                 original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.GetTackleQualifiedItemIDs)),
                 postfix: new HarmonyMethod(PatcherType, nameof(GetTackleQualifiedItemIDs_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Slingshot.GetAmmoDamage",
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.GetAmmoDamage)),
                postfix: new HarmonyMethod(PatcherType, nameof(GetAmmoDamage_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Slingshot.GetAmmoCollisionSound",
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.GetAmmoCollisionSound)),
                postfix: new HarmonyMethod(PatcherType, nameof(GetAmmoCollisionSound_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.OutputGeodeCrusher",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputGeodeCrusher)),
                postfix: new HarmonyMethod(PatcherType, nameof(OutputGeodeCrusher_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.sinkDebris",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
                postfix: new HarmonyMethod(PatcherType, nameof(sinkDebris_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "FruitTree.draw",
                original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.draw)),
                postfix: new HarmonyMethod(PatcherType, nameof(FruitTree_Draw_Postfix))
            );
        }

        internal static PerScreen<int> StonesBroken = new();

        public static void sinkDebris_Postfix(Debris debris, GameLocation __instance, bool __result)
        {
            try
            {
                if (__result is true && CoreUtility.AnyPlayerHasProfession("Forage-Fish") && debris.item is not null && debris.item.HasContextTag("category_forage") && Game1.player.modData.TryGetValue(ModEntry.Key_HasFoundForage, out var str))
                {
                    if (str == "false")
                    {
                        if (Game1.player.modData[ModEntry.Key_ForageGuessItemID] == debris.item.QualifiedItemId)
                        {
                            var list = Game1.getOnlineFarmers();

                            foreach (var item in list)
                                item.modData[ModEntry.Key_HasFoundForage] = "true";
                            Game1.addHUDMessage(new($"You threw in the correct forage! You can start making bubbles appear with {debris.item.DisplayName} when you throw it in the water, and there isn't a bubble in the map already.", 1));
                        }
                        else
                        {
                            Game1.addHUDMessage(new($"Sorry, {debris.item.DisplayName} isn't the correct forage to throw in. Try again with another!", 1));
                        }
                    }
                    if (__instance.fishSplashPoint.Value == Point.Zero && Game1.player.modData[ModEntry.Key_HasFoundForage] is "true")
                    {
                        List<Point> list = new();
                        for (int x = 0; x < __instance.waterTiles.waterTiles.GetLength(0); x++)
                            for (int y = 0; y < __instance.waterTiles.waterTiles.GetLength(1); y++)
                                if (__instance.waterTiles.waterTiles[x, y].isWater && __instance.waterTiles.waterTiles[x, y].isVisible)
                                    list.Add(new(x, y));

                        __instance.fishSplashPoint.Value = Game1.random.ChooseFrom(list);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.sinkDebris", "postfixed", true);
            }
        }

        public static void FruitTree_Draw_Postfix(FruitTree __instance, SpriteBatch spriteBatch)
        {
            try
            {
                if (__instance is not null)
                {
                    if (!__instance.stump.Value && __instance.modData?.TryGetValue(ModEntry.Key_TFHasTapper, out string value) is true && value is "true")
                    {
                        var metaData = ItemRegistry.GetData(__instance.modData[ModEntry.Key_TFTapperID]);
                        Rectangle rect = metaData.GetSourceRect();
                        rect.Height = 16;
                        spriteBatch.Draw(metaData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.Tile.X * 64f, (__instance.Tile.Y - 1) * 64f)), rect, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, __instance.getBoundingBox().Bottom / 10000f + 0.001f);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FruitTree.draw", "postfixed", true);
            }
        }

        public static void OutputGeodeCrusher_Postfix(StardewValley.Object machine, Item inputItem, bool probe, ref Item __result)
        {
            try
            {
                if ((inputItem.HasContextTag("category_minerals") || inputItem.HasContextTag("category_gem")) && inputItem is StardewValley.Object obj && CoreUtility.CurrentPlayerHasProfession("Farm-Mine"))
                {
                    var oneofObj = obj.getOne();
                    oneofObj.modData.TryAdd("Kedi.VPP.CurrentPreserveType", "Kedi.VPP.GemDust");
                    GameLocation location;
                    location = machine.Location;
                    Vector2 pixelPos;
                    pixelPos = machine.TileLocation * 64f;
                    if (!probe)
                    {
                        Utility.addSmokePuff(location, pixelPos + new Vector2(4f, -48f), 200);
                        Utility.addSmokePuff(location, pixelPos + new Vector2(-16f, -56f), 300);
                        Utility.addSmokePuff(location, pixelPos + new Vector2(16f, -52f), 400);
                        Utility.addSmokePuff(location, pixelPos + new Vector2(32f, -56f), 200);
                        Utility.addSmokePuff(location, pixelPos + new Vector2(40f, -44f), 500);
                    }
                    __result = ManagerUtility.CreateFlavoredSyrupOrDust(oneofObj as StardewValley.Object);
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.OutputGeodeCrusher", "postfixed", true);
            }
        }
        public static void maximumStackSize_Postfix(StardewValley.Object __instance, ref int __result)
        {
            try
            {
                if (__instance.HasContextTag("category_gem") && __instance.uses.Value > 0)
                    __result = 1;
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.maximumStackSize", "postfixed", true);
            }
        }
        public static void GetTackleQualifiedItemIDs_Postfix(ref List<string> __result, FishingRod __instance)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession("Fish-Mine"))
                {
                    if (__result.Count > 0 && __instance.CanUseTackle())
                    {
                        for (int i = 0; i < __result.Count; i++)
                        {
                            __result[i] = __result[i] switch
                            {
                                "(O)74" => "(O)856", //Pris shard -> Curiosity lure
                                "(O)72" => "(O)877", //Diamond -> Quality bobber
                                "(O)64" => "(O)693", //Ruby -> treasure hunter
                                "(O)60" => "(O)691", //Emerald -> barbed hook
                                "(O)70" => "(O)694", //Jade -> Trap bobber
                                "(O)62" => "(O)692", //Aquamarine -> lead bobber
                                "(O)66" => "(O)687", //Amethyst -> dressed spinner
                                "(O)68" => "(O)695", //Topaz -> cork bobber
                                "(O)82" => "(O)Kedi.VPP.SnailTackle", //Fire Quartz -> Snail Tackle
                                _ => __result[i]
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FishingRod.GetTackleQualifiedItemIDs", "postfixed", true);
            }
        }

        public static void DrawIconBar_Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize)
        {
            try
            {
                if (__instance.HasContextTag("category_gem") && __instance.uses.Value > 0)
                {
                    float health;
                    health = (FishingRod.maxTackleUses - __instance.uses.Value) / FishingRod.maxTackleUses;
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), StardewValley.Utility.getRedToGreenLerpColor(health));
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.DrawIconBar", "postfixed", true);
            }
        }

        public static void OnStoneDestroyed_Postfix(GameLocation __instance, Farmer who, int x, int y)
        {
            try
            {
                if (who is null || who != Game1.player)
                    return;

                if (CoreUtility.CurrentPlayerHasProfession("Mine-Combat", useThisInstead: who))
                {
                    StonesBroken.Value++;
                }
                if (StonesBroken.Value / 100 > 0 && StonesBroken.Value % 100 == 0 && !Game1.doesHUDMessageExist(ModEntry.Helper.Translation.Get("Message.StoneBroken", new { Stones = StonesBroken.Value - 1, Buff = StonesBroken.Value / 100 })))
                {
                    BuffEffects buffEffects = new();
                    buffEffects.Attack.Value = StonesBroken.Value / 100;
                    who.buffs.Apply(new("VPP.Mining-Combat.Attack", "VPP.CombatMining.ComboProfession", "Mining-Combat", Buff.ENDLESS, Game1.buffsIcons, 11, buffEffects, false, "Attack"));

                    BuffEffects buffEffects2 = new();
                    buffEffects2.Defense.Value = StonesBroken.Value / 100;
                    who.buffs.Apply(new("VPP.Mining-Combat.Defense", "VPP.CombatMining.ComboProfession", "Mining-Combat", Buff.ENDLESS, Game1.buffsIcons, 11, buffEffects, false, "Defense"));

                    Game1.addHUDMessage(new(ModEntry.Helper.Translation.Get("Message.StoneBroken", new { Stones = StonesBroken.Value, Buff = StonesBroken.Value / 100 }), 1));
                }
                if (MiningPatcher.IsExplosionForExplosivePersonality && Game1.random.NextBool(0.15) && TalentUtility.CurrentPlayerHasTalent("ExplosivePersonality", who: who))
                {
                    Game1.createObjectDebris(Game1.random.ChooseFrom(new string[] { "535", "536", "537", "749" }), x, y);
                    MiningPatcher.IsExplosionForExplosivePersonality = false;
                }
                if (__instance is MineShaft shaft && shaft.mineLevel != 77377 && !shaft.ladderHasSpawned)
                {
                    if (shaft.modData.TryGetValue(TalentCore.Key_DownInTheDepths, out string val))
                    {
                        shaft.modData[TalentCore.Key_DownInTheDepths] = (int.Parse(val) + 1).ToString();
                        if (val is "8")
                        {
                            shaft.createLadderDown(x, y);
                            shaft.createLadderAt(new(x, y));
                            shaft.ladderHasSpawned = true;
                            shaft.modData[TalentCore.Key_DownInTheDepths] = "0";
                        }
                    }
                    else
                    {
                        shaft.modData.Add(TalentCore.Key_DownInTheDepths, "0");
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.OnStoneDestroyed", "postfixed", true);
            }
        }
        public static void canThisBeAttached_FishingRod_Postfix(StardewValley.Object o, FishingRod __instance, int slot, ref bool __result)
        {
            try
            {
                if (slot is not 0)
                {
                    if (o.HasContextTag("category_gem") && CoreUtility.CurrentPlayerHasProfession("Fish-Mine"))
                    {
                        __result = __instance.CanUseTackle();
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FishingRod.canThisBeAttached", "postfixed", true);
            }
        }
        public static void canThisBeAttached_Slingshot_Postfix(StardewValley.Object o, ref bool __result)
        {
            try
            {
                if (o.Category == -4 && CoreUtility.CurrentPlayerHasProfession("Combat-Fish"))
                {
                    __result = true;
                }
                else if (TalentUtility.CurrentPlayerHasTalent("Slimeshot") && o.HasContextTag("slime_item"))
                {
                    __result = true;
                }
                else if (TalentUtility.CurrentPlayerHasTalent("DazzlingStrike") && o.HasContextTag("category_gem"))
                {
                    __result = true;
                }
                else if (o.QualifiedItemId == "(TR)MagicQuiver")
                {
                    __result = true;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.canThisBeAttached", "postfixed", true);
            }
        }
        public static int FishPriceToDamage(StardewValley.Object ammo)
        {
            if (ammo is not null)
                if (ammo.Category == -4)
                    return ammo.Price * 5;
            return 50;
        }
        public static void GetAmmoDamage_Postfix(StardewValley.Object ammunition, ref int __result)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession("Combat-Fish"))
                {
                    __result = FishPriceToDamage(ammunition);
                }
                if (ammunition is Trinket trinket && trinket.QualifiedItemId == "(TR)MagicQuiver")
                {
                    __result = 40;
                }

            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.GetAmmoDamage", "postfixed", true);
            }
        }
        public static void GetAmmoCollisionSound_Postfix(StardewValley.Object ammunition, ref string __result)
        {
            try
            {
                if (ammunition.Category == StardewValley.Object.FishCategory || ammunition.Name.Contains("Slime"))
                {
                    __result = "slimedead";
                }
                /*else if (ammunition is Trinket trinket && trinket.QualifiedItemId == "(TR)MagicQuiver")
                {
                    __result = "magic_arrow_hit";
                    ammunition.Stack++;
                }*/
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.GetAmmoCollisionSound", "postfixed", true);
            }
        }
    }
}
