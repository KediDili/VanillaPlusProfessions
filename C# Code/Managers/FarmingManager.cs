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

        readonly static string PatcherName = nameof(FarmingManager);
        readonly static System.Type PatcherType = typeof(FarmingManager);

        public void ApplyPatches()
        {
            System.Type[] tools = { typeof(MilkPail), typeof(Shears) };
            for (int i = 0; i < tools.Length; i++)
            {
                CoreUtility.PatchMethod(
                    PatcherName, tools[i].Name + ".DoFunction",
                    original: AccessTools.Method(tools[i], nameof(MilkPail.DoFunction)),
                    prefix: new HarmonyMethod(PatcherType, nameof(DoFunction_Prefix))
                );
            }
            CoreUtility.PatchMethod(
                PatcherName, "NPC.getGiftTasteForThisItem",
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getGiftTasteForThisItem)),
                postfix: new HarmonyMethod(PatcherType, nameof(getGiftTasteForThisItem_Postfix))
            );
        }
        public static void DoFunction_Prefix(Farmer who)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Caretaker, useThisInstead: who))
                    who.Stamina += 4;
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "<VanillaTool>.DoFunction", "postfixed", true);
            }
        }
        
        public static void getGiftTasteForThisItem_Postfix(NPC __instance, Item item, ref int __result)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Connoisseur))
                {
                    var obj = __instance.GetData();
                    string field = "false";
                    if (item.Category is -26 && !item.HasContextTag("alcohol_item") && (__result == 0 || __result == 8 || __result == 2))
                    {
                        if (obj.CustomFields?.TryGetValue("Kedi.VPP.ExcludeFromConnoisseur", out field) is true && (field?.ToLower() == "true"))
                            __result = 0;
                    }
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "NPC.getGiftTasteForThisItem", "postfixed", true);
            }
        }
        
    }
}
