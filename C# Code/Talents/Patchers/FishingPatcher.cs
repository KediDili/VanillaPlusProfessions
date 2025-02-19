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

namespace VanillaPlusProfessions.Talents.Patchers
{
    public class FishingPatcher
    {
        readonly static string PatcherName = nameof(FishingPatcher);
        readonly static Type PatcherType = typeof(FishingPatcher);

        internal static void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                PatcherName, "FishingRod.pullFishFromWater",
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.pullFishFromWater)),
                prefix: new HarmonyMethod(PatcherType, nameof(PullFishFromWater_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "FishPond.GetFishProduce",
                original: AccessTools.Method(typeof(FishPond), nameof(FishPond.GetFishProduce)),
                prefix: new HarmonyMethod(PatcherType, nameof(TryOverrideFishPondChance))
            );
            CoreUtility.PatchMethod(
                PatcherName, "BobberBar.draw",
                original: AccessTools.Method(typeof(BobberBar), nameof(BobberBar.draw), new Type[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(PatcherType, nameof(draw_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.performTenMinuteUpdate",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)),
                transpiler: new HarmonyMethod(PatcherType, nameof(performTenMinuteUpdate_Transpiler))
            );
            CoreUtility.PatchMethod(
                PatcherName, "FishPond.doAction",
                original: AccessTools.Method(typeof(FishPond), nameof(FishPond.doAction)),
                prefix: new HarmonyMethod(PatcherType, nameof(doAction_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "CrabPot.checkForAction",
                original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.checkForAction)),
                prefix: new HarmonyMethod(PatcherType, nameof(CrabPot_checkForAction_Prefix))
            );

            CoreUtility.PatchMethod(
                PatcherName, "CrabPot.NeedsBait",
                original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.NeedsBait)),
                postfix: new HarmonyMethod(PatcherType, nameof(NeedsBait_Postfix))
            );
        }

        public static void NeedsBait_Postfix(CrabPot __instance, Farmer player, ref bool __result)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("FishsWishes", who: player) && (Game1.GetPlayer(__instance.owner.Value) ?? player ?? Game1.player).professions.Contains(11))
                {
                    __result = __instance.bait.Value is null;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CrabPot.NeedsBait", "postfixed", true);
            }
        }

        //I DID try transpiling. There were too many labels and editing the index directly just didnt work. Guess there are times you can't do everything Correctly:tm:
        public static bool CrabPot_checkForAction_Prefix(Farmer who, bool justCheckingForActivity, CrabPot __instance, ref int ___ignoreRemovalTimer, ref bool __result)
        {
            try
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
                                int minFishSize = int.TryParse(rawData[3], out int result_min) ? result_min : 1;
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
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CrabPot.checkForAction", "prefixed", true);
            }
            return true;
        }
        public static bool doAction_Prefix(FishPond __instance, Farmer who)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", who: who) && who.CurrentItem is not null and Trinket trinket && trinket.QualifiedItemId is "(TR)FrogEgg")
                {
                    if (!__instance.modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out string str) || (__instance.modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out str) && string.IsNullOrEmpty(str)))
                    {
                        __instance.modData[TalentCore.Key_HiddenBenefit_FrogEggs] = trinket.TrinketToString();
                        AccessTools.Method(typeof(FishPond), "showObjectThrownIntoPondAnimation").Invoke(__instance, new object[] { who, trinket, null });
                        who.ActiveItem = null;
                        return false;
                        // add bool __result and return true here;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("InTheWeeds", who: who) && who.CurrentItem is not null and StardewValley.Object obj && obj.QualifiedItemId is "(O)152" or "(O)153")
                {
                    obj.Category = StardewValley.Object.FishCategory;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FishPond.doAction", "prefixed", true);
            }
            return true;
        }

        public static void PullFishFromWater_Prefix(FishingRod __instance)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("ItWasThisBig"))
                {
                    if (__instance.recordSize)
                    {
                        __instance.fishQuality++;
                        if (__instance.fishQuality is 3)
                            __instance.fishQuality = 4;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FishingRod.PullFishFromWater", "prefixed", true);
            }
        }
        public static IEnumerable<CodeInstruction> performTenMinuteUpdate_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            try
            {
                foreach (var item in insns)
                {
                    if (item.opcode.Equals(OpCodes.Ldc_I4_8))
                    {
                        item.opcode = OpCodes.Call;
                        item.operand = AccessTools.Method(PatcherType, nameof(TryOverrideBubbleDistance));
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.performTenMinuteUpdate", "transpiled", true);
            }
            return insns;
        }
        public static int TryOverrideBubbleDistance() => TalentUtility.AnyPlayerHasTalent("BubbleTrouble") ? 5 : 8;

        public static bool TryOverrideFishPondChance(FishPond __instance, Random random, ref Item __result)
        {
            try
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
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "FishPond.GetFishProduce", "prefixed", true);
            }
            return true;
        }
        
        public static int TryOverrideChallengeFish() => TalentUtility.CurrentPlayerHasTalent("Fishing_One_Fish_Two_Fish") ? 4 : 3;
        public static IEnumerable<CodeInstruction> draw_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            try
            {
                foreach (var item in list)
                {
                    if (item.opcode.Equals(OpCodes.Ldc_I4_3))
                    {
                        item.opcode = OpCodes.Call;
                        item.operand = AccessTools.Method(PatcherType, nameof(TryOverrideChallengeFish));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "BobberBar.draw", "transpiled", true);
            }
            return list;
        }
    }
}