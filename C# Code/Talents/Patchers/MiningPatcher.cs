using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.GameData.Objects;
using System.Collections.Generic;
using System.Linq;
using VanillaPlusProfessions.Utilities;
using System;
using StardewValley.Internal;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public static class MiningPatcher
    {
        readonly static string PatcherName = nameof(MiningPatcher);
        readonly static Type PatcherType = typeof(MiningPatcher);

        internal static void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                PatcherName, "Utility.getTreasureFromGeode",
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getTreasureFromGeode)),
                postfix: new HarmonyMethod(PatcherType, nameof(getTreasureFromGeode_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.getDescription",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getDescription)),
                postfix: new HarmonyMethod(PatcherType, nameof(getDescription_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.onExplosion",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.onExplosion)),
                prefix: new HarmonyMethod(PatcherType, nameof(onExplosion_Prefix))
            );
        }
        public static bool IsExploding = false;

        public static bool IsExplosionForExplosivePersonality = false;

        public static bool IsUpdating = false;
        public static bool onExplosion_Prefix(StardewValley.Object __instance, ref bool __result, Farmer who)
        {
            try
            {
                IsExplosionForExplosivePersonality = true;
                if ((__instance.isForage() || __instance.bigCraftable.Value || __instance.QualifiedItemId is "(O)590" or "(O)SeedSpot") && TalentUtility.CurrentPlayerHasTalent("Mining_Detonation_Dampener", who: who))
                {
                    if (__instance.QualifiedItemId is not "(BC)78")
                    {
                        IsExploding = true;
                        __result = false;
                        return false;
                    }
                }
                IsExploding = false;
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.getDescription", "prefixed", true);
            }
            return true;
        }

        public static void getDescription_Postfix(StardewValley.Object __instance, ref string __result)
        {
            try
            {
                if (Utility.IsGeode(__instance, true) && TalentUtility.CurrentPlayerHasTalent("X-ray") && __instance.modData.TryGetValue(TalentCore.Key_XrayDrop, out string value) && value is not null or "")
                {
                    __result = Game1.parseText(__result.Replace("\n", "").Replace("\r", "") + " " + ModEntry.Helper.Translation.Get("Item.Xray.GeodeDrop", new { dropName = ItemRegistry.GetData(value).DisplayName }), Game1.smallFont, ModEntry.Helper.Reflection.GetMethod(__instance, "getDescriptionWidth").Invoke<int>(null));
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.getDescription", "postfixed", true);
            }
        }

        public static void getTreasureFromGeode_Postfix(Item geode, ref Item __result)
        {
            try
            {
                if (__result is not null && geode is not null && !geode.ItemId.Contains("MysteryBox"))
                {
                    if (TalentUtility.CurrentPlayerHasTalent("X-ray") && geode.modData.TryGetValue(TalentCore.Key_XrayDrop, out string drop) is true && !IsUpdating)
                    {
                        if (drop is not null or "" && Utility.IsGeode(geode, true))
                        {
                            Item tryItem = ItemRegistry.Create(drop, geode.Category == __result.Category ? __result.Stack : 1, 0);
                            var data = ItemRegistry.GetDataOrErrorItem(tryItem.ItemId);
                            if (data.IsErrorItem && data.RawData is not null and ObjectData data1)
                            {
                                for (int i = 0; i < data1.GeodeDrops.Count; i++)
                                {
                                    if (GameStateQuery.CheckConditions(data1.GeodeDrops[i].Condition) && Game1.random.NextBool(data1.GeodeDrops[i].Chance))
                                    {
                                        Item queryResults = ItemQueryResolver.TryResolveRandomItem(data1.GeodeDrops[i], new(Game1.player.currentLocation, Game1.player, Game1.random, "X-ray context"), avoidRepeat: false);
                                        if (queryResults is not null)
                                        {
                                            if (data1.GeodeDrops[i].SetFlagOnPickup != null)
                                                queryResults.SetFlagOnPickup = data1.GeodeDrops[i].SetFlagOnPickup;
                                            queryResults.Quality = __result.Quality;
                                            queryResults.Stack = 1;
                                            __result = queryResults ?? __result;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                __result = tryItem;
                            }
                        }
                    }
                    else if (TalentUtility.CurrentPlayerHasTalent("Mining_Matryoshka") && Game1.random.NextBool(0.15) && !geode.HasContextTag(TalentCore.ContextTag_Matryoshka_Banned_FromDropping))
                    {
                        var geodes = (from KeyValuePair<string, ObjectData> @object in Game1.objectData
                                      where (@object.Value.GeodeDropsDefaultItems || @object.Value.GeodeDrops?.Count > 0 is true) && !@object.Value.ContextTags?.Contains(TalentCore.ContextTag_Matryoshka_Banned_FromBeingDropped) is true && geode.ItemId != @object.Key
                                      select @object.Key).ToList();
                        __result = ItemRegistry.Create(Game1.random.ChooseFrom(geodes), 1, geode.Quality);
                    }
                    else if (TalentUtility.CurrentPlayerHasTalent("Mining_Museum_Piece") && LibraryMuseum.totalArtifacts != Game1.player.archaeologyFound.Keys.Count() && geode.ItemId is not null && Game1.random.NextBool(0.1))
                    {
                        List<KeyValuePair<string, double>> validIDs = new();

                        if (Game1.objectData.TryGetValue(geode.ItemId, out var data) && data is not null && data.GeodeDrops is not null)
                        {
                            foreach (ObjectGeodeDropData dropx in data.GeodeDrops.OrderBy((ObjectGeodeDropData p) => p.Precedence))
                            {
                                if ((!Game1.random.NextBool(dropx.Chance) || (dropx.Condition != null && !GameStateQuery.CheckConditions(dropx.Condition, Game1.player.currentLocation, Game1.player, null, null, Game1.random))))
                                    continue;

                                Item item = ItemQueryResolver.TryResolveRandomItem(dropx, new ItemQueryContext(Game1.player.currentLocation, Game1.player, Game1.random, "Museum Piece context"), avoidRepeat: false);
                                if (item is not null && (!LibraryMuseum.HasDonatedArtifact(item.ItemId) || Game1.random.NextBool(0.20)))
                                {
                                    if (dropx.SetFlagOnPickup != null)
                                    {
                                        item.SetFlagOnPickup = dropx.SetFlagOnPickup;
                                    }
                                    item.Quality = __result.Quality;
                                    item.Stack = 1;
                                    __result = item;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Utility.getTreasureFromGeode", "postfixed", true);
            }
        }
    }
}