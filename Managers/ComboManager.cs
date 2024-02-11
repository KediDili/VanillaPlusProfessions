using HarmonyLib;
using System;
using System.Collections.Generic;
using StardewValley.Tools;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection.Emit;
using System.Linq;
using StardewValley.TerrainFeatures;
using StardewValley.Extensions;

namespace VanillaPlusProfessions.Managers
{
    public class ComboManager : IProfessionManager
    {
        public int SkillValue => 9;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), "canThisBeAttached", new Type[] { typeof(StardewValley.Object), typeof(int)}),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.canThisBeAttached_FishingRod_Postfix))
            );
            
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Slingshot), "canThisBeAttached", new Type[] { typeof(StardewValley.Object), typeof(int)}),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.canThisBeAttached_Slingshot_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed)),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.OnStoneDestroyed_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DrawIconBar), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color) }),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.DrawIconBar_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize)),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.maximumStackSize_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.GetTackleQualifiedItemIDs)),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.GetTackleQualifiedItemIDs_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.PerformFire)),
                transpiler: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.Transpiler))
            );
            /*ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.performToolAction_Postfix)))
            );*/
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), "CheckForActionOnMachine"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.CheckForActionOnMachine_Postfix)))
            );
           /* ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performUseAction)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.performUseAction_Postfix_FruitTree)))
            );*/
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.draw)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.draw_FruitTree_Postfix)))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.draw)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.draw_GiantCrop_Postfix)))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputGeodeCrusher)),
                postfix: new HarmonyMethod(typeof(ComboManager), nameof(ComboManager.OutputGeodeCrusher_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ComboManager), nameof(ComboManager.sinkDebris_Postfix)))
            );
        }

        internal static int StonesBroken;

        public static void sinkDebris_Postfix(Debris debris, GameLocation __instance, bool __result)
        {
            if (__result is true && CoreUtility.AnyPlayerHasProfession(74) && debris.item.HasContextTag("category_forage") && Game1.player.modData.TryGetValue(ModEntry.Key_HasFoundForage, out var str))
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
        public static void OutputGeodeCrusher_Postfix(StardewValley.Object machine, Item inputItem, bool probe, ref Item __result)
        {
            if ((inputItem.HasContextTag("category_minerals") || inputItem.HasContextTag("category_gem")) && inputItem is StardewValley.Object obj && CoreUtility.CurrentPlayerHasProfession(71))
            {
                var oneofObj = obj.getOne();
                oneofObj.modData.TryAdd("Kedi.SMP.CurrentPreserveType", "Kedi.SMP.GemDust");
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
                health = ((FishingRod.maxTackleUses - __instance.uses.Value) + 0f) / FishingRod.maxTackleUses;
                spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), StardewValley.Utility.getRedToGreenLerpColor(health));
            }
        }

        public static void OnStoneDestroyed_Postfix(Farmer who)
        {
            if (CoreUtility.CurrentPlayerHasProfession(72))
            {
                StonesBroken++;
            }
            if (StonesBroken / 100 > 0 && StonesBroken % 100 == 0 && !Game1.doesHUDMessageExist($"You broke {StonesBroken - 1} stones today, your attack and defense is increased by {StonesBroken / 100}"))
            {
                ObjectBuffData buff1 = new()
                {
                    IsDebuff = false,
                    Duration = Buff.ENDLESS,
                    IconSpriteIndex = 11,
                    CustomAttributes = new()
                };
                buff1.CustomAttributes.Attack = StonesBroken / 100;
                BuffEffects buffEffects = new(buff1.CustomAttributes);
                who.buffs.Apply(new("SMP.Attack", "SMP.CombatMining.ComboProfession", "Some More Professions", Buff.ENDLESS, Game1.buffsIcons, 11, buffEffects, false, "Attack"));

                ObjectBuffData buff2 = new()
                {
                    IsDebuff = false,
                    Duration = Buff.ENDLESS,
                    IconSpriteIndex = 10,
                    CustomAttributes = new()
                };
                buff2.CustomAttributes.Defense = StonesBroken / 100;
                BuffEffects buffEffects2 = new(buff2.CustomAttributes);

                who.buffs.Apply(new("SMP.Defense", "SMP.CombatMining.ComboProfession", "Some More Professions", Buff.ENDLESS, Game1.buffsIcons, 10, buffEffects2, false, "Defense"));
                Game1.addHUDMessage(new($"You broke {StonesBroken} stones today, your attack and defense is increased by {StonesBroken / 100}", 1));
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
        }
        public static int FishPriceToDamage(Slingshot slingshot)
        {
            if (slingshot.attachments[0] is not null)
                if (slingshot.attachments[0].Category == -4)
                    return slingshot.attachments[0].Price * 5;
            return 50;
        }
       /*public static void performUseAction_Postfix_FruitTree(FruitTree __instance)
        {
            if (Utility.CurrentPlayerHasProfession(49))
            {
                if (Game1.player.ActiveObject?.IsHeldOverHead() == true && Game1.player.ActiveObject?.IsTapper() == true)
                {
                    FruitTreeData fruitTreeData = __instance.GetData();
                    var obj = ItemRegistry.Create<StardewValley.Object>(__instance.modData["KediDili.SomeMoreProfessions.TapperID"]);
                    obj.modData.Add("KediDili.SomeMoreProfessions.ProduceDaysLeft", "0");
                    var dsdsd = ItemRegistry.Create<StardewValley.Object>(fruitTreeData.Fruit[0].ItemId);
                    dsdsd.modData.Add("Kedi.SMP.CurrentPreserveType", "Kedi.SMP.FruitSyrup");
                    obj.lastInputItem.Add(dsdsd);

                    if (__instance.modData.TryAdd("KediDili.SomeMoreProfessions.IsTapped", "true")
                    && __instance.modData.TryAdd("KediDili.SomeMoreProfessions.TapperID", Game1.player.ActiveObject.QualifiedItemId))
                    {
                        Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        __instance.Location.Objects.Add(__instance.Tile, obj);
                    }

                    else if (__instance.modData.TryGetValue("KediDili.SomeMoreProfessions.IsTapped", out var value) && __instance.modData.ContainsKey("KediDili.SomeMoreProfessions.TapperID") && value is "false")
                    {
                        __instance.modData["KediDili.SomeMoreProfessions.IsTapped"] = "true";
                        __instance.modData["KediDili.SomeMoreProfessions.TapperID"] = Game1.player.ActiveObject.QualifiedItemId;
                        Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                        __instance.Location.Objects.Add(__instance.Tile, obj);
                    }
                }
                else if (__instance.fruit.Count > 0)
                {
                    __instance.shake(__instance.Tile, true);
                }
            }
        }
        public static void performToolAction_Postfix(StardewValley.Object __instance)
        {
            if (__instance.IsTapper() && __instance.Location.terrainFeatures.TryGetValue(__instance.TileLocation, out var value) && value is FruitTree tree)
                tree.modData["KediDili.SomeMoreProfessions.IsTapped"] = "false";
        }*/

        public static void draw_FruitTree_Postfix(FruitTree __instance, SpriteBatch spriteBatch)
        {
            ManagerUtility.TapperOnTerrainFeature(__instance, spriteBatch);
        }
        public static void draw_GiantCrop_Postfix(GiantCrop __instance, SpriteBatch spriteBatch)
        {
            ManagerUtility.TapperOnTerrainFeature(__instance, spriteBatch);
        }
        public static void CheckForActionOnMachine_Postfix(StardewValley.Object __instance, bool justCheckingForActivity, ref bool __result)
        {
            if (__instance is not null && __instance.IsTapper() && __instance.Location.terrainFeatures.TryGetValue(__instance.TileLocation, out var terrainFeature) && terrainFeature is FruitTree tree)
            {
                if (!justCheckingForActivity && !__result && __instance.heldObject.Value is not null)
                {
                    __instance.modData[ModEntry.Key_TFTapperDaysLeft] = "5";
                    Game1.player.addItemByMenuIfNecessary(__instance.heldObject.Value);
                    __instance.heldObject.Value = null;
                    __result = true;
                    return;
                }
                else
                {
                    if (!justCheckingForActivity)
                        tree.performUseAction(__instance.TileLocation);
                }
            }
            __result = false;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var number = 3;
            var indesx = 0;
            List<CodeInstruction> insts = codeInstructions.ToList();
            foreach (var item in codeInstructions)
            {
                indesx++;
                yield return item;
                if (number == 0 && item.opcode.Equals(OpCodes.Stloc_S) && item.OperandIs(7))
                {
                    yield return new(OpCodes.Ldloc_S, 4);
                    yield return new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Item), nameof(Item.Category)));
                    yield return new(OpCodes.Ldc_I4_S, -4);
                    yield return new(OpCodes.Callvirt, AccessTools.Method(typeof(int), "op_Equality"));
                    yield return new(OpCodes.Brfalse_S, insts[indesx + 1].ExtractLabels()[0]);
                    yield return new(OpCodes.Ldstr, "slimedead");
                    yield return new(OpCodes.Stloc, 7);
                    yield return new(OpCodes.Ldarg_0);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(ComboManager), nameof(ComboManager.FishPriceToDamage)));
                    //yield return new(OpCodes.Ldc_I4_S, 50);
                    yield return new(OpCodes.Stloc, 5);
                }
                else
                {
                    number--;
                }
            }
        }
    }
}
