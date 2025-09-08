using HarmonyLib;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    internal class BuildingPatcher
    {
        public static void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                "BuildingPatcher", "LevelUpMenu.getExtraInfoForLevel",
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.getExtraInfoForLevel)),
                postfix: new(AccessTools.Method(typeof(BuildingPatcher), nameof(getExtraInfoForLevel)))
            );
            CoreUtility.PatchMethod(
                "BuildingPatcher", "Bush.GetShakeOffItem",
                original: AccessTools.Method(typeof(Bush), nameof(Bush.GetShakeOffItem)),
                postfix: new(AccessTools.Method(typeof(BuildingPatcher), nameof(GetShakeOffItem)))
            );
        }
        public static void GetShakeOffItem(Bush __instance, ref string __result)
        {
            if (__instance.Location.ParentBuilding?.buildingType.Value == Constants.Id_SecretGlade && __instance.tileSheetOffset.Value == 1)
            {
                Season season = __instance.Location.ParentBuilding.GetParentLocation().GetSeason();
                __result = season switch
                {
                    Season.Spring => "(O)296",
                    Season.Fall => "(O)410",
                    _ => null
                };
            }
        }

        public static void getExtraInfoForLevel(int whichSkill, int whichLevel, ref List<string> __result)
        {
            if (whichSkill == 3)
            {
                if (whichLevel is 16)
                {
                    __result.Add(ModEntry.CoreModEntry.Value.Helper.Translation.Get("ExtraInfo.MinecartRepository"));
                }
                else if (whichLevel is 18)
                {
                    __result.Add(ModEntry.CoreModEntry.Value.Helper.Translation.Get("ExtraInfo.MineralCavern"));
                }
            }
            if (whichSkill == 2)
            {
                if (whichLevel is 14)
                {
                    __result.Add(ModEntry.CoreModEntry.Value.Helper.Translation.Get("ExtraInfo.AnimalsDropSeeds"));
                }
                else if (whichLevel is 16)
                {
                    __result.Add(ModEntry.CoreModEntry.Value.Helper.Translation.Get("ExtraInfo.Sawmill"));
                }
                else if (whichLevel is 18)
                {
                    __result.Add(ModEntry.CoreModEntry.Value.Helper.Translation.Get("ExtraInfo.SecretGlade"));
                }
            }
        }
    }
}
