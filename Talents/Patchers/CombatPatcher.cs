using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using HarmonyLib;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Tools;
using VanillaPlusProfessions.Enchantments;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Extensions;
using StardewValley.Network;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;
using StardewValley.Buffs;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public static class CombatPatcher
    {
        internal static void ApplyPatches()
        {
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.GetAvailableEnchantments)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.GetAvailableEnchantments_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(BaseEnchantment.GetAvailableEnchantments), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.CanAutoFire)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.CanAutoFire_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Slingshot.CanAutoFire), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.PerformFire)),
                    transpiler: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.Transpiler))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Slingshot.PerformFire), "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.draw)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.draw_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Slingshot.draw), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithMonster)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.behaviorOnCollisionWithMonster_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(BasicProjectile.behaviorOnCollisionWithMonster), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithOther)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.behaviorOnCollisionWithOther_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(BasicProjectile.behaviorOnCollisionWithOther), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithPlayer)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.behaviorOnCollisionWithOther_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(BasicProjectile.behaviorOnCollisionWithPlayer), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithTerrainFeature)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.behaviorOnCollisionWithOther_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(BasicProjectile.behaviorOnCollisionWithTerrainFeature), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Ring), nameof(Ring.CanCombine)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.CanCombine_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Ring.CanCombine), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GreenSlime), nameof(GreenSlime.onDealContactDamage)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.onDealContactDamage_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(GreenSlime.onDealContactDamage), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                    prefix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.gainExperience_Prefix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Farmer.gainExperience), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Stats), nameof(Stats.takeStep)),
                    prefix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.takeStep_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Stats.takeStep), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Boots), nameof(Boots.onEquip)),
                    prefix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.onEquip_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Boots.onEquip), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Boots), nameof(Boots.onUnequip)),
                    prefix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.onUnequip_Postfix))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Boots.onUnequip), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(Farmer), typeof(bool) }),
                    transpiler: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.damageMonster_Transpiler_2))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(GameLocation.damageMonster), "transpiling");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(RainbowHairTrinketEffect), nameof(RainbowHairTrinketEffect.Apply)),
                   postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.OnApply_RainbowHairTrinketEffect_Postfix))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(RainbowHairTrinketEffect.Apply), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                   original: AccessTools.Method(typeof(RainbowHairTrinketEffect), nameof(RainbowHairTrinketEffect.Unapply)),
                   postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.OnUnApply_RainbowHairTrinketEffect_Postfix))
               );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(RainbowHairTrinketEffect.Unapply), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.TakeDamage))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(Farmer.takeDamage), "postfixing");
            }
            try
            {
                ModEntry.Harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
                    postfix: new HarmonyMethod(typeof(CombatPatcher), nameof(CombatPatcher.placementAction))
                );
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, nameof(CombatPatcher), nameof(StardewValley.Object.placementAction), "postfixing");
            }
        }

        public static void placementAction(StardewValley.Object __instance, int x, int y, ref bool __result, GameLocation location)
        {
            if (__instance.QualifiedItemId == "(BC)Kedi.VPP.perch")
            {
                if (location is AnimalHouse)
                {
                    Vector2 placementTile = new Vector2(x / 64, y / 64);
                    if (!location.Objects.TryAdd(placementTile, new ParrotPerch(placementTile, "", false)))
                    {
                        location.Objects[placementTile] = new ParrotPerch(placementTile, "", false);
                        __result = true;
                    }
                }
                else
                {
                    Game1.addHUDMessage(new("This can be placed only to animal buildings!", HUDMessage.error_type));
                    __result = false;
                }
            }
            else if (__instance.QualifiedItemId == "(BC)217" && __instance is Chest chest)
            {
                if (TalentUtility.AnyPlayerHasTalent("Misc_MiniFridgeBigSpace"))
                {
                    chest.SpecialChestType = Chest.SpecialChestTypes.BigChest;
                }
            }
        }

        public static void TakeDamage(Farmer __instance, Monster damager)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Combat_Grit", who:__instance) && __instance.temporaryInvincibilityTimer == 0 && !(damager is BigSlime or GreenSlime && __instance.isWearingRing("520")))
            {
                __instance.currentTemporaryInvincibilityDuration = (int)(__instance.currentTemporaryInvincibilityDuration * 1.2);
            }
            if (__instance.currentLocation.debris.LastOrDefault()?.debrisMessage.Value is not null and string str)
            {
                if (int.TryParse(str, out int result))
                {
                    foreach (var trinketRing in TalentUtility.GetAllTrinketRings(__instance))
                    {
                        trinketRing.Trinket.OnReceiveDamage(__instance, result);
                    }
                }
            }
        }

        public static void OnApply_RainbowHairTrinketEffect_Postfix(Farmer farmer)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Combat_HiddenBenefits", who: farmer))
            {
                farmer.maxStamina.Value += 30;
                farmer.maxHealth += 30;
            }
        }
        public static void OnUnApply_RainbowHairTrinketEffect_Postfix(Farmer farmer)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Combat_HiddenBenefits", who: farmer))
            {
                farmer.maxStamina.Value -= 30;
                farmer.maxHealth -= 30;
            }
        }

        public static IEnumerable<CodeInstruction> damageMonster_Transpiler_2(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> instructions = insns.ToList();

            LocalBuilder localBuilder = null;

            foreach (var item in insns)
            {
                if (item.opcode == OpCodes.Ldarg_S && item.operand is LocalBuilder builder && builder.LocalIndex is 4)
                {
                    localBuilder = builder;
                    break;
                }
            }

            foreach (var item in insns)
            {
                if (item.opcode == OpCodes.Ldc_R4 && item.operand == (object)0 && localBuilder is not null)
                {
                    yield return new(OpCodes.Ldarg_S, localBuilder);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(CombatPatcher), nameof(TryOverrideVanillaCritChance)));
                    continue;
                }
                else if (item.opcode == OpCodes.Ldc_R4 && item.operand == (object)1 && localBuilder is not null)
                {
                    yield return new(OpCodes.Ldarg_S, localBuilder);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(CombatPatcher), nameof(TryOverrideVanillaCritMultiplier)));
                    continue;
                }
                yield return item;
            }
        }

        public static float TryOverrideVanillaCritChance(Farmer who)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Bullseye", who: who))
            {
                return 0.5f + who.buffs.CriticalChanceMultiplier;
            }
            return 0f;
        }
        public static float TryOverrideVanillaCritMultiplier(Farmer who)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Bullseye", who: who))
            {
                return 0.5f + who.buffs.CriticalPowerMultiplier;
            }
            return 0f;
        }

        public static void onEquip_Postfix(Farmer who, Boots __instance)
        {
            if (__instance.QualifiedItemId == "(B)515")
            {
                Game1.changeMusicTrack("Cowboy_OVERWORLD", false, StardewValley.GameData.MusicContext.MusicPlayer); //Prairie king music track

                BuffEffects buffEffects = new();
                buffEffects.MiningLevel.Value += 2;
                buffEffects.FarmingLevel.Value += 2;
                Buff buff = new("VPP.HiddenBenefits.Mining.Farming", "Cowboy Boots", "Cowboy Boots", -2, Game1.buffsIcons, 10, buffEffects, false, "The Prairie King");
                who.buffs.Apply(buff);
            }
        }
        public static void onUnequip_Postfix(Farmer who, Boots __instance)
        {
            if (__instance.QualifiedItemId == "(B)515")
            {
                Game1.stopMusicTrack(StardewValley.GameData.MusicContext.MusicPlayer);
                who.buffs.Remove("VPP.HiddenBenefits.Mining.Farming");
            }
        }
        public static void gainExperience_Prefix(Farmer __instance, int which, ref int howMuch)
        {
            if (Game1.player.currentLocation.IsFarm && Game1.player.currentLocation is not SlimeHutch && howMuch is not 0 && which is 4 && TalentUtility.CurrentPlayerHasTalent("dsdsds", who: __instance))
                howMuch *= 3;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var number = 3;
            var indesx = 0;
            List<CodeInstruction> insts = codeInstructions.ToList();
            foreach (var item in codeInstructions)
            {
                indesx++;
                if (number == 0 && item.opcode.Equals(OpCodes.Ldc_I4_1))
                {
                    yield return new(OpCodes.Ldarg_1);
                    yield return new(OpCodes.Ldarg_0);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(SlingshotEnchantment), nameof(SlingshotEnchantment.OnShoot)));
                }
                else
                {
                    number--;
                }
                yield return item;
            }
        }
        public static void draw_Postfix(SpriteBatch b, Slingshot __instance)
        {
            if (__instance.lastUser.usingSlingshot && __instance.lastUser.IsLocalPlayer && TalentCore.IsActionButtonUsed.Value)
            {
                int mouseX = __instance.aimPos.X;
                int mouseY = __instance.aimPos.Y;
                Vector2 shoot_origin = __instance.GetShootOrigin(__instance.lastUser);
                Vector2 v = Utility.getVelocityTowardPoint(shoot_origin, __instance.AdjustForHeight(new Vector2(mouseX, mouseY)), 256f);

                double distanceBetweenRadiusAndSquare = Math.Sqrt(v.X * v.X + v.Y * v.Y) - 181.0;
                double xPercent = v.X / 256f;
                double yPercent = v.Y / 256f;
                int x = (int)(v.X - distanceBetweenRadiusAndSquare * xPercent);
                int y = (int)(v.Y - distanceBetweenRadiusAndSquare * yPercent);
                if (!Game1.options.useLegacySlingshotFiring)
                {
                    x *= -1;
                    y *= -1;
                }
                Vector2 target = new(shoot_origin.X - x, shoot_origin.Y - y);
                Vector2 vecPlayerToTarget = Game1.GlobalToLocal(Game1.viewport, target) - Game1.GlobalToLocal(Game1.viewport, Game1.player.Position);

                Vector2 normalizethis = new(vecPlayerToTarget.Y, -vecPlayerToTarget.X);
                normalizethis.Normalize();

                Vector2 target1 = target + (normalizethis * 96);
                Vector2 target2 = target - (normalizethis * 96);

                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, target1), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, target2), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
            }
        }

        public static void onDealContactDamage_Postfix(Farmer who, GreenSlime __instance)
        {
            if (__instance.Player == who && TalentUtility.CurrentPlayerHasTalent("Slippery", who: who))
            {
                who.buffs.Remove("13");
                BuffEffects dsdsd = new();
                dsdsd.Speed.Value = 1;
                who.buffs.Apply(new("VPP.Slippery.Speed", "talents", "Slippery Talent", 20000, ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]), 28, dsdsd, false, Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Slippery.Name")), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Slippery.Desc"))));
            }
        }

        public static void CanCombine_Postfix(Ring __instance, Ring ring, ref bool __result)
        {
            if (!__result)
            {
                if (__instance.QualifiedItemId == ring.QualifiedItemId)
                {
                    __result = true;
                }
                if (__instance.HasContextTag("trinket_ring_accessorise") && ring.HasContextTag("trinket_ring_accessorise"))
                {
                    __result = false;
                }
            }
        }

        public static void behaviorOnCollisionWithOther_Postfix(NetString ___itemId, NetPosition ___position, GameLocation location)
        {
            if (TalentUtility.CurrentPlayerHasTalent("Combat_DazzlingStrike"))
            {
                var obj = ItemRegistry.Create<StardewValley.Object>(___itemId.Value);
                if (obj.HasContextTag("category_gem") && Game1.random.NextBool(0.8))
                {
                    Game1.createObjectDebris(___itemId.Value, (int)___position.X / 64, (int)___position.Y / 64, location);
                }
            }
        }

        public static void behaviorOnCollisionWithMonster_Postfix(NPC n, NetBool ___damagesMonsters, NetString ___itemId, NetPosition ___position, GameLocation location)
        {
            if (___damagesMonsters.Value)
            {
                if (n is Monster monster && monster is not GreenSlime or BigSlime && ___itemId.Value == "766" && !monster.modData.ContainsKey(TalentCore.Key_SlowerSliming))
                {
                    monster.speed -= 1;
                    monster.modData[TalentCore.Key_SlowerSliming] = "slimed";
                    monster.startGlowing(Color.Green, false, 0.5f);
                }
            }
            behaviorOnCollisionWithOther_Postfix(___itemId, ___position, location);
        }

        public static void takeStep_Postfix()
        {
            foreach (var trinketRing in TalentUtility.GetAllTrinketRings(Game1.player))
            {
                trinketRing.Trinket.OnFootstep(Game1.player);
            }
        }

        public static void CanAutoFire_Postfix(Slingshot __instance, ref bool __result)
        {
            if (__instance.enchantments.Any() && __instance.getLastFarmerToUse().CurrentTool == __instance)
            {
                if (__instance.enchantments[0] is AutoFireEnchantment && __instance.attachments[0] is not null)
                {
                    __result = true;
                    return;
                }
            }
            __result = false;
        }
        public static void GetAvailableEnchantments_Postfix(ref List<BaseEnchantment> __result)
        {
            bool containsSlingShots = false;
            bool containsStarburst = false;
            foreach (var item in __result)
            {
                if (item is SlingshotEnchantment)
                    containsSlingShots = true;
                else if (item is MagicEnchantment)
                    containsStarburst = true;
            }
            if (!containsSlingShots)
            {
                __result.Add(new AutoFireEnchantment());
                __result.Add(new RapidEnchantment());
                __result.Add(new BatKillerEnchantment());
                __result.Add(new ThriftyEnchantment());
            }
            if (!containsStarburst)
            {
                __result.Add(new MagicEnchantment());
            }
        }
    }
}
