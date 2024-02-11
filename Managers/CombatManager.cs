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

namespace VanillaPlusProfessions.Managers
{
    public class CombatManager : IProfessionManager
    {
        public int SkillValue => 4;

        public Dictionary<string, Profession> RelatedProfessions { get; set; } = new();

        public void ApplyPatches()
        {
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Grub), nameof(Grub.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                postfix: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.takeDamage_Postfix_Grub))
            );
            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(RockCrab), nameof(RockCrab.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                postfix: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.takeDamage_Postfix_RockCrab))
            );

            ModEntry.Harmony.Patch(
                original: AccessTools.Method("StardewValley.Tools.MeleeWeapon:doAnimateSpecialMove"),
                postfix: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.doAnimateSpecialMove_Postfix))
            );

            ModEntry.Harmony.Patch(
                original: AccessTools.Constructor(typeof(Buff), new Type[] { typeof(string), typeof(string), typeof(string), typeof(int), typeof(Texture2D), typeof(int), typeof(BuffEffects), typeof(bool?), typeof(string), typeof(string) }),
                postfix: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.Constructor_Postfix))
            );

            ModEntry.Harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
                prefix: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.takeDamage_Prefix))
            );
            //Rectangle, int, int, bool, float, int, float, float, bool, Farmer, bool
            if (Game1.versionLabel.Contains("beta"))
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new System.Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool) }),
                    transpiler: new HarmonyMethod(typeof(CombatManager), nameof(CombatManager.Transpiler))
                );
            }
        }

        internal static bool OnCrit;

        public static void takeDamage_Postfix_Grub(Grub __instance, NetBool ___pupating, int damage, int xTrajectory, int yTrajectory, ref int __result)
        {
            if (___pupating.Value && CoreUtility.CurrentPlayerHasProfession(62))
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
            if (!___shellGone.Value && __instance.Sprite.currentFrame % 4 == 0 && CoreUtility.CurrentPlayerHasProfession(62))
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
        public static void Constructor_Postfix(Buff __instance, bool? isDebuff = false)
        {
            int csds = (int)(__instance.millisecondsDuration * 0.2);

            if (isDebuff is true && __instance.id != "17" && CoreUtility.CurrentPlayerHasProfession(65))
                __instance.millisecondsDuration -= csds;

            else if (isDebuff is false && CoreUtility.CurrentPlayerHasProfession(64))
                __instance.millisecondsDuration += csds;
        }
        public static bool takeDamage_Prefix(Farmer __instance)
        {
            if (CoreUtility.CurrentPlayerHasProfession(66, __instance))
            {
                if (MeleeWeapon.defenseCooldown > 0 || MeleeWeapon.daggerCooldown > 0 || MeleeWeapon.clubCooldown > 0 || MeleeWeapon.attackSwordCooldown > 0)
                    return false;
            }
            return true;
        }
        public static void doAnimateSpecialMove_Postfix()
        {
            if (OnCrit && CoreUtility.CurrentPlayerHasProfession(69))
            {
                MeleeWeapon.clubCooldown = 0;
                MeleeWeapon.daggerCooldown = 0;
                MeleeWeapon.defenseCooldown = 0;
                MeleeWeapon.attackSwordCooldown = 0;
            }
            if (CoreUtility.CurrentPlayerHasProfession(67))
            {
                MeleeWeapon.clubCooldown /= 2;
                MeleeWeapon.daggerCooldown /= 2;
                MeleeWeapon.defenseCooldown /= 2;
                MeleeWeapon.attackSwordCooldown /= 2;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns)
        {
            var found = false;
            foreach (var item in insns)
            //IL_01d5
            {
                if (item.opcode == OpCodes.Ldarg_3 && found == false)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, (short)8);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, (short)10);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, ModEntry.Helper.Reflection.GetMethod(typeof(CombatManager), nameof(CombatManager.CustomOtherMethod)).MethodInfo);
                    found = true;
                }
                yield return item;
            }
        }
        public static void CustomOtherMethod(Monster monster, bool crit, Farmer who, int minDamage, int maxDamage) //boooooooo
        {
            if (CoreUtility.CurrentPlayerHasProfession(68, who) || CoreUtility.CurrentPlayerHasProfession(63, who))
            {
                if (monster is MetalHead or DwarvishSentry or RockCrab && who.professions.Contains(68))
                {
                    bool isNatural = crit;
                    if (monster is RockCrab crab) 
                    {
                        var field = ModEntry.Helper.Reflection.GetField<NetBool>(crab, "isStickBug").GetValue().Value;
                        if (field)
                            crit = true;
                    }
                    else
                        crit = true;
                    if (crit && !isNatural)
                        Game1.player?.currentLocation?.playSound("crit");
                }
                OnCrit = crit;

                if (who.health <= who.maxHealth / 4 && who.professions.Contains(63))
                {
                    minDamage *= 2;
                    maxDamage *= 2;
                }
            }
        }
    }
}
