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
using StardewValley.Tools;
using VanillaPlusProfessions.Utilities;

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
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(FishingPatcher.PullFishFromWater_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), nameof(FishingRod.pullFishFromWater), "prefixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(FishPond), nameof(FishPond.dayUpdate)),
                    transpiler: new HarmonyMethod(typeof(FishingPatcher), nameof(FishingPatcher.dayUpdate_Transpiler))
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
                    transpiler: new HarmonyMethod(typeof(FishingPatcher), nameof(FishingPatcher.draw_Transpiler))
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
                    transpiler: new HarmonyMethod(typeof(FishingPatcher), nameof(FishingPatcher.performTenMinuteUpdate_Transpiler))
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
                    prefix: new HarmonyMethod(typeof(FishingPatcher), nameof(FishingPatcher.doAction_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(FishingPatcher), "FishPond.doAction", "prefixing");
            }
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

        public static bool TryOverrideFishPondChance(FishPond fishPond, Random r)
        {
            if (fishPond.output.Value is not null)
            {
                if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", fishPond.owner.Value) && (fishPond.output.Value is null || fishPond.output.Value.QualifiedItemId != "(O)812"))
                {
                    if (fishPond.modData.TryGetValue(TalentCore.Key_HiddenBenefit_FrogEggs, out string str) && !string.IsNullOrEmpty(str))
                    {
                        fishPond.output.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(fishPond.GetFishObject());
                        return false;
                    }
                }
                if (TalentUtility.CurrentPlayerHasTalent("SpawningSeason", fishPond.owner.Value) && (fishPond.output.Value is null || fishPond.output.Value.QualifiedItemId != "(O)812"))
                {
                    return r.NextBool();
                }
            }
            return r.NextDouble() < (double)Utility.Lerp(0.15f, 0.95f, fishPond.currentOccupants.Value / 10f);
        }
        public static IEnumerable<CodeInstruction> dayUpdate_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            int index = -1;
            var list = insns.ToList();
            foreach (var item in list)
            {
                index++;
                if (item.opcode.Equals(OpCodes.Stloc_1))
                {
                    list.RemoveRange(index + 2, 11);
                    list.Insert(index + 2, new(OpCodes.Call, AccessTools.Method(typeof(FishingPatcher), nameof(FishingPatcher.TryOverrideFishPondChance))));
                    list[index + 3].opcode = OpCodes.Brfalse_S;
                    list.Insert(index + 1, new(OpCodes.Ldarg_0));
                    break;
                }
            }
            return list;
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
                    item.operand = AccessTools.Method(typeof(FishingPatcher), nameof(FishingPatcher.TryOverrideChallengeFish));
                    break;
                }
            }
            return list;
        }
    }
}