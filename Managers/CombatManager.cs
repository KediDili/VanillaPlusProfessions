using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Monsters;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Tools;
using System.Reflection.Emit;
using System;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Utilities;
using System.Linq;
using StardewValley.Extensions;
using StardewValley.Enchantments;

namespace VanillaPlusProfessions.Managers
{
    public class CombatManager : IProfessionManager
    {
        public int SkillValue => 4;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Grub), nameof(Grub.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(takeDamage_Postfix_Grub))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), nameof(Grub.takeDamage), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Bug), nameof(Bug.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(takeDamage_Postfix_Bug))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), nameof(Bug.takeDamage), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Mummy), nameof(Mummy.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(takeDamage_Postfix_Bug))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), nameof(Bug.takeDamage), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Bug), nameof(Bug.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(takeDamage_Postfix_Bug))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), "Bug.takeDamage", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(RockCrab), nameof(RockCrab.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(takeDamage_Postfix_RockCrab))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), "RockCrab.takeDamage", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method("StardewValley.Tools.MeleeWeapon:doAnimateSpecialMove"),
                    postfix: new HarmonyMethod(typeof(CombatManager), nameof(doAnimateSpecialMove_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), "'MeleeWeapon.doAnimateSpecialMove'", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.CanBeDamaged)),
                    prefix: new HarmonyMethod(typeof(CombatManager), nameof(CanBeDamaged_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), "'Farmer.CanBeDamaged'", "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool) }),
                    transpiler: new HarmonyMethod(typeof(CombatManager), nameof(Transpiler))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatManager), "GameLocation.damageMonster", "transpiling");
            }
        }

        internal static bool OnCrit;
        internal static Monster Rended;
        
        public static void takeDamage_Postfix_Mummy(Mummy __instance, int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who, ref int __result)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Warrior", useThisInstead: who))
            {
                int actualDamage = Math.Max(1, damage - __instance.resilience.Value);
                if (Game1.random.NextDouble() < __instance.missChance.Value - __instance.missChance.Value * addedPrecision)
                {
                    actualDamage = -1;
                }
                else
                {
                    __instance.Slipperiness = 2;
                    __instance.Health -= actualDamage;
                    __instance.setTrajectory(xTrajectory, yTrajectory);
                    __instance.currentLocation.playSound("shadowHit");
                    __instance.currentLocation.playSound("skeletonStep");
                    __instance.IsWalkingTowardPlayer = true;
                    if (__instance.Health <= 0)
                    {
                        if (!isBomb)
                        {
                            Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, __instance.Position, Color.BlueViolet, 10)
                            {
                                holdLastFrame = true,
                                alphaFade = 0.01f,
                                interval = 70f
                            }, __instance.currentLocation);
                            __instance.currentLocation.playSound("ghost");
                        }
                        else
                        {
                            __instance.reviveTimer.Value = 10000;
                            __instance.Health = __instance.MaxHealth;
                            __instance.deathAnimation();
                        }
                    }
                }
                __result = actualDamage;
            }
        }

        public static void takeDamage_Postfix_Bug(Bug __instance, int damage, int xTrajectory, int yTrajectory, double addedPrecision, Farmer who, ref int __result)
        {
            if (__instance.isArmoredBug.Value && CoreUtility.CurrentPlayerHasProfession("Warrior", useThisInstead: who))
            {
                int actualDamage = Math.Max(1, damage - __instance.resilience.Value);
                if (Game1.random.NextDouble() < __instance.missChance.Value - __instance.missChance.Value * addedPrecision)
                {
                    actualDamage = -1;
                }
                else
                {
                    __instance.Health -= actualDamage;
                    __instance.currentLocation.playSound("hitEnemy");
                    __instance.setTrajectory(xTrajectory / 3, yTrajectory / 3);
                    if (__instance.isHardModeMonster.Value)
                    {
                        __instance.FacingDirection = Math.Abs((__instance.FacingDirection + Game1.random.Next(-1, 2)) % 4);
                        __instance.Halt();
                        __instance.setMovingInFacingDirection();
                    }
                    if (__instance.Health <= 0)
                    {
                        __instance.deathAnimation();
                    }
                }
                __result = actualDamage;
            }
        }

        public static void takeDamage_Postfix_Grub(Grub __instance, NetBool ___pupating, int damage, int xTrajectory, int yTrajectory, ref int __result)
        {
            if (___pupating.Value && CoreUtility.CurrentPlayerHasProfession("Warrior"))
            {
                int actualDamage = Math.Max(1, damage - __instance.resilience.Value);
                __instance.Health -= actualDamage;
                __instance.setTrajectory(xTrajectory, yTrajectory);
                if (__instance.Health <= 0)
                {
                    __instance.currentLocation.playSound("slimedead");
                    Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, __instance.Position, __instance.isHardModeMonster.Value ? Color.LimeGreen : Color.Orange, 10)
                    {
                        holdLastFrame = true,
                        alphaFade = 0.01f,
                        interval = 50f
                    }, __instance.currentLocation);
                }
                __result = actualDamage;
            }
        }
        public static void takeDamage_Postfix_RockCrab(RockCrab __instance, NetBool ___shellGone, int damage, int xTrajectory, int yTrajectory, ref int __result)
        {
            if (!___shellGone.Value && __instance.Sprite.currentFrame % 4 == 0 && CoreUtility.CurrentPlayerHasProfession("Warrior"))
            {
                int actualDamage = Math.Max(1, damage - __instance.resilience.Value);
                __instance.Health -= actualDamage;
                __instance.Slipperiness = 3;
                __instance.setTrajectory(xTrajectory, yTrajectory);
                __instance.currentLocation.playSound("hitEnemy");
                __instance.glowingColor = Color.Cyan;
                if (__instance.Health <= 0)
                {
                    __instance.currentLocation.playSound("monsterdead");
                    __instance.deathAnimation();
                    Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, __instance.Position, Color.Red, 10)
                    {
                        holdLastFrame = true,
                        alphaFade = 0.01f
                    }, __instance.currentLocation);
                }
                __result = actualDamage;
            }
        }
        public static void CanBeDamaged_Postfix(Farmer __instance, ref bool __result)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Technician", useThisInstead: __instance) && ((MeleeWeapon.defenseCooldown > 0 && MeleeWeapon.defenseCooldown < 4000) || (MeleeWeapon.daggerCooldown > 0 && MeleeWeapon.daggerCooldown < 4000) || (MeleeWeapon.clubCooldown > 0 && MeleeWeapon.clubCooldown < 4000) || (MeleeWeapon.attackSwordCooldown < 4000 && MeleeWeapon.attackSwordCooldown > 0)))
            {
                TalentUtility.MakeFarmerInvincible(__instance);
                __result = false;
            }
            else if (Game1.random.NextBool(0.1) && TalentUtility.CurrentPlayerHasTalent("Combat_Sidestep", who: __instance) && !(__instance.isWearingRing("520") && __instance.currentLocation is SlimeHutch))
            {
                TalentUtility.MakeFarmerInvincible(__instance);
                __result = false;
            }
        }
        public static void doAnimateSpecialMove_Postfix()
        {
            if (OnCrit && CoreUtility.CurrentPlayerHasProfession("Assailant"))
            {
                MeleeWeapon.clubCooldown = 0;
                MeleeWeapon.daggerCooldown = 0;
                MeleeWeapon.defenseCooldown = 0;
                MeleeWeapon.attackSwordCooldown = 0;
                Game1.playSound("objectiveComplete");
                OnCrit = false;
            }
            if (CoreUtility.CurrentPlayerHasProfession("Speedster"))
            {
                MeleeWeapon.clubCooldown /= 2;
                MeleeWeapon.daggerCooldown /= 2;
                MeleeWeapon.defenseCooldown /= 2;
                MeleeWeapon.attackSwordCooldown /= 2;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var close = false;
            var found = false;
            var skip = 8;
            var list = insns.ToList();
            var index = 0;
            foreach (var item in list)
            {
                index++;
                if (item.opcode == OpCodes.Stloc_S && skip is 0)
                {
                    list.Insert(index + 1, new(OpCodes.Ldarg_2));
                    list.Insert(index + 2, new(OpCodes.Ldarg_3));
                    list.Insert(index + 3, new(OpCodes.Ldloc_S, 9));
                    list.Insert(index + 4, new(OpCodes.Ldloc_S, 4));
                    list.Insert(index + 5, new(OpCodes.Ldarg, 10));
                    list.Insert(index + 6, new(OpCodes.Ldloc_2));
                    list.Insert(index + 7, new(OpCodes.Call, AccessTools.Method(typeof(CombatManager), nameof(TryOverrideVanillaDamage))));
                    list.Insert(index + 8, new(OpCodes.Stloc_S, 9));
                    break;
                }
                else if (item.opcode == OpCodes.Stloc_S && skip is not 0)
                {
                    skip--;
                }
            }
            index = 0;
            skip = 4;
            foreach (var item in list)
            {
                index++;
                if (item.opcode == OpCodes.Ldsfld)
                {
                    if (skip is 0 && !close)
                    {
                         close = true;
                    }
                    else if (skip is not 0)
                    {
                        skip--;
                    }
                }
                if (item.opcode == OpCodes.Bge_Un && !found && close && skip is 0)
                {
                    //I don't like relying on a fixed index either, but I'm not sure how to make this more dynamic,
                    //so come to me with a 'you can use x method to make it better' instead of 'GASP, you're using a fixed index, this is wrong!' if you're bothered by this.
                    
                    list.RemoveRange(index - 13, 13);
                    list.Insert(index - 13, new(OpCodes.Ldarg, 10));
                    list.Insert(index - 12, new(OpCodes.Ldarg, 7));
                    list.Insert(index - 11, new(OpCodes.Ldloc_2));
                    list.Insert(index - 10, new(OpCodes.Call, AccessTools.Method(typeof(CombatManager), nameof(CombatManager.TryOverrideVanillaCritChance))));
                    item.opcode = OpCodes.Brfalse;
                    list.Insert(index - 9, item);
                    found = true;
                    break;
                }
            }
            
            return list;
        }

        public static int TryOverrideVanillaDamage(int minDamage, int maxDamage, int vanillaDamage, MeleeWeapon weapon, Farmer who, Monster monster)
        {
            int addedMaxDamage = 0;
            int addedMinDamage = 0;
            int result = 0;
            if (who.health <= who.maxHealth / 4 && who.professions.Contains(63))
            {
                addedMaxDamage += maxDamage;
                addedMinDamage += minDamage;
            }
            if (TalentUtility.isFavoredMonster(monster, who))
            {
                addedMaxDamage += maxDamage;
                addedMinDamage += minDamage;
            }
            if (TalentUtility.CurrentPlayerHasTalent("Combat_Flurry", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                if (monster.modData.TryGetValue(TalentCore.Key_Flurry, out string val))
                {
                    if (val is not "5")
                    {
                        monster.modData[TalentCore.Key_Flurry] = (int.Parse(val) + 1).ToString();
                        addedMaxDamage += (int.Parse(val) + 1) * 2;
                        addedMinDamage += (int.Parse(val) + 1) * 2;
                    }
                    else
                    {
                        monster.modData[TalentCore.Key_Flurry] = "1";
                    }
                }
                else
                {
                    monster.modData.TryAdd(TalentCore.Key_Flurry, "1");
                }
            }

            result = addedMaxDamage is 0 || addedMinDamage is 0 ? vanillaDamage : Game1.random.Next(minDamage + addedMinDamage, maxDamage + addedMaxDamage + 1);

            if (TalentUtility.CurrentPlayerHasTalent("Combat_DebiliatingStab", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                monster.Speed--;
            }
            else if (TalentUtility.CurrentPlayerHasTalent("Combat_SeveringSwipe", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                monster.DamageToFarmer = (int)(monster.DamageToFarmer * 0.60f);
            }
            else if (TalentUtility.CurrentPlayerHasTalent("Combat_ConcussiveImpact", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                monster.startGlowing(Color.Red, false, 0.5f);
                Monster fuckDelegates = monster;
                DelayedAction.functionAfterDelay(() =>
                {
                    monster.takeDamage(fuckDelegates.Health / 40, 1, 1, false, 1, who);
                }, 1000
                );
                Monster fuckDelegates2 = fuckDelegates;
                DelayedAction.functionAfterDelay(() =>
                {
                    fuckDelegates2.takeDamage(fuckDelegates2.Health / 60, 1, 1, false, 1, who);
                }, 1000
                );
                Monster fuckDelegates3 = fuckDelegates2;
                DelayedAction.functionAfterDelay(() =>
                {
                    monster.takeDamage(fuckDelegates3.Health / 80, 1, 1, false, 1, who);
                }, 1000
                );
                monster.stopGlowing();
            }
            
            if (TalentUtility.CurrentPlayerHasTalent("Combat_Aftershock", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                monster.takeDamage(result / 10, 1, 1, false, 1, who);
            }
            else if (TalentUtility.CurrentPlayerHasTalent("Combat_Champion", who: who) && weapon is not null && weapon.type.Value is 5)
            {
                BuffEffects sdsdsd = new();
                sdsdsd.Defense.Value = 2;
                
                Buff buff = new("VPP.Champion.Defense", "Champion talent", "Champion talent", 6000, Game1.buffsIcons, 10, sdsdsd, false, ModEntry.Helper.Translation.Get("Buff.Champion.Name"), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Champion.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.Helper.Translation.Get("Buff.Champion.Name"))));
                who.buffs.Apply(buff);
            }

            if (TalentUtility.CurrentPlayerHasTalent("Combat_RendingStrike", who: who))
            {
                if (Rended is null || Rended != monster)
                {
                    Rended = monster;
                }
                else
                {
                    if (!monster.modData.TryAdd(TalentCore.Key_RendingStrike, "1"))
                    {
                        monster.modData[TalentCore.Key_RendingStrike] = (int.Parse(monster.modData[TalentCore.Key_RendingStrike]) + 1).ToString();
                        if (monster.modData[TalentCore.Key_RendingStrike] == "4")
                        {
                            monster.DamageToFarmer -= 10;
                            monster.modData[TalentCore.Key_RendingStrike] = "0";
                        }
                    }
                }
            }

            foreach (var trinketRing in TalentUtility.GetAllTrinketRings(who))
            {
                trinketRing.Trinket.OnDamageMonster(who, monster, result, false, OnCrit);
            }

            return result;
        }

        public static bool TryOverrideVanillaCritChance(Farmer who, float critChance, Monster monster)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Assassin", useThisInstead:who))
            {
                if (monster is MetalHead or HotHead or DwarvishSentry or RockCrab)
                {
                    OnCrit = true;
                }
                else
                {
                    OnCrit = Game1.random.NextDouble() < (double)(critChance + who.LuckLevel * (critChance / 40f));
                }
            }
            else
            {
                OnCrit = Game1.random.NextDouble() < (double)(critChance + who.LuckLevel * (critChance / 40f));
            }
            if (OnCrit && TalentUtility.CurrentPlayerHasTalent("Combat_Ferocity", who: who))
            {
                BuffEffects buffEffects2 = new();
                buffEffects2.CriticalPowerMultiplier.Value += 0.1f;
                who.buffs.Apply(new("VPP.Ferocity.Speed", "VPP.Ferocity.Talent", "Ferocity", 10000, Game1.buffsIcons, 11, buffEffects2, false, ModEntry.Helper.Translation.Get("Buff.Ferocity.Name"), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Ferocity.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.Helper.Translation.Get("Buff.Ferocity.Name")))));
            }
            return OnCrit;
        }
    }
}
