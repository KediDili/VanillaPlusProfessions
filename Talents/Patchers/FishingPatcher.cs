using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Tools;
using VanillaPlusProfessions.Utilities;
using Microsoft.Xna.Framework;
using xTile.Dimensions;
using StardewValley.Monsters;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public class FishingPatcher
    {
        internal static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.pullFishFromWater)),
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(PullFishFromWater_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "FishingRod.pullFishFromWater", "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishPond), nameof(FishPond.GetFishProduce)),
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(TryOverrideFishPondChance))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "FishPond.dayUpdate", "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BobberBar), nameof(BobberBar.draw), new Type[] { typeof(SpriteBatch) }),
                    transpiler: new HarmonyMethod(typeof(FishingPatcher), nameof(draw_Transpiler))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "BobberBar.draw", "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)),
                    transpiler: new HarmonyMethod(typeof(FishingPatcher), nameof(performTenMinuteUpdate_Transpiler))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "GameLocation.performTenMinuteUpdate", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishPond), nameof(FishPond.doAction)),
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(doAction_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "FishPond.doAction", "prefixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.checkForAction)),
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(CrabPot_checkForAction_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "CrabPot.checkForAction", "prefixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.NeedsBait)),
                    postfix: new HarmonyMethod(typeof(FishingPatcher), nameof(NeedsBait_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "CrabPot.NeedsBait", "postfixing");
            }
        }

        public static void NeedsBait_Postfix(CrabPot __instance, Farmer player, ref bool __result)
        {
            if (TalentUtility.CurrentPlayerHasTalent("FishsWishes", who: player) && (Game1.GetPlayer(__instance.owner.Value) ?? player ?? Game1.player).professions.Contains(11))
            {
                __result = __instance.bait.Value is null;
            }
        }

        //I DID try transpiling. There were too many labels and editing the index directly just didnt work. Guess there are times you can't do everything Correctly:tm:
        public static bool CrabPot_checkForAction_Prefix(Farmer who, bool justCheckingForActivity, CrabPot __instance, ref int ___ignoreRemovalTimer, ref bool __result)
        {
            if (TalentUtility.AnyPlayerHasTalent("FishTrap") && __instance.heldObject.Value is not null)
            {
                if (DataLoader.Fish(Game1.content).TryGetValue(__instance.heldObject.Value?.ItemId, out string value) && !string.IsNullOrEmpty(value) && !value.Contains("trap") && __instance.tileIndexToShow == 714)
                {
                    if (justCheckingForActivity)
                    {
                        __result = true;
                        return false;
                    }
                    if (__instance.heldObject.Value != null)
                    {
                        if (who.IsLocalPlayer && !who.addItemToInventoryBool(__instance.heldObject.Value))
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                            __result = false;
                            return false;
                        }
                        if (DataLoader.Fish(Game1.content).TryGetValue(__instance.heldObject.Value.ItemId, out var rawDataStr))
                        {
                            string[] rawData = value.Split('/');
                            int minFishSize = int.TryParse(rawData[3], out int result_min) ? result_min: 1;
                            int maxFishSize = int.TryParse(rawData[4], out int result_max) ? result_max : 10;
                            who.caughtFish(__instance.heldObject.Value.QualifiedItemId, Game1.random.Next(minFishSize, maxFishSize + 1), from_fish_pond: false, __instance.heldObject.Value.Stack);
                            who.gainExperience(1, 5);
                        }
                    }
                    __instance.heldObject.Value = null;
                    __instance.readyForHarvest.Value = false;
                    __instance.tileIndexToShow = 710;
                    __instance.lidFlapping = true;
                    __instance.lidFlapTimer = 60f;
                    __instance.bait.Value = null;
                    who.animateOnce(279 + who.FacingDirection);
                    __instance.Location.playSound("fishingRodBend");
                    DelayedAction.playSoundAfterDelay("coin", 500);
                    __instance.shake = Vector2.Zero;
                    __instance.shakeTimer = 0f;
                    ___ignoreRemovalTimer = 750;
                    __result = true;
                    return false;
                }
            }
            return true;
        }
        public static void doAction_Prefix(FishPond __instance, Farmer who)
        {
            if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", who: who) && who.CurrentItem is not null and Trinket trinket && trinket.QualifiedItemId is "(TR)FrogEgg")
            {
                if (__instance.modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out string str) && string.IsNullOrEmpty(str))
                {
                    __instance.modData[TalentCore.Key_HiddenBenefit_FrogEggs] = trinket.TrinketToString();
                }
            }
            if (TalentUtility.CurrentPlayerHasTalent("InTheWeeds", who:who) && who.CurrentItem is not null and StardewValley.Object obj && obj.QualifiedItemId is "(O)152" or "(O)153")
            {
                obj.Category = StardewValley.Object.FishCategory;
            }
        }

        public static void PullFishFromWater_Prefix(FishingRod __instance)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Fishing_It_Was_This_Big"))
            {
                if (__instance.recordSize)
                {
                    __instance.fishQuality++;
                    if (__instance.fishQuality is 3)
                        __instance.fishQuality = 4;
                }
            }
        }
        public static IEnumerable<CodeInstruction> performTenMinuteUpdate_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            foreach (var item in insns)
            {
                if (item.opcode.Equals(OpCodes.Ldc_I4_8))
                {
                    item.opcode = OpCodes.Call;
                    item.operand = AccessTools.Method(typeof(FishingPatcher), nameof(FishingPatcher.TryOverrideBubbleDistance));
                }
            }
            return insns;
        }
        public static int TryOverrideBubbleDistance() => TalentUtility.AnyPlayerHasTalent("Fishing_Bubble_Trouble") ? 5 : 8;

        public static bool TryOverrideFishPondChance(FishPond __instance, Random random, ref Item __result)
        {
            random ??= Game1.random;
            if (__instance.output.Value is not null)
            {
                if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", __instance.owner.Value) && (__instance.output.Value is null || __instance.output.Value.QualifiedItemId != "(O)812"))
                {
                    if (__instance.modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out string str) && !string.IsNullOrEmpty(str))
                    {
                        __instance.output.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(__instance.GetFishObject());
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("SpawningSeason", __instance.owner.Value) && (__instance.output.Value is null || __instance.output.Value.QualifiedItemId != "(O)812"))
                {
                    if (random.NextBool())
                    {
                        __result = __instance.output.Value;
                        return false;
                    }
                }
            }
            return true;
        }
        
        public static int TryOverrideChallengeFish() => TalentUtility.CurrentPlayerHasTalent("Fishing_One_Fish_Two_Fish") ? 4 : 3;
        public static IEnumerable<CodeInstruction> draw_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            foreach (var item in list)
            {
                if (item.opcode.Equals(OpCodes.Ldc_I4_3))
                {
                    item.opcode = OpCodes.Call;
                    item.operand = AccessTools.Method(typeof(FishingPatcher), nameof(TryOverrideChallengeFish));
                    break;
                }
            }
            return list;
        }
    }
}