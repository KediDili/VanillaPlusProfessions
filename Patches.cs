using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace VanillaPlusProfessions
{
    public static class Patches
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int number = 6;

            foreach (var item in instructions)
            {
                if (item.opcode.Equals(OpCodes.Ldc_I4_S) && item.OperandIs(10))
                {
                    if (number > 0)
                        item.operand = 20;
                    number--;
                }
                yield return item;
            }
        }
        /*public static void LevelUpMenu_Postfix(List<int> ___professionsToChoose, int skill, int level, LevelUpMenu __instance)
        {
            if (level is 15) //Can I somehow get rid of this?
            {
                __instance.isProfessionChooser = true;
                foreach (var item in ModEntry.Professions)
                    if (Game1.player.professions.Contains(item.Value.Requires) && skill == item.Value.Skill)
                        ___professionsToChoose.Add(item.Key);
            }
        }*/
        //Patched
        public static void answerDialogueAction_Postfix(string questionAndAnswer)
        {
            string[] answers = new string[] { "professionForget_farming", "professionForget_fishing", "professionForget_farming", "professionForget_mining", "professionForget_combat" };
            int whichSkill = -1;
            for (int i = 0; i < answers.Length; i++)
                if (questionAndAnswer.Contains(answers[i]))
                    whichSkill = i;
            if (whichSkill > -1)
            {
                foreach (var item in ModEntry.Managers[whichSkill].RelatedProfessions.Values)
                    Game1.player.professions.Remove(item.ID);
                //ModEntry.Managers[5].RemoveComboProfessions(whichSkill);
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
            if (__result is 10 || ModEntry.levelExperiences[0] < oldXP || newXP > ModEntry.levelExperiences[0])
                for (int i = 0; i < ModEntry.levelExperiences.Length; i++)
                    if (ModEntry.levelExperiences[i] < oldXP && newXP >= ModEntry.levelExperiences[i])
                    {
                        if (__result is -1)
                            __result = 10;
                        __result++; 
                    }
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
