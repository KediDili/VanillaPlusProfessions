using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.GameData.Objects;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public static class MiscPatcher
    {
        public static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.HospitalDeath)),
                    transpiler: new HarmonyMethod(typeof(MiscPatcher), nameof(Transpiler_2))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), nameof(Event.DefaultCommands.HospitalDeath), "transpiling");

            }

            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.MineDeath)),
                    transpiler: new HarmonyMethod(typeof(MiscPatcher), nameof(Transpiler_2))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), nameof(Event.DefaultCommands.MineDeath), "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryAddPrismaticButterfly)),
                   postfix: new HarmonyMethod(typeof(MiscPatcher), nameof(tryAddPrismaticButterfly))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), nameof(GameLocation.tryAddPrismaticButterfly), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.TryCreateBuffsFromData)),
                   postfix: new HarmonyMethod(typeof(MiscPatcher), nameof(TryCreateBuffsFromData_Postfix))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), nameof(StardewValley.Object.TryCreateBuffsFromData), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.readNote)),
                   postfix: new HarmonyMethod(typeof(MiscPatcher), nameof(readNote))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), nameof(GameLocation.readNote), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(Chest), nameof(Chest.SetSpecialChestType)),
                   postfix: new HarmonyMethod(typeof(MiscPatcher), nameof(SetSpecialChestType_Postfix))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "Chest.SetSpecialChestType", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Chest), nameof(Chest.ShowMenu)),
                    transpiler: new HarmonyMethod(typeof(MiscPatcher), nameof(ShowMenu_Transpiler))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "Chest.ShowMenu", "transpiling");
            }
        }
        public static IEnumerable<CodeInstruction> ShowMenu_Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            try
            {
                list.Reverse();
                foreach (var item in list)
                {
                    if (item.opcode == OpCodes.Ldnull)
                    {
                        item.opcode = OpCodes.Ldarg_0;
                        break;
                    }
                }
                list.Reverse();
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "Chest.ShowMenu", "transpiling");
            }
            return list;
        }
        public static void SetSpecialChestType_Postfix(Chest __instance)
        {
            try
            {
                if (__instance.QualifiedItemId == "(BC)216" && TalentUtility.AnyPlayerHasTalent("MiniFridgeBigSpace"))
                {
                    __instance.SpecialChestType = Chest.SpecialChestTypes.BigChest;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "Chest.SetSpecialChestType", "postfixed", true);
            }
        }
        public static string TrinketToString(this Trinket trinket)
        {
            return (trinket.GetEffect() as CompanionTrinketEffect).Variant.ToString();
        }
        public static Trinket StringToTrinket(this string trinket)
        {
            Trinket toReturn = new("(TR)FrogEgg", 0);
            (toReturn.GetEffect() as CompanionTrinketEffect).Variant = int.Parse(trinket);

            return toReturn;
        }

        public static IEnumerable<CodeInstruction> Transpiler_2(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> toReturn = insns.ToList();
            try
            {
                for (int i = 0; i < toReturn.Count; i++)
                {
                    if (toReturn[i].operand is not null and 15000 or 1000)
                    {
                        toReturn.Insert(i + 1, new(OpCodes.Call, AccessTools.Method(typeof(MiscPatcher), nameof(ReplaceGoldCost))));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "Event.DefaultCommands.HospitalDeath or Event.DefaultCommands.MineDeath", "transpiling");
            }           
            return toReturn;
        }

        public static int ReplaceGoldCost(int og) => TalentUtility.CurrentPlayerHasTalent("NarrowEscape") ? 200 : og; 
        
        public static void readNote(int which)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("LostAndFound") && Game1.player.stats.NotesFound >= which)
                {
                    string name = "";
                    int index = 0;
                    BuffEffects buffEffects = new();
                    switch (which)
                    {
                        case 0:  // 0 - Tips on Farming - +1 Farming
                            name = "Farming";
                            buffEffects.FarmingLevel.Value = 1;
                            index = 0;
                            break;
                        case 1: // 1 - A book by Marnie - +2 Farming?
                            name = "Farming";
                            buffEffects.FarmingLevel.Value = 2;
                            index = 0;
                            break;
                        case 2:  // 2 - On Foraging - +2 Foraging?
                            name = "Foraging";
                            buffEffects.ForagingLevel.Value = 2;
                            index = 5;
                            break;
                        case 3: // 3 - Fisherman act 1 - +1 Fishing?
                            name = "Fishing";
                            buffEffects.FishingLevel.Value = 1;
                            index = 1;
                            break;
                        case 4: // 4 - How deep the mines go? - +1 Mining 
                            name = "Mining";
                            buffEffects.MiningLevel.Value = 1;
                            index = 2;
                            break;
                        case 5:  // 5 - A note that hints on people will give you cooking recipes if you befriend them - Magnetism
                            name = "Magnetism";
                            buffEffects.MagneticRadius.Value = 40;
                            index = 8;
                            break;
                        case 6: // 6 - Scarecrows - +3 Farming
                            name = "Farming";
                            buffEffects.FarmingLevel.Value = 3;
                            index = 0;
                            break;
                        case 7: // 7 - The secret of Stardrop - Max Stamina?
                            name = "Max Stamina";
                            buffEffects.MaxStamina.Value = 40;
                            index = 16;
                            break;
                        case 8: // 8 - Journey of Prairie King - +2 Attack
                            name = "Attack";
                            buffEffects.Attack.Value = 2;
                            index = 11;
                            break;
                        case 9: // 9 - A study on Diamond yields - +3 Mining?
                            name = "Mining";
                            buffEffects.MiningLevel.Value = 3;
                            index = 2;
                            break;
                        case 10: // 10 - Brewmaster's Guide - +4 Farming
                            name = "Farming";
                            buffEffects.FarmingLevel.Value = 4;
                            index = 0;
                            break;
                        case 11:  // 11 - Mysteries of Dwarves - +3 Mining?
                            name = "Mining";
                            buffEffects.MiningLevel.Value = 3;
                            index = 2;
                            break;
                        case 12: // 12 - Hightlights from the book of yoba - +4 Foraging
                            name = "Foraging";
                            buffEffects.ForagingLevel.Value = 4;
                            index = 5;
                            break;
                        case 13: // 13 - Marriage Guide - +3 Luck
                            name = "Luck";
                            buffEffects.LuckLevel.Value = 3;
                            index = 4;
                            break;
                        case 14: // 14 - The fisherman act 2 - +3 Fishing?
                            name = "Fishing";
                            buffEffects.FishingLevel.Value = 3;
                            index = 1;
                            break;
                        case 15: // 15 - A note explaining how crystalariums work - +4 Mining?
                            name = "Mining";
                            buffEffects.MiningLevel.Value = 4;
                            index = 2;
                            break;
                        case 16: // 16 - Secrets of the Legendary Fish - +4 Fishing
                            name = "Fishing";
                            buffEffects.FishingLevel.Value = 4;
                            index = 1; 
                            break;
                        case 17: // 17 - A note that hints at Qi's casino quest - +4 Luck
                            name = "Luck";
                            buffEffects.LuckLevel.Value = 4;
                            index = 4;
                            break;
                        case 18: // 18 - Note From Gunther - +4 Speed
                            name = "Speed";
                            buffEffects.Speed.Value = 4;
                            index = 9;
                            break;
                        case 19: // 19 - Goblins by Jasper - +4 Defense
                            name = "Defense";
                            buffEffects.Defense.Value = 4;
                            index = 10;
                            break;
                        case 20: // 20 - Easter Egg book - +4 Speed
                            name = "Speed";
                            buffEffects.Speed.Value = 4;
                            index = 9;
                            break;
                        default:
                            break;
                    }

                    Game1.player.buffs.Apply(new("Kedi.VPP.LostAndFound", "Lost Books", "Lost Books", -2, Game1.buffsIcons, index, buffEffects, false, name, ""));

                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "GameLocation.readNote", "postfixed", true);
            }
        }

        public static void tryAddPrismaticButterfly(GameLocation __instance)
        {
            try
            {
                bool anyPrismaticButterflies = false;

                if (TalentUtility.HostHasTalent("ButterflyEffect") && !anyPrismaticButterflies && Game1.player.team.sharedDailyLuck.Value < -0.02)
                {
                    foreach (Critter critter in __instance.critters)
                    {
                        if (critter is VoidButterfly)
                        {
                            return;
                        }
                    }
                    Random r = Utility.CreateDaySaveRandom(Game1.player.UniqueMultiplayerID % 10000);
                    string[] possibleLocations = new string[] { "WitchSwamp", "BugLair", "Sewers", "PirateCove", "Railroad", "BusTunnel", "Mines121", "Caldera" };
                    string locationChoice = possibleLocations[r.Next(possibleLocations.Length)];
                    if (!__instance.Name.Equals(locationChoice))
                    {
                        return;
                    }
                    Vector2 prism_v = __instance.getRandomTile(r);
                    for (int i = 0; i < 32; i++)
                    {
                        if (__instance.isTileLocationOpen(prism_v))
                        {
                            break;
                        }
                        prism_v = __instance.getRandomTile(r);
                    }
                    __instance.critters.Add(new VoidButterfly(__instance, prism_v)
                    {
                        stayInbounds = true
                    });
                }

            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "GameLocation.tryAddPrismaticButterfly", "postfixed", true);
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var item in codeInstructions)
            {
                yield return item;
                if (item.opcode == OpCodes.Ldc_I4_S && item.operand is -2 or -8 or -20)
                {
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(MiscPatcher), nameof(TryOverrideFriendshipDecay)));
                }
            }
        }

        public static int TryOverrideFriendshipDecay(int oldDecay)
        {
            if (Game1.player.mailReceived.Contains("Misc_Admiration"))
            {
                return oldDecay / 2;
            }
            return oldDecay;
        }

        public static void TryCreateBuffsFromData_Postfix(IEnumerable<Buff> __result, ObjectData obj)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("Misc_GoodEats") && __result.Any())
                {
                    string bufftype = "";
                    foreach (var item in __result)
                    {
                        if (item.effects.Immunity.Value > 0)
                            bufftype = "immunity";
                        else if (item.effects.MaxStamina.Value > 0)
                            bufftype = "maxstamina";
                        else if (item.effects.Attack.Value > 0)
                            bufftype = "attack";
                        else if (item.effects.Defense.Value > 0)
                            bufftype = "defense";
                        else if (item.effects.Speed.Value > 0)
                            bufftype = "speed";
                        else if (item.effects.MagneticRadius.Value > 0)
                            bufftype = "magnet";

                        else if (item.effects.WeaponPrecisionMultiplier.Value > 0)
                            bufftype = "weaponprecision";
                        else if (item.effects.WeaponSpeedMultiplier.Value > 0)
                            bufftype = "weaponspeed";
                        else if (item.effects.KnockbackMultiplier.Value > 0)
                            bufftype = "knockback";
                        else if (item.effects.AttackMultiplier.Value > 0)
                            bufftype = "attackmultiplier";
                        else if (item.effects.CriticalChanceMultiplier.Value > 0)
                            bufftype = "critchancemultiplier";
                        else if (item.effects.CriticalPowerMultiplier.Value > 0)
                            bufftype = "critpowermultiplier";

                        else if (item.effects.FarmingLevel.Value > 0)
                            bufftype = "farming";
                        else if (item.effects.FishingLevel.Value > 0)
                            bufftype = "fishing";
                        else if (item.effects.ForagingLevel.Value > 0)
                            bufftype = "foraging";
                        else if (item.effects.MiningLevel.Value > 0)
                            bufftype = "mining";
                        else if (item.effects.CombatLevel.Value > 0)
                            bufftype = "combat";
                        else if (item.effects.LuckLevel.Value > 0)
                            bufftype = "luck";

                        AccessTools.Field(typeof(Buff), "id").SetValue(item, item.id + "_" + bufftype);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(MiscPatcher), "StardewValley.Object.TryCreateBuffsFromData", "postfixed", true);
            }
        }
    }
}
