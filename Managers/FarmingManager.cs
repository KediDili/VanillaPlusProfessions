using HarmonyLib;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Managers
{
    public class FarmingManager : IProfessionManager
    {
        public int SkillValue => 0;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(NPC), nameof(NPC.getGiftTasteForThisItem)),
                    postfix: new HarmonyMethod(typeof(FarmingManager), nameof(getGiftTasteForThisItem_Postfix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingManager), nameof(NPC.getGiftTasteForThisItem), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(MilkPail), nameof(MilkPail.DoFunction)),
                    prefix: new HarmonyMethod(typeof(FarmingManager), nameof(DoFunction_Prefix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingManager), nameof(MilkPail.DoFunction), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(Shears), nameof(Shears.DoFunction)),
                   prefix: new HarmonyMethod(typeof(FarmingManager), nameof(DoFunction_Prefix))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(FarmingManager), nameof(Shears.DoFunction), "postfixing");
            }
        }
        public static void DoFunction_Prefix(Farmer who)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Caretaker", useThisInstead: who))
                who.Stamina += 4;
        }
        
        public static void getGiftTasteForThisItem_Postfix(NPC __instance, Item item, ref int __result)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Connoisseur"))
            {
                var obj = __instance.GetData();
                if (obj.CustomFields?.TryGetValue("Kedi.VPP.ExcludeFromConnoisseur", out var field) is true && item.Category is -26 && !item.HasContextTag("alcohol_item") && __result > 3 && (string.IsNullOrEmpty(field) || field.ToLower() == "false"))
                    __result = 0;
            }
        }
        
    }
}
