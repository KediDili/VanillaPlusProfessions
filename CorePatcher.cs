using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using VanillaPlusProfessions.Managers;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions
{
    public static class CorePatcher
    {
        internal static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getProfessionForSkill)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.getProfessionForSkill_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "Farmer.getProfessionForSkill", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkForLevelGain)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.checkForLevelGain_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "Farmer.checkForLevelGain", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Constructor(typeof(LevelUpMenu), new Type[] { typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.Transpiler)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "LevelUpMenu constructor", "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.Transpiler)))
                );
            }
            catch (Exception e) 
            { 
                CoreUtility.PrintError(e, nameof(CorePatcher), "GameLocation.performAction", "transpiling");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(LevelUpMenu), "getProfessionName"),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.getProfessionName_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "LevelUpMenu.getProfessionName", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.answerDialogueAction_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "GameLocation.answerDialogueAction", "postfixing");
            }
            
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.canRespec)),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.canRespec_Postfix)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "GameLocation.canRespec", "postfixing");
            }

            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(CorePatcher), nameof(CorePatcher.gainExperience_Transpiler)))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CorePatcher), "Farmer.gainExperience", "transpiling");
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int number = 6;
            MethodInfo replacement = AccessTools.Method(typeof(CoreUtility), nameof(CoreUtility.GetMaxLevel));

            foreach (var item in instructions)
            {
                if (item.opcode.Equals(OpCodes.Ldc_I4_S) && item.OperandIs(10))
                {
                    if (number > 0)
                    {
                        item.opcode = OpCodes.Call;
                        item.operand = replacement;
                    }
                    number--;
                }
                yield return item;
            }
        }
        public static bool TryOverride() //Listen, I'm lazy and this works anyway, there's label business I don't want to fiddle with
        {
            return ModEntry.Helper.ModRegistry.IsLoaded("KediDili.VanillaPlusProfessions");
        }

        //Patched
        public static void answerDialogueAction_Postfix(string questionAndAnswer)
        {
            string[] answers = new string[] { "professionForget_farming", "professionForget_fishing", "professionForget_foraging", "professionForget_mining", "professionForget_combat" };
            int whichSkill = -1;
            for (int i = 0; i < answers.Length; i++)
                if (questionAndAnswer.Contains(answers[i]))
                    whichSkill = i;
            if (whichSkill > -1)
            {
                foreach (var item in ModEntry.Professions.Values)
                    if (item.Skill == whichSkill)
                        Game1.player.professions.Remove(item.ID);

                int level = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[whichSkill]);
                if (level >= 15)
                {
                    Game1.player.newLevels.Add(new Point(whichSkill, 15));
                    if (level >= 20)
                        Game1.player.newLevels.Add(new Point(whichSkill, 20));
                }
            }
        }
        //Patched
        public static void canRespec_Postfix(int skill_index, ref bool __result)
        {
            if (Game1.player.newLevels.Contains(new Point(skill_index, 15)) || Game1.player.newLevels.Contains(new Point(skill_index, 20)))
            {
                __result = false;
            }
        }
        //Patched
        public static void getProfessionName_Postfix(int whichProfession, ref string __result)
        {
            foreach (var item in ModEntry.Professions)
            {
                if (item.Value.ID == whichProfession)
                {
                    __result = item.Key;
                }
            }
        }
        //Patched
        public static void getProfessionForSkill_Postfix(Farmer __instance, int skillType, int skillLevel, ref int __result)
        {
            int result = skillLevel / 5;
            if (result == 3)
            {
                foreach (var item in ModEntry.Managers[skillType].RelatedProfessions)
                    if (__instance.professions.Contains(item.Value.Requires))
                        __result = item.Value.ID;
            }
            else if (result == 4)
                foreach (var item in ModEntry.Managers[6].RelatedProfessions)
                    if (__instance.professions.Contains(item.Value.ID))
                        __result = item.Value.ID;
        }
        //Patched
        public static void checkForLevelGain_Postfix(ref int __result, int oldXP, int newXP)
        {
            for (int level = 1; level <= 10; level++)
            {
                if (oldXP < ModEntry.levelExperiences[level - 1] && newXP >= ModEntry.levelExperiences[level - 1])
                {
                    //but I cant leave it like this, otherwise players will keep getting level up messages

                    __result = level + 10;
                }
            }
        }

        public static IEnumerable<CodeInstruction> gainExperience_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            MethodInfo methodInfo = AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Level));
            MethodInfo replacement = AccessTools.Method(typeof(CorePatcher), nameof(TryOverrideVanillaLevel));
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call && (MethodInfo)list[i].operand == methodInfo)
                {
                    list[i].operand = replacement;
                    break;
                }
            }
            return list;
        }
        
        public static int TryOverrideVanillaLevel(Farmer who)
        {
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                result += who.GetUnmodifiedSkillLevel(i) / (CoreUtility.GetMaxLevel() / 5);
            }
            return result;
        }

        //100 = 1Lv - 100     | 21000 = 11Lv - 6000
        //380 = 2Lv - 280     | 28000 = 12Lv - 7000
        //770 = 3Lv - 390     | 36500 = 13Lv - 8500
        //1300 = 4Lv - 530    | 50000 = 14v - 9500
        //2150 = 5Lv - 850    | 60000 = 15Lv - 10000
        //3300 = 6Lv - 1150   | 72000 = 16Lv - 12000
        //4800 = 7Lv - 1500   | 85500 = 17Lv - 13500
        //6900 = 8Lv - 2100   | 101000 = 18Lv - 15500
        //10000 = 9Lv - 3100  | 118000 = 19Lv - 17000
        //15000 = 10Lv - 5000 | 138000 = 20Lv - 20000
    }
}
