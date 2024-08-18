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
using StardewValley.Objects;

namespace VanillaPlusProfessions.Managers
{
    public class ComboManager : IProfessionManager
    {
        public int SkillValue => 9;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishingRod), "canThisBeAttached", new Type[] { typeof(StardewValley.Object), typeof(int) }),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.canThisBeAttached_FishingRod_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "'FishingRod.canThisBeAttached'", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), "canThisBeAttached", new Type[] { typeof(StardewValley.Object), typeof(int) }),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.canThisBeAttached_Slingshot_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "'Slingshot.canThisBeAttached'", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.OnStoneDestroyed_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "GameLocation.OnStoneDestroyed", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DrawIconBar), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color) }),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.DrawIconBar_Postfix))
              );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "Object.DrawIconBar", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.maximumStackSize_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "Object.maximumStackSize", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.GetTackleQualifiedItemIDs)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.GetTackleQualifiedItemIDs_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "FishingRod.GetTackleQualifiedItemIDs", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.GetAmmoDamage)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.GetAmmoDamage_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "Slingshot.GetAmmoDamage", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.GetAmmoCollisionSound)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.GetAmmoCollisionSound_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "Slingshot.GetAmmoCollisionSound", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnMachine"),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.CheckForActionOnMachine_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "'Object.CheckForActionOnMachine'", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputGeodeCrusher)),
                    postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.OutputGeodeCrusher_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "Object.OutputGeodeCrusher", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.sinkDebris_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "GameLocation.sinkDebris", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.draw)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.FruitTree_Draw_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ComboManager), "FruitTree.draw", "postfixing");
            }
        }

        internal static PerScreen<int> StonesBroken = new();

        public static void sinkDebris_Postfix(Debris debris, GameLocation __instance, bool __result)
        {
            if (__result is true && CoreUtility.AnyPlayerHasProfession(74) && debris.item is not null && debris.item.HasContextTag("category_forage") && Game1.player.modData.TryGetValue(ModEntry.Key_HasFoundForage, out var str))
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
                                list.Add(new (x, y));

                    __instance.fishSplashPoint.Value = Game1.random.ChooseFrom(list);
                }
            }
        }

        public static void FruitTree_Draw_Postfix(FruitTree __instance, SpriteBatch spriteBatch)
        {
            if (__instance is not null)
            {
                if (!__instance.stump.Value && __instance.modData.TryGetValue(ModEntry.Key_TFHasTapper, out string value) && value is "true")
                {
                    var metaData = ItemRegistry.GetData(__instance.modData[ModEntry.Key_TFTapperID]);
                    Rectangle rect = metaData.GetSourceRect();
                    rect.Height = 16;
                    spriteBatch.Draw(metaData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.Tile.X * 64f, (__instance.Tile.Y - 1) * 64f)), rect, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, __instance.getBoundingBox().Bottom / 10000f + 0.001f);
                }
            }
        }

        public static void OutputGeodeCrusher_Postfix(StardewValley.Object machine, Item inputItem, bool probe, ref Item __result)
        {
            if ((inputItem.HasContextTag("category_minerals") || inputItem.HasContextTag("category_gem")) && inputItem is StardewValley.Object obj && CoreUtility.CurrentPlayerHasProfession(71))
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
        public static void maximumStackSize_Postfix(StardewValley.Object __instance, ref int __result)
        {
            if (__instance.HasContextTag("category_gem") && __instance.uses.Value > 0)
            {
                __result = 1;
            }
        }
        public static void GetTackleQualifiedItemIDs_Postfix(ref List<string> __result, FishingRod __instance)
        {
            if (CoreUtility.CurrentPlayerHasProfession(77))
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

        public static void DrawIconBar_Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize)
        {
            if (__instance.HasContextTag("category_gem") && __instance.uses.Value > 0)
            {
                float health;
                health = (FishingRod.maxTackleUses - __instance.uses.Value) / FishingRod.maxTackleUses;
                spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), StardewValley.Utility.getRedToGreenLerpColor(health));
            }
        }

        public static void OnStoneDestroyed_Postfix(GameLocation __instance, Farmer who, int x, int y)
        {
            if (who is null)
                return;

            if (CoreUtility.CurrentPlayerHasProfession(72, useThisInstead: who))
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
        public static void canThisBeAttached_FishingRod_Postfix(StardewValley.Object o, FishingRod __instance, int slot, ref bool __result)
        {
            if (slot is not 0)
            {
                if (o.HasContextTag("category_gem") && CoreUtility.CurrentPlayerHasProfession(77))
                {
                    __result = __instance.CanUseTackle();
                }
            }
        }
        public static void canThisBeAttached_Slingshot_Postfix(StardewValley.Object o, ref bool __result)
        {
            if (o.Category == -4 && CoreUtility.CurrentPlayerHasProfession(79))
            {
                __result = true;
            }
            else if (true && o.HasContextTag("slime_item"))
            {
                __result = true;
            }
            else if (true && o.HasContextTag("category_gem"))
            {
                __result = true;
            }
            else if (o.QualifiedItemId == "(TR)MagicQuiver")
            {
                __result = true;
            }
        }
        public static int FishPriceToDamage(StardewValley.Object ammo)
        {
            if (ammo is not null)
                if (ammo.Category == -4)
                    return ammo.Price * 5;
            return 50;
        }
        public static void CheckForActionOnMachine_Postfix(StardewValley.Object __instance, bool justCheckingForActivity, ref bool __result)
        {
            if (__instance is not null && __instance.IsTapper() && __instance.Location.terrainFeatures.TryGetValue(__instance.TileLocation, out var terrainFeature) && terrainFeature is FruitTree or GiantCrop)
            {
                if (!justCheckingForActivity && !__result && __instance.heldObject.Value is not null)
                {
                    if (terrainFeature is FruitTree tree)
                    {
                        __instance.modData[ModEntry.Key_TFTapperDaysLeft] = ManagerUtility.GetProduceTimeBasedOnPrice(tree, out StardewValley.Object _);
                    }
                    else if (terrainFeature is GiantCrop crop)
                    {
                        __instance.modData[ModEntry.Key_TFTapperDaysLeft] = ManagerUtility.GetProduceTimeBasedOnPrice(crop, out StardewValley.Object _);
                    }
                    Game1.player.addItemByMenuIfNecessary(__instance.heldObject.Value);
                    __instance.heldObject.Value = null;
                    __result = true;
                    return;
                }
                else
                {
                    if (!justCheckingForActivity && terrainFeature is FruitTree tree)
                        tree.performUseAction(__instance.TileLocation);
                }
            }
            __result = false;
        }
        public static void GetAmmoDamage_Postfix(StardewValley.Object ammunition, ref int __result)
        {
            if (CoreUtility.CurrentPlayerHasProfession(54))
            {
                __result = ammunition.Price * 5;
            }
            if (ammunition is Trinket trinket && trinket.QualifiedItemId == "(TR)MagicQuiver")
            {
                __result = 40;
            }
        }
        public static void GetAmmoCollisionSound_Postfix(StardewValley.Object ammunition, ref string __result)
        {
            if (ammunition.Category == StardewValley.Object.FishCategory || ammunition.Name.Contains("Slime"))
            {
                __result = "slimedead";
            }
            else if (ammunition is Trinket trinket && trinket.QualifiedItemId == "(TR)MagicQuiver")
            {
                __result = "magic_arrow_hit";
                ammunition.Stack++;
            }
        }
    }
}
