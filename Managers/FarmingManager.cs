using HarmonyLib;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;

namespace VanillaPlusProfessions.Managers
{
    public class FarmingManager : IProfessionManager
    {
        public int SkillValue => 0;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getGiftTasteForThisItem)),
                postfix: new HarmonyMethod(typeof(FarmingManager), nameof(FarmingManager.getGiftTasteForThisItem_Postfix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(MilkPail), nameof(MilkPail.DoFunction)),
                prefix: new HarmonyMethod(typeof(FarmingManager), nameof(FarmingManager.DoFunction_Prefix))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Shears), nameof(Shears.DoFunction)),
                prefix: new HarmonyMethod(typeof(FarmingManager), nameof(FarmingManager.DoFunction_Prefix))
            );            
        }
        public static void DoFunction_Prefix(Farmer who)
        {
            if (CoreUtility.CurrentPlayerHasProfession(33, who))
                who.Stamina += 4;
        }
        
        public static void getGiftTasteForThisItem_Postfix(NPC __instance, Item item, ref int __result)
        {
            if (CoreUtility.CurrentPlayerHasProfession(35))
            {
                var obj = __instance.GetData();
                obj.CustomFields.TryGetValue("Kedi.SMP.ExcludeFromConnoisseur", out var field); 
                if (item.Category is -26 && !item.HasContextTag("alcohol_item") && __result > 3 && (string.IsNullOrEmpty(field) || field.ToLower() == "false"))
                    __result = 0;
            }
        }
    }
}
