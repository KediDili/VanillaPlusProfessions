using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    public class CraftablePatcher
    {
        internal static Dictionary<string, List<Vector2>> ForageCropLocations = new();
        public static void ApplyPatches()
        {
            try
            {
                CoreUtility.PatchMethod(
                    "CraftablePatcher", "Crop.newDay",
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
                CoreUtility.PatchMethod(
                    "CraftablePatcher", "Crow.update",
                    original: AccessTools.Method(typeof(Crow), nameof(Crow.update)),
                    transpiler: new(AccessTools.Method(typeof(CraftablePatcher), nameof(update_Transpiler)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crow.update'", "transpiling");
            }
            try
            {
                CoreUtility.PatchMethod(
                    "CraftablePatcher", "GameLocation.OnHarvestedForage",
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnHarvestedForage)),
                    postfix: new(AccessTools.Method(typeof(CraftablePatcher), nameof(OnHarvestedForage_Postfix)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'GameLocation.OnHarvestedForage'", "postfixing");
            }
            try
            {
                CoreUtility.PatchMethod("CraftablePatcher", "FarmAnimal.Eat",
                    original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.Eat)),
                    postfix: new(AccessTools.Method(typeof(CraftablePatcher), nameof(Eat_Postfix)))
                );
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'GameLocation.OnHarvestedForage'", "postfixing");
            }
        }
        
        public static void Eat_Postfix(FarmAnimal __instance)
        {
            if (Game1.GetPlayer(__instance.ownerID.Value) is Farmer who && who.GetUnmodifiedSkillLevel(3) >= 16 & Game1.random.NextBool(0.02))
            {
                var list = (from asd in DataLoader.Objects(Game1.content)
                            where asd.Value.Category == StardewValley.Object.SeedsCategory && TalentUtility.EligibleForCropPerks(asd.Key, Constants.LevelPerk_Foraging_16)
                            select asd.Key).ToList();
                           
                Game1.createObjectDebris(Game1.random.ChooseFrom(list), (int)__instance.Tile.X, (int)__instance.Tile.Y, __instance.ownerID.Value);
            }
        }

        public static IEnumerable<CodeInstruction> update_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            int index = 0, count = 3;
            try
            {
                foreach (var ins in list)
                {
                    if (ins.opcode == OpCodes.Ldc_I4_5)
                    {
                        count--;
                        if (count == 0)
                        {
                            ins.opcode = OpCodes.Ldc_I4_4;
                        }
                    }
                    index++;
                }
            }
            catch (System.Exception e)
            {
                CoreUtility.PrintError(e, nameof(CraftablePatcher), "'Crow.update'", "transpiling");
            }
            return list;
        }

        public static void OnHarvestedForage_Postfix(Farmer who, Object forage)
        {
            try
            {
                if (forage.modData.ContainsKey(Constants.Key_VPPDeluxeForage))
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
                        if (count != 0d)
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
