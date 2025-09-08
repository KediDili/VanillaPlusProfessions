using HarmonyLib;
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

namespace VanillaPlusProfessions.Managers
{
    public class CombatManager : IProfessionManager
    {
        public int SkillValue => 4;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        readonly static string PatcherName = nameof(CombatManager);
        readonly static System.Type PatcherType = typeof(CombatManager);

        internal static bool OnCrit;
        internal static Monster Rended;

        public void ApplyPatches()
        {
            Type[] monsters = { typeof(Grub), typeof(Mummy), typeof(Bug), typeof(RockCrab) };
            for (int i = 0; i < monsters.Length; i++)
            {
                CoreUtility.PatchMethod(
                    PatcherName, monsters[i].Name + ".takeDamage",
                    original: AccessTools.Method(monsters[i], "takeDamage", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                    postfix: new HarmonyMethod(PatcherType, "takeDamage_Postfix_" + monsters[i].Name)
                );
            }
            CoreUtility.PatchMethod(
                PatcherName, "MeleeWeapon.doAnimateSpecialMove",
                original: AccessTools.Method("StardewValley.Tools.MeleeWeapon:doAnimateSpecialMove"),
                postfix: new HarmonyMethod(PatcherType, nameof(doAnimateSpecialMove_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Farmer.CanBeDamaged",
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.CanBeDamaged)),
                postfix: new HarmonyMethod(PatcherType, nameof(CanBeDamaged_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.damageMonster",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool) }),
                transpiler: new HarmonyMethod(PatcherType, nameof(Transpiler))
            );
        }
        
        public static void takeDamage_Postfix_Mummy(Mummy __instance, int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who, ref int __result)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Warrior, useThisInstead: who))
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
                            Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, __instance.Position, Color.BlueViolet, 10)
                            {
                                holdLastFrame = true,
                                alphaFade = 0.01f,
                                interval = 70f
                            }, __instance.currentLocation);
                            __instance.currentLocation.playSound("ghost");
                            __instance.deathAnimation();
                        }
                    }
                    __result = actualDamage;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Mummy.takeDamage", "postfixed", true);
            }
        }

        public static void takeDamage_Postfix_Bug(Bug __instance, int damage, int xTrajectory, int yTrajectory, double addedPrecision, Farmer who, ref int __result)
        {
            try
            {
                if (__instance.isArmoredBug.Value && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Warrior, useThisInstead: who))
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Bug.takeDamage", "postfixed", true);
            }
        }

        public static void takeDamage_Postfix_Grub(Grub __instance, NetBool ___pupating, int damage, int xTrajectory, int yTrajectory, ref int __result)
        {
            try
            {
                if (___pupating.Value && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Warrior))
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Grub.takeDamage", "postfixed", true);
            }
        }
        public static void takeDamage_Postfix_RockCrab(RockCrab __instance, NetBool ___shellGone, int damage, int xTrajectory, int yTrajectory, ref int __result)
        {
            try
            {
                if (!___shellGone.Value && __instance.Sprite.currentFrame % 4 == 0 && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Warrior))
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "RockCrab.takeDamage", "postfixed", true);
            }
        }
        public static void CanBeDamaged_Postfix(Farmer __instance, ref bool __result)
        {
            try
            {
                if (__instance.temporaryInvincibilityTimer == 0)
                {
                    if (ShouldBeInvincible(__instance))
                    {
                        TalentUtility.MakeFarmerInvincible(__instance);
                        __instance.currentLocation.debris.Add(new(ModEntry.GetMe().Helper.Translation.Get("Message.Dodge"), 5, __instance.StandingPixel.ToVector2(), Color.White, 1f, 0f));
                        __result = false;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Farmer.CanBeDamaged", "postfixed", true);
            }
        }

        public static bool ShouldBeInvincible(Farmer who)
        {
            bool HasTechnician = CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Technician, useThisInstead: who);
            bool DefenseCooldown = MeleeWeapon.defenseCooldown > 0 && MeleeWeapon.defenseCooldown < 4000;
            bool DaggerSpecialMove = MeleeWeapon.daggerCooldown > 0 && MeleeWeapon.daggerCooldown < 4000;
            bool ClubSpecialMove = MeleeWeapon.clubCooldown > 0 && MeleeWeapon.clubCooldown < 4000;
            bool TripleShotCooldown = TalentCore.TalentCoreEntry.Value.TripleShotCooldown > 0 && TalentCore.TalentCoreEntry.Value.TripleShotCooldown < 4000;

            bool HasSideStep = TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Sidestep, who: who);
            bool IsInSlimeHutch = who.currentLocation is SlimeHutch;
            bool IsWearingSlimeCharmer = who.isWearingRing("520");
            return (HasTechnician && DefenseCooldown && DaggerSpecialMove && ClubSpecialMove && TripleShotCooldown) || (Game1.random.NextBool(0.1) && HasSideStep && !(!IsWearingSlimeCharmer && IsInSlimeHutch));
        }

        public static void doAnimateSpecialMove_Postfix()
        {
            try
            {
                if (OnCrit && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Assailant))
                {
                    MeleeWeapon.clubCooldown = 0;
                    MeleeWeapon.daggerCooldown = 0;
                    MeleeWeapon.defenseCooldown = 0;
                    MeleeWeapon.attackSwordCooldown = 0;
                    TalentCore.TalentCoreEntry.Value.TripleShotCooldown = 0; 
                    Game1.playSound("objectiveComplete");
                    OnCrit = false;
                }
                if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Speedster))
                {
                    MeleeWeapon.clubCooldown /= 2;
                    MeleeWeapon.daggerCooldown /= 2;
                    MeleeWeapon.defenseCooldown /= 2;
                    MeleeWeapon.attackSwordCooldown /= 2;
                    TalentCore.TalentCoreEntry.Value.TripleShotCooldown /= 2;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "MeleeWeapon.doAnimateSpecialMove", "postfixed", true);
            }

        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var list = insns.ToList();
            try
            {
                var close = false;
                var found = false;
                var skip = 8;
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
                        list.Insert(index + 7, new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(TryOverrideVanillaDamage))));
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
                        list.Insert(index - 10, new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(TryOverrideVanillaCritChance))));
                        item.opcode = OpCodes.Brfalse;
                        list.Insert(index - 9, item);
                        found = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GameLocation.damageMonster", "transpiled", true);
            }
            return list;
        }

        public static int TryOverrideVanillaDamage(int minDamage, int maxDamage, int vanillaDamage, MeleeWeapon weapon, Farmer who, Monster monster)
        {
            try
            {
                int addedMaxDamage = 0;
                int addedMinDamage = 0;
                int result = 0;
                if (who.health <= who.maxHealth / 4 && CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Berserker, useThisInstead:who))
                {
                    addedMaxDamage += maxDamage;
                    addedMinDamage += minDamage;
                }
                if (TalentUtility.isFavoredMonster(monster, who))
                {
                    addedMaxDamage += maxDamage;
                    addedMinDamage += minDamage;
                }
                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Flurry, who: who) && weapon is not null && weapon.type.Value is 1)
                {
                    if (monster.modData.TryGetValue(Constants.Key_Flurry, out string val))
                    {
                        if (val is not "5")
                        {
                            monster.modData[Constants.Key_Flurry] = (int.Parse(val) + 1).ToString();
                            addedMaxDamage += (int.Parse(val) + 1) * 2;
                            addedMinDamage += (int.Parse(val) + 1) * 2;
                        }
                        else
                        {
                            monster.modData[Constants.Key_Flurry] = "1";
                        }
                    }
                    else
                    {
                        monster.modData.TryAdd(Constants.Key_Flurry, "1");
                    }
                }

                result = addedMaxDamage is 0 || addedMinDamage is 0 ? vanillaDamage : Game1.random.Next(minDamage + addedMinDamage, maxDamage + addedMaxDamage + 1);

                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_DebiliatingStab, who: who) && weapon is not null && weapon.type.Value is 1 && monster.Speed > 1)
                {
                    monster.Speed--;
                }
                else if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_SeveringSwipe, who: who) && weapon is not null && weapon.type.Value is 3 && monster.DamageToFarmer > 0)
                {
                    monster.DamageToFarmer = (int)(monster.DamageToFarmer * 0.90f);
                }
                else if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_ConcussiveImpact, who: who) && weapon is not null && weapon.type.Value is 2)
                {
                    monster.startGlowing(Color.Red, false, 0.5f);
                    Monster fuckDelegates = monster;
                    if (monster.Health - (result / 4) > 0 && monster.Health > 0 && result / 4 > 0)
                    {
                        DelayedAction.functionAfterDelay(() =>
                        {
                            if (monster.Health > 0)
                            {
                                who.currentLocation.debris.Add(new Debris(result / 4, monster.StandingPixel.ToVector2(), Color.White, 3f, monster));
                                TalentUtility.ApplyExtraDamage(monster, who, result / 4);
                            }
                        }, 1000);
                    }
                    if ((monster.Health - ((result / 4) + (result / 6))) > 0 && monster.Health > 0 && ((result / 4) + (result / 6)) > 0)
                    {
                        DelayedAction.functionAfterDelay(() =>
                        {
                            if (monster.Health > 0)
                            {
                                who.currentLocation.debris.Add(new Debris(result / 6, monster.StandingPixel.ToVector2(), Color.White, 2f, monster));
                                TalentUtility.ApplyExtraDamage(monster, who, result / 6);
                            }
                        }, 2000);
                    }

                    if ((monster.Health - ((result / 6) + (result / 4) + (result / 8))) > 0 && monster.Health > 0 && (result / 6) + (result / 4) + (result / 8) > 0)
                    {
                        DelayedAction.functionAfterDelay(() =>
                        {
                            if (monster.Health > 0)
                            {
                                who.currentLocation.debris.Add(new Debris(result / 8, monster.StandingPixel.ToVector2(), Color.White, 1f, monster));
                                TalentUtility.ApplyExtraDamage(monster, who, result / 8);
                            }
                        }, 3000);
                    }
                    monster.stopGlowing();
                }

                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Aftershock, who: who) && weapon is not null && weapon.type.Value is 2 && monster.Health > 0)
                {
                    if (monster.MaxHealth - (result / 10) > 0)
                    {
                        who.currentLocation.debris.Add(new Debris(result / 10, monster.StandingPixel.ToVector2(), Color.White, 1f, who));
                        TalentUtility.ApplyExtraDamage(monster, who, result / 10);
                    }
                }
                else if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Champion, who: who) && weapon is not null && weapon.type.Value is 3)
                {
                    BuffEffects sdsdsd = new();
                    sdsdsd.Defense.Value = 2;

                    Buff buff = new("VPP.Champion.Defense", "Champion talent", "Champion talent", 6000, Game1.buffsIcons, 10, sdsdsd, false, ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Champion.Name"), Game1.parseText(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Champion.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Champion.Name"))));
                    who.buffs.Apply(buff);
                }

                if (TalentUtility.CurrentPlayerHasTalent(Constants.Talent_RendingStrike, who: who))
                {
                    if (Rended is null || Rended != monster)
                    {
                        Rended = monster;
                    }
                    else
                    {
                        if (!monster.modData.TryAdd(Constants.Key_RendingStrike, "1"))
                        {
                            monster.modData[Constants.Key_RendingStrike] = (int.Parse(monster.modData[Constants.Key_RendingStrike]) + 1).ToString();
                            if (monster.modData[Constants.Key_RendingStrike] == "4")
                            {
                                monster.DamageToFarmer -= 10;
                                monster.modData[Constants.Key_RendingStrike] = "0";
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CombatManager.TryOverrideVanillaDamage", "<individual method, transpiled in>", true);
            }
            return vanillaDamage;
        }

        public static bool TryOverrideVanillaCritChance(Farmer who, float critChance, Monster monster)
        {
            try
            {
                if (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Assassin, useThisInstead: who))
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
                if (OnCrit && TalentUtility.CurrentPlayerHasTalent(Constants.Talent_Ferocity, who: who))
                {
                    BuffEffects buffEffects2 = new();
                    buffEffects2.CriticalPowerMultiplier.Value += 0.1f;
                    who.buffs.Apply(new("VPP.Ferocity.Speed", "VPP.Ferocity.Talent", "Ferocity", 10000, Game1.buffsIcons, 11, buffEffects2, false, ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Ferocity.Name"), Game1.parseText(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Ferocity.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.CoreModEntry.Value.Helper.Translation.Get("Buff.Ferocity.Name")))));
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CombatManager.TryOverrideVanillaCritChance", "<individual method>", true);
            }
            
            return OnCrit;
        }
    }
}
