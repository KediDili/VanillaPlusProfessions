using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;
using xTile.Tiles;

namespace VanillaPlusProfessions.Craftables
{
    public class CraftablePatcher
    {
        internal static Dictionary<string, List<Vector2>> ForageCropLocations = new();

        internal static string Key_VPPDeluxeForage = "KediDili.VanillaPlusProfessions/DeluxeForage";
        
        public static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                    transpiler: new(AccessTools.Method(typeof(CraftablePatcher), nameof(newDay_Transpiler)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crop.newDay'", "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnHarvestedForage)),
                    postfix: new(AccessTools.Method(typeof(CraftablePatcher), nameof(OnHarvestedForage_Postfix)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'GameLocation.OnHarvestedForage'", "postfixing");
            }
        }
        public static void OnHarvestedForage_Postfix(Farmer who, Object forage)
        {
            try
            {
                if (forage.modData.ContainsKey(Key_VPPDeluxeForage))
                {
                    who.gainExperience(2, 50);
                    if (ForageCropLocations.ContainsKey(forage.Location.NameOrUniqueName))
                        ForageCropLocations[forage.Location.NameOrUniqueName].Remove(forage.TileLocation);
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'GameLocation.OnHarvestedForage'", "postfixing");
            }
        }

        public static IEnumerable<CodeInstruction> newDay_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            var method = AccessTools.Method(typeof(Crop), nameof(Crop.isWildSeedCrop));
            int index = 0, count = 1;
            try
            {
                foreach (var ins in list)
                {
                    if (ins.operand is MethodInfo info && info == method)
                    {
                        if (count != 0)
                        {
                            count--;
                        }
                        else
                        {
                            object instruction = list[index + 1].operand;
                            list.Insert(index + 2, new(OpCodes.Ldarg_0));
                            list.Insert(index + 3, new(OpCodes.Ldc_I4_1));
                            list.Insert(index + 4, new(OpCodes.Call, AccessTools.Method(typeof(CraftablePatcher), nameof(IsVPPForageCrop))));
                            list.Insert(index + 5, new(OpCodes.Brtrue_S, instruction));
                            break;
                        }
                    }
                    index++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crop.newDay'", "transpiling");
            }
            return list;
        }

        public static bool IsVPPForageCrop(Crop crop, bool prepareForDestruction = true)
        {
            bool returnValue = crop.whichForageCrop.Value is "KediDili.VPPData.CP_DeluxeWildSpringSeeds" or "KediDili.VPPData.CP_DeluxeWildSummerSeeds" or "KediDili.VPPData.CP_DeluxeWildFallSeeds" or "KediDili.VPPData.CP_DeluxeWildWinterSeeds";
            if (returnValue && prepareForDestruction)
            {
                //Save the locations, when the farmer picks it, check this
                //Patching Crop.harvest doesn't work because these wild crops are destroyed upon yielding produce.
                ForageCropLocations.TryAdd(crop.currentLocation.NameOrUniqueName, new());
                ForageCropLocations[crop.currentLocation.NameOrUniqueName].Add(crop.tilePosition);
            }
            return !returnValue;
        }
    }
}
