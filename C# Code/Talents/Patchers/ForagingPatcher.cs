using System;
using System.Collections.Generic;
using System.Linq;
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
using StardewValley.Quests;

namespace VanillaPlusProfessions.Talents.Patchers
{
    internal static class ForagingPatcher
    {
        readonly static string PatcherName = nameof(ForagingPatcher);
        readonly static Type PatcherType = typeof(ForagingPatcher);

        internal static void ApplyPatches()
        {
            CoreUtility.PatchMethod(PatcherName, "Tree.performTreeFall",
                original: AccessTools.Method(typeof(Tree), "performTreeFall"),
                postfix: new HarmonyMethod(PatcherType, nameof(performTreeFall_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "Tree.shake",
                original: AccessTools.Method(typeof(Tree), nameof(Tree.shake)),
                prefix: new HarmonyMethod(PatcherType, nameof(shake_Prefix))
            );
            CoreUtility.PatchMethod(PatcherName, "Tree.dayUpdate",
                original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
                postfix: new HarmonyMethod(PatcherType, nameof(dayUpdate_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "Bush.inBloom",
                original: AccessTools.Method(typeof(Bush), nameof(Bush.inBloom)),
                postfix: new HarmonyMethod(PatcherType, nameof(inBloom_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "Object.OutputSolarPanel",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.OutputSolarPanel)),
                postfix: new HarmonyMethod(PatcherType, nameof(OutputSolarPanel_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "RockGolem.getExtraDropItems",
                original: AccessTools.Method(typeof(RockGolem), nameof(RockGolem.getExtraDropItems)),
                postfix: new HarmonyMethod(PatcherType, nameof(LivingHat_RockGolem_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "Object.cutWeed",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.cutWeed)),
                postfix: new HarmonyMethod(PatcherType, nameof(LivingHat_Object_Postfix))
            );
            CoreUtility.PatchMethod(PatcherName, "Torch.checkForAction",
                original: AccessTools.Method(typeof(Torch), nameof(Torch.checkForAction)),
                prefix: new HarmonyMethod(PatcherType, nameof(checkForAction_Prefix))
            );
            CoreUtility.PatchMethod(PatcherName, "CraftingPage.GetRecipesToDisplay",
                original: AccessTools.Method(typeof(CraftingPage), "GetRecipesToDisplay"),
                prefix: new HarmonyMethod(PatcherType, nameof(GetRecipesToDisplay_Prefix))
            );

            //Keep this under watch but... why?
            CoreUtility.PatchMethod(PatcherName, "CraftingRecipe.createItem",
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
                prefix: new HarmonyMethod(PatcherType, nameof(createItem_Postfix))
            );
        }
        public static bool createItem_Postfix(ref Item __result, CraftingRecipe __instance)
        {
            try
            {
                Item producedItem = ItemRegistry.Create(__instance.GetItemData().QualifiedItemId, __instance.numberProducedPerCraft);
                if (__instance.isCookingRecipe && producedItem is StardewValley.Object @object)
                {
                    if (TalentCore.IsCookoutKit.Value)
                    {
                        @object.Edibility += (int)(@object.Edibility / 0.5f);
                        @object.Quality = 1;
                    }
                    if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits") && Game1.random.NextBool(0.1))
                    {
                        if (Game1.player.currentLocation.GetFridge()?.Items.ContainsId("(TR)IceRod") is true)
                        {
                            @object.Quality = 2;
                            __result = @object;
                            return false;
                        }
                        foreach (var item in Game1.player.currentLocation.Objects.Pairs)
                        {
                            if (item.Value is Chest chest && chest.QualifiedItemId == "(BC)216" && chest.Items.ContainsId("(TR)IceRod"))
                            {
                                @object.Quality = 2;
                                __result = @object;
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CraftingRecipe.createItem", "postfixed", true);
            }
            return true;
        }

        public static void dayUpdate_Postfix(Tree __instance)
        {
            try
            {
                if (__instance.fertilized.Value && __instance.growthStage.Value < 5 && !__instance.stump.Value)
                {
                    if (TalentUtility.CurrentPlayerHasTalent("Foraging_Grove_Tending"))
                    {
                        __instance.growthStage.Value++;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Tree.dayUpdate", "prefixed", true);
            }            
        }

        public static void checkForAction_Prefix(Torch __instance, bool justCheckingForActivity)
        {
            try
            {
                if (__instance.QualifiedItemId is "(BC)278" && !justCheckingForActivity)
                {
                    TalentCore.IsCookoutKit.Value = true;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Torch.checkForAction", "prefixed", true);
            }
        }
        public static void GetRecipesToDisplay_Prefix(CraftingPage __instance, ref List<string> __result)
        {
            try
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CraftingPage.GetRecipesToDisplay", "prefixed", true);
            }
        }

        public static void shake_Prefix(Tree __instance)
        {
            try
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Tree.shake", "prefixed", true);
            }
        }

        public static void LivingHat_RockGolem_Postfix(ref List<Item> __result)
        {
            try
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "RockGolem.getExtraDropItems", "postfixed", true);
            }
        }
        public static void LivingHat_Object_Postfix()
        {
            try
            {
                if (Game1.random.NextBool(TryOverrideLivingHatChance()))
                {
                    Game1.createItemDebris(ItemRegistry.Create("(H)40"), Game1.player.StandingPixel.ToVector2(), Game1.player.FacingDirection);
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.cutWeed", "postfixed", true);
            }
        }

        public static float TryOverrideLivingHatChance() => TalentUtility.CurrentPlayerHasTalent("EyeSpy") ? 0.0001f : 1E-05f;

        public static void OutputSolarPanel_Postfix(ref int overrideMinutesUntilReady)
        {
            try
            {
                if (TalentUtility.AnyPlayerHasTalent("StaticCharge"))
                    overrideMinutesUntilReady -= 4000; //2600 - 600 = 2000; 2000 * 2 = 4000;
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.OutputSolarPanel", "postfixed", true);
            }
        }

        public static void performTreeFall_Postfix(Tool t, int explosion, bool __result, Tree __instance)
        {
            try
            {
                if (__result && !__instance.falling.Value && __instance.stump.Value && Game1.random.NextBool(0.5))
                {
                    if (TalentUtility.AnyPlayerHasTalent("NatureSecrets") && __instance.Location?.GetData()?.Forage?.Count > 0)
                    {
                        List<string> strings = (from forageData in __instance.Location.GetData().Forage
                                                where GameStateQuery.CheckConditions(forageData.Condition, __instance.Location ?? t.getLastFarmerToUse().currentLocation ?? Game1.player.currentLocation, t.getLastFarmerToUse() ?? Game1.player)
                                                && (forageData.Season is null || (forageData.Season is not null && Game1.season == forageData.Season))
                                                select forageData.ItemId).ToList();

                        strings.RemoveWhere(str => string.IsNullOrEmpty(str) || ItemContextTagManager.HasBaseTag(str, TalentCore.ContextTag_Banned_NatureSecrets) || !ItemRegistry.Exists("(O)" + str));

                        if (strings.Count > 0)
                        {
                            Game1.createObjectDebris(Game1.random.ChooseFrom(strings), (int)__instance.Tile.X, (int)__instance.Tile.Y, __instance.Location);
                        }
                    }
                }
                if (t is null && explosion is 0 && !IsAnyCharAround(__instance.Location, __instance.Tile))
                {
                    if (TalentUtility.AnyPlayerHasTalent("SurgeProtection"))
                    {
                        Game1.createMultipleObjectDebris("382", (int)__instance.Tile.X, (int)__instance.Tile.Y, 3);
                        __instance.falling.Value = false;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Tree.performTreeFall", "postfixed", true);
            }
        }

        public static bool IsAnyCharAround(GameLocation loc, Vector2 tilePosition)
        {
            try
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
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "ForagingPatcher.IsAnyCharAround", "<individual method>", true);
            }
            return false;
        }

        //Berry-mania
        public static void inBloom_Postfix(Bush __instance, ref bool __result)
        {
            try
            {
                if (TalentUtility.AnyPlayerHasTalent("Berry-mania") && __instance.size.Value is 1 or 2)
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.getDescription", "prefixed", true);
            }
        }
    }
}
