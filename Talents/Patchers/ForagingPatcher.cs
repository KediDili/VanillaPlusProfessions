using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.WildTrees;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents.Patchers
{
    internal static class ForagingPatcher
    {
        internal static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Tree), "performTreeFall"),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.performTreeFall_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), "Tree.performTreeFall", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Tree), nameof(Tree.shake)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.shake_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(Tree.shake), "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.dayUpdate_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(Tree.dayUpdate), "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Bush), nameof(Bush.inBloom)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.inBloom_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(Bush.inBloom), "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputSolarPanel)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.OutputSolarPanel_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(StardewValley.Object.OutputSolarPanel), "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(RockGolem), nameof(RockGolem.getExtraDropItems)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.LivingHat_RockGolem_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(RockGolem.getExtraDropItems), "postfixing");
            }
           
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.cutWeed)),
                    postfix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.LivingHat_Object_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(StardewValley.Object.cutWeed), "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Torch), nameof(Torch.checkForAction)),
                    prefix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.checkForAction_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), nameof(Torch.checkForAction), "prefixing");
            }
           
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(CraftingPage), "GetRecipesToDisplay"),
                    prefix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.GetRecipesToDisplay_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), "CraftingPage.GetRecipesToDisplay", "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
                    prefix: new HarmonyMethod(typeof(ForagingPatcher), nameof(ForagingPatcher.createItem_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(ForagingPatcher), "CraftingPage.GetRecipesToDisplay", "postfixing");
            }
        }

        public static void createItem_Postfix(ref Item __result, CraftingRecipe __instance)
        {
            if (__instance.isCookingRecipe && __result is StardewValley.Object @object)
            {
                if (TalentCore.IsCookoutKit.Value)
                {
                    @object.Edibility += (int)(@object.Edibility * 0.5f);
                    @object.Quality = 1;
                }
                if (TalentUtility.CurrentPlayerHasTalent("Combat_HiddenBenefits") && Game1.random.NextBool(0.1))
                {
                    foreach (var item in Game1.player.currentLocation.Objects.Pairs)
                    {
                        if (item.Value is Chest chest && chest.QualifiedItemId == "(BC)216" && chest.Items.ContainsId("(TR)IceRod"))
                        {
                            __result.Quality = 2;
                            break;
                        }
                    }
                }
            }
        }

        public static void dayUpdate_Prefix(Tree __instance)
        {
            if (__instance.fertilized.Value && __instance.growthStage.Value < 5 && !__instance.stump.Value)
            {
                if (TalentUtility.CurrentPlayerHasTalent("Foraging_Grove_Tending"))
                {
                    __instance.growthStage.Value++;
                }
            }
        }

        public static void checkForAction_Prefix(Torch __instance, bool justCheckingForActivity)
        {
            if (__instance.QualifiedItemId is "(BC)278" && !justCheckingForActivity)
            {
                TalentCore.IsCookoutKit.Value = true;
            }
        }
        public static void GetRecipesToDisplay_Prefix(CraftingPage __instance, ref List<string> __result)
        {
            if (__instance.cooking && !TalentCore.IsCookoutKit.Value && __result is not null)
            {
                List<string> listToEdit = __result;
                foreach (var item in __result)
                {
                    if (ItemContextTagManager.HasBaseTag(ArgUtility.SplitQuoteAware(item, '/')[2], TalentCore.ContextTag_SurvivalCooking))
                    {
                        listToEdit.Remove(item);
                    }
                }
                __result = listToEdit;
            }
        }

        public static void shake_Prefix(Tree __instance)
        {
            if (__instance.hasSeed.Value && __instance.modData.ContainsKey(TalentCore.Key_Reforestation) && TalentUtility.CurrentPlayerHasTalent("Foraging_Reforestation") && __instance.growthStage.Value is 5)
            {
                WildTreeData data = __instance.GetData();
                if (data != null && data.SeedDropItems?.Count > 0)
                {
                    foreach (WildTreeSeedDropItemData drop in data.SeedDropItems)
                    {
                        Item seed2 = (Item)AccessTools.Method(typeof(Tree), "TryGetDrop", new Type[] { typeof(WildTreeItemData), typeof(Random), typeof(Farmer), typeof(string) }).Invoke(__instance, new object[] { drop, Game1.random, Game1.player, "SeedDropItems" });
                        if (seed2 != null)
                        {
                            if (Game1.player.professions.Contains(16) && seed2.HasContextTag("forage_item"))
                                seed2.Quality = 4;

                            Game1.createItemDebris(seed2, new Vector2(__instance.Tile.X * 64f, (__instance.Tile.Y - 3f) * 64f), -1, __instance.Location, Game1.player.StandingPixel.Y);
                        }
                    }
                }
            }
        }

        public static void LivingHat_RockGolem_Postfix(ref List<Item> __result)
        {
            foreach (var item in __result)
            {
                if (item.QualifiedItemId == "(H)40")
                {
                    return;
                }
            }
            if (Game1.random.NextBool(TryOverrideLivingHatChance()))
            {
                __result = new() { ItemRegistry.Create("(H)40") };
            }
        }
        public static void LivingHat_Object_Postfix()
        {
            if (Game1.random.NextBool(TryOverrideLivingHatChance()))
            {
                Game1.createItemDebris( ItemRegistry.Create("(H)40"), Game1.player.StandingPixel.ToVector2(), Game1.player.FacingDirection);
            }
        }

        public static float TryOverrideLivingHatChance() => TalentUtility.CurrentPlayerHasTalent("EyeSpy") ? 0.0001f : 1E-05f;

        public static void OutputSolarPanel_Postfix(ref int overrideMinutesUntilReady)
        {
            if (TalentUtility.AnyPlayerHasTalent("Foraging_Static_Charge"))
                overrideMinutesUntilReady -= 4000; //2600 - 600 = 2000; 2000 * 2 = 4000;
        }

        public static void performTreeFall_Postfix(Tool t, int explosion, bool __result, Tree __instance)
        {
            if (!__result && __instance.falling.Value && __instance.stump.Value)
            {
                /*if (TalentUtility.AnyPlayerHasTalent("Foraging_Nature_Secrets") && Game1.random.NextBool(0.1))
                {
                    List<string> strings = (from forageData in __instance.Location.GetData().Forage
                                           where Game1.random.NextBool(forageData.Chance) && GameStateQuery.CheckConditions(forageData.Condition, __instance.Location, t.getLastFarmerToUse())
                                           && (forageData.Season is null || (forageData.Season is not null && Game1.season == forageData.Season))
                                           select forageData.ItemId).ToList();
                    if (strings.Count > 0)
                    {
                        Game1.createObjectDebris(Game1.random.ChooseFrom(strings), (int)__instance.Tile.X, (int)__instance.Tile.Y, __instance.Location);
                    }
                }*/
            }
            if (t is null && explosion is 0 && !IsAnyCharAround(__instance.Location, __instance.Tile))
            {
                if (TalentUtility.AnyPlayerHasTalent("Foraging_Surge_Protection"))
                {
                    Game1.createMultipleObjectDebris("382", (int)__instance.Tile.X, (int)__instance.Tile.Y, 3);
                    __instance.falling.Value = false;
                }
            }
        }

        public static bool IsAnyCharAround(GameLocation loc, Vector2 tilePosition)
        {
            Vector2 pos = tilePosition;
            bool xincrement = false;
            bool yincrement = false;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (loc.isCharacterAtTile(pos) is not null and NPC npc)
                    {
                        return npc.IsEmoting && npc.CurrentEmote is 16;
                    }
                    else
                    {
                        if (x is 1 && !xincrement)
                        {
                            pos.X++;
                            xincrement = true;
                        }
                        else if (x is 2)
                        {
                            pos.X = tilePosition.X - 1;
                            xincrement = false;
                        }
                        if (y is 1 && !yincrement)
                        {
                            pos.Y++;
                            yincrement = true;
                        }
                        else if (y is 2)
                        {
                            pos.Y = tilePosition.Y - 1;
                            yincrement = false;
                        }
                    }
                }
                xincrement = false;
            }
            return false;
        }

        //Berry-mania
        public static void inBloom_Postfix(Bush __instance, ref bool __result)
        {
            if (TalentUtility.AnyPlayerHasTalent("Foraging_Berrymania") && __instance.size.Value is 1 or 2)
            {
                Season season = __instance.Location.GetSeason();
                if (season is Season.Spring && Game1.dayOfMonth > 13 && Game1.dayOfMonth < 22)
                {
                    __result = true;
                }
                else if (season is Season.Fall && Game1.dayOfMonth > 6 && Game1.dayOfMonth < 15)
                {
                    __result = true;
                }
            }
        }
    }
}
