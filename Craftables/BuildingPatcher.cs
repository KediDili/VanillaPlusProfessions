using HarmonyLib;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaPlusProfessions.Craftables
{
    internal class BuildingPatcher
    {
        public static void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.getExtraInfoForLevel)),
                postfix: new(AccessTools.Method(typeof(BuildingPatcher), nameof(getExtraInfoForLevel)))
            );
        }

        public static void getExtraInfoForLevel(int whichSkill, int whichLevel, ref List<string> __result)
        {
            if (whichSkill == 3)
            {
                if (whichLevel is 16)
                {
                    __result.Add(ModEntry.Helper.Translation.Get("ExtraInfo.MinecartRepository"));
                }
                else if (whichLevel is 18)
                {
                    __result.Add(ModEntry.Helper.Translation.Get("ExtraInfo.MineralCavern"));
                }
            }
        }
    }
}
