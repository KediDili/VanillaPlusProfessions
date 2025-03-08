using System;
using System.Collections.Generic;
using System.Linq;
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
using StardewValley.Objects.Trinkets;
using StardewValley.Extensions;
using StardewValley.Network;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Emit;
using VanillaPlusProfessions.Utilities;
using StardewValley.Buffs;
using VanillaPlusProfessions.Craftables;
using Microsoft.Xna.Framework.Input;

namespace VanillaPlusProfessions.Talents.Patchers
{
    public static class CombatPatcher
    {
        readonly static string PatcherName = nameof(CombatPatcher);
        readonly static System.Type PatcherType = typeof(CombatPatcher);

        internal static void ApplyPatches()
        {
            CoreUtility.PatchMethod(
                PatcherName, "BaseEnchantment.GetAvailableEnchantments",
                original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.GetAvailableEnchantments)),
                postfix: new HarmonyMethod(PatcherType, nameof(GetAvailableEnchantments_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Slingshot.CanAutoFire",
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.CanAutoFire)),
                postfix: new HarmonyMethod(PatcherType, nameof(CanAutoFire_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Slingshot.PerformFire",
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.PerformFire)),
                prefix: new HarmonyMethod(PatcherType, nameof(PerformFire_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Slingshot.draw",
                original: AccessTools.Method(typeof(Slingshot), nameof(Slingshot.draw)),
                postfix: new HarmonyMethod(PatcherType, nameof(draw_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "BasicProjectile.behaviorOnCollisionWithOther",
                original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithOther)),
                postfix: new HarmonyMethod(PatcherType, nameof(behaviorOnCollisionWithOther_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "BasicProjectile.behaviorOnCollisionWithPlayer",
                original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithPlayer)),
                postfix: new HarmonyMethod(PatcherType, nameof(behaviorOnCollisionWithOther_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "BasicProjectile.behaviorOnCollisionWithTerrainFeature",
                original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithTerrainFeature)),
                postfix: new HarmonyMethod(PatcherType, nameof(behaviorOnCollisionWithOther_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "BasicProjectile.behaviorOnCollisionWithMonster",
                original: AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithMonster)),
                postfix: new HarmonyMethod(PatcherType, nameof(behaviorOnCollisionWithMonster_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Ring.CanCombine",
                original: AccessTools.Method(typeof(Ring), nameof(Ring.CanCombine)),
                postfix: new HarmonyMethod(PatcherType, nameof(CanCombine_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "GreenSlime.onDealContactDamage",
                original: AccessTools.Method(typeof(GreenSlime), nameof(GreenSlime.onDealContactDamage)),
                postfix: new HarmonyMethod(PatcherType, nameof(onDealContactDamage_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Farmer.gainExperience",
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                prefix: new HarmonyMethod(PatcherType, nameof(gainExperience_Prefix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Stats.takeStep",
                original: AccessTools.Method(typeof(Stats), nameof(Stats.takeStep)),
                prefix: new HarmonyMethod(PatcherType, nameof(takeStep_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Boots.onEquip",
                original: AccessTools.Method(typeof(Boots), nameof(Boots.onEquip)),
                prefix: new HarmonyMethod(PatcherType, nameof(onEquip_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Boots.onUnequip",
                original: AccessTools.Method(typeof(Boots), nameof(Boots.onUnequip)),
                prefix: new HarmonyMethod(PatcherType, nameof(onUnequip_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "GameLocation.damageMonster",
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(Farmer), typeof(bool) }),
                transpiler: new HarmonyMethod(PatcherType, nameof(damageMonster_Transpiler_2))
            );
            CoreUtility.PatchMethod(
                PatcherName, "RainbowHairTrinketEffect.Apply",
                original: AccessTools.Method(typeof(RainbowHairTrinketEffect), nameof(RainbowHairTrinketEffect.Apply)),
                postfix: new HarmonyMethod(PatcherType, nameof(OnApply_RainbowHairTrinketEffect_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "RainbowHairTrinketEffect.Unapply",
                original: AccessTools.Method(typeof(RainbowHairTrinketEffect), nameof(RainbowHairTrinketEffect.Unapply)),
                postfix: new HarmonyMethod(PatcherType, nameof(OnUnApply_RainbowHairTrinketEffect_Postfix))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Farmer.takeDamage",
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
                postfix: new HarmonyMethod(PatcherType, nameof(TakeDamage))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Object.placementAction",
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
                postfix: new HarmonyMethod(PatcherType, nameof(placementAction))
            );
            CoreUtility.PatchMethod(
                PatcherName, "Trinket.CanSpawnTrinket",
                original: AccessTools.Method(typeof(Trinket), nameof(Trinket.CanSpawnTrinket)),
                postfix: new HarmonyMethod(PatcherType, nameof(CanSpawnTrinket_Postfix))
            );
        }

        public static void CanSpawnTrinket_Postfix(Farmer f, ref bool __result)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("Accessorise", who: f) && ModEntry.ModConfig.Value.MasteryCaveChanges)
                {
                    __result = f.CombatLevel >= 10;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Trinket.CanSpawnTrinket", "postfixed", true);
            }
        }

        public static void placementAction(StardewValley.Object __instance, int x, int y, GameLocation location)
        {
            try
            {
                if (__instance.QualifiedItemId == "(BC)KediDili.VPPData.CP_ProgrammableDrill")
                {
                    MachineryEventHandler.DrillLocations.TryAdd(Game1.player.currentLocation.Name, new());
                    if (!MachineryEventHandler.DrillLocations[Game1.player.currentLocation.Name].Contains(new Vector2(x / 64, y / 64)))
                    {
                        MachineryEventHandler.DrillLocations[Game1.player.currentLocation.Name].Add(new Vector2(x / 64, y / 64));
                    }
                }
                else if (__instance.QualifiedItemId == "(BC)KediDili.VPPData.CP_ThermalReactor")
                {
                    MachineryEventHandler.ThermalReactorLocations.TryAdd(Game1.player.currentLocation.Name, new());
                    if (!MachineryEventHandler.ThermalReactorLocations[Game1.player.currentLocation.Name].Contains(new Vector2(x / 64, y / 64)))
                    {
                        MachineryEventHandler.ThermalReactorLocations[Game1.player.currentLocation.Name].Add(new Vector2(x / 64, y / 64));
                    }
                }
                else if (__instance.QualifiedItemId == "(BC)KediDili.VPPData.CP_NodeMaker")
                {
                    MachineryEventHandler.NodeMakerLocations.TryAdd(Game1.player.currentLocation.Name, new());
                    if (!MachineryEventHandler.NodeMakerLocations[Game1.player.currentLocation.Name].Contains(new Vector2(x / 64, y / 64)))
                    {
                        MachineryEventHandler.NodeMakerLocations[Game1.player.currentLocation.Name].Add(new Vector2(x / 64, y / 64));
                    }
                }
                else if (__instance.QualifiedItemId == "(BC)KediDili.VPPData.CP_MinecartChest" || __instance.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector")
                {
                    Vector2 placementTile = new(x / 64, y / 64);
                    var chest = new Chest(true, __instance.ItemId);
                    chest.modData.Add("Pathoschild.Automate/StoreItems", "Disabled");
                    chest.modData.Add("Pathoschild.Automate/TakeItems", "Disabled");
                    chest.modData.Add("Pathoschild.ChestsAnywhere/IsIgnored", "true");
                    if (!location.Objects.TryAdd(placementTile, chest))
                    {
                        location.Objects[placementTile] = chest;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Object.placementAction", "postfixed", true);
            }
        }

        public static void TakeDamage(Farmer __instance, Monster damager)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("Grit", who: __instance) && !__instance.isRidingHorse() && __instance.temporaryInvincibilityTimer == 0 && !(damager is BigSlime or GreenSlime && __instance.isWearingRing("520")))
                {
                    __instance.currentTemporaryInvincibilityDuration = (int)(__instance.currentTemporaryInvincibilityDuration * 1.2);
                }
                if (__instance.currentLocation.debris.LastOrDefault()?.debrisMessage.Value is not null and string str && int.TryParse(str, out int result))
                {
                    foreach (var trinketRing in TalentUtility.GetAllTrinketRings(__instance))
                    {
                        trinketRing.Trinket.OnReceiveDamage(__instance, result);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Farmer.takeDamage", "postfixed", true);
            }
        }

        public static void OnApply_RainbowHairTrinketEffect_Postfix(Farmer farmer)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", who: farmer))
                {
                    farmer.maxStamina.Value += 30;
                    farmer.maxHealth += 30;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "RainbowHairTrinketEffect.OnApply", "postfixed", true);
            }
        }
        public static void OnUnApply_RainbowHairTrinketEffect_Postfix(Farmer farmer)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("HiddenBenefits", who: farmer))
                {
                    farmer.maxStamina.Value -= 30;
                    farmer.maxHealth -= 30;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "RainbowHairTrinketEffect.OnUnApply", "postfixed", true);
            }
        }

        public static IEnumerable<CodeInstruction> damageMonster_Transpiler_2(IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> instructions = new();
            try
            {
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
                        instructions.Add(new(OpCodes.Ldarg_S, localBuilder));
                        instructions.Add(new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(TryOverrideVanillaCritChance))));
                        continue;
                    }
                    else if (item.opcode == OpCodes.Ldc_R4 && item.operand == (object)1 && localBuilder is not null)
                    {
                        instructions.Add(new(OpCodes.Ldarg_S, localBuilder));
                        instructions.Add(new(OpCodes.Call, AccessTools.Method(PatcherType, nameof(TryOverrideVanillaCritMultiplier))));
                        continue;
                    }
                    instructions.Add(item);
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "RainbowHairTrinketEffect.OnUnApply", "transpiled", true);
            }
            return instructions;
        }

        public static float TryOverrideVanillaCritChance(Farmer who)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("Bullseye", who: who))
                {
                    return 0.5f + who.buffs.CriticalChanceMultiplier;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CombatPatcher.TryOverrideVanillaCritChance", "<individual method>", true);
            }
            return 0f;

        }
        public static float TryOverrideVanillaCritMultiplier(Farmer who)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("Bullseye", who: who))
                {
                    return 0.5f + who.buffs.CriticalPowerMultiplier;
                }

            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "CombatPatcher.TryOverrideVanillaCritChance", "<individual method>", true);
            }
            return 0f;
        }

        public static void onEquip_Postfix(Farmer who, Boots __instance)
        {
            try
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Boots.onEquip", "postfixed", true);
            }
        }
        public static void onUnequip_Postfix(Farmer who, Boots __instance)
        {
            try
            {
                if (__instance.QualifiedItemId == "(B)515")
                {
                    Game1.stopMusicTrack(StardewValley.GameData.MusicContext.MusicPlayer);
                    who.buffs.Remove("VPP.HiddenBenefits.Mining.Farming");
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Boots.onUnequip", "postfixed", true);
            }

        }
        public static void gainExperience_Prefix(Farmer __instance, int which, ref int howMuch)
        {
            try
            {
                if (Game1.player.currentLocation.IsFarm && Game1.player.currentLocation is not SlimeHutch && howMuch is not 0 && which is 4 && TalentUtility.CurrentPlayerHasTalent("dsdsds", who: __instance))
                    howMuch *= 3;
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Farmer.gainExperience", "prefixed", true);
            }
        }

        public static bool PerformFire_Prefix(Slingshot __instance, GameLocation location, Farmer who)
        {
            try
            {
                if (__instance.attachments[0] is not null and Trinket trinket && trinket?.QualifiedItemId == "(TR)MagicQuiver")
                {

                    if (__instance.GetBackArmDistance(who) > 4 && !__instance.canPlaySound)
                    {

                        AccessTools.Method(typeof(Slingshot), "updateAimPos").Invoke(__instance, null);
                        int mouseX = __instance.aimPos.X;
                        int mouseY = __instance.aimPos.Y;
                        Vector2 v = Utility.getVelocityTowardPoint(__instance.GetShootOrigin(who), __instance.AdjustForHeight(new Vector2(mouseX, mouseY)), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.buffs.WeaponSpeedMultiplier));
                        float projectileRotation = (float)Math.Atan2(v.Y, v.X) + (float)Math.PI / 2f;
                        Vector2 shoot_origin = __instance.GetShootOrigin(who);

                        BasicProjectile p = new(Game1.random.Next((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MinDamage ?? 10, ((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MaxDamage ?? 10) + 1), 16, 0, 0, 0f, v.X, v.Y, shoot_origin - new Vector2(32f, 32f), null, null, null, explode: false, damagesMonsters: true, location, who)
                        {
                            IgnoreLocationCollision = true
                        };
                        p.ignoreObjectCollisions.Value = true;
                        p.acceleration.Value = v;
                        p.maxVelocity.Value = 24f;
                        p.projectileID.Value = 14;
                        p.startingRotation.Value = projectileRotation;
                        p.alpha.Value = 0.001f;
                        p.alphaChange.Value = 0.05f;
                        p.light.Value = true;
                        p.collisionSound.Value = "magic_arrow_hit";
                        location.projectiles.Add(p);
                        location.playSound("magic_arrow");
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.PerformFire", "prefixed", true);
            }

            return true;
        }
        public static void draw_Postfix(SpriteBatch b, Slingshot __instance)
        {
            try
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

                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, target1), new Rectangle(652, 204, 44, 44), Color.White, 0f, new Vector2(22f, 22f), 1f, SpriteEffects.None, 0.999999f);
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, target2), new Rectangle(652, 204, 44, 44), Color.White, 0f, new Vector2(22f, 22f), 1f, SpriteEffects.None, 0.999999f);
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.draw", "postfixed", true);
            }
        }

        public static void onDealContactDamage_Postfix(Farmer who, GreenSlime __instance)
        {
            try
            {
                if (__instance.Player == who && TalentUtility.CurrentPlayerHasTalent("Slippery", who: who))
                {
                    who.buffs.Remove("13");
                    BuffEffects dsdsd = new();
                    dsdsd.Speed.Value = 1;
                    who.buffs.Apply(new("VPP.Slippery.Speed", "talents", "Slippery Talent", 20000, ModEntry.Helper.GameContent.Load<Texture2D>(ContentEditor.ContentPaths["ItemSpritesheet"]), 28, dsdsd, false, Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Slippery.Name")), Game1.parseText(ModEntry.Helper.Translation.Get("Buff.Slippery.Desc"), Game1.smallFont, TalentUtility.BuffDescriptionLength(ModEntry.Helper.Translation.Get("Buff.Slippery.Name")))));
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "GreenSlime.onDealContactDamage", "postfixed", true);
            }
        }

        public static void CanCombine_Postfix(Ring __instance, Ring ring, ref bool __result)
        {
            try
            {
                if (!__result && TalentUtility.CurrentPlayerHasTalent("Consolidation"))
                {
                    if (__instance.QualifiedItemId == ring.QualifiedItemId)
                    {
                        __result = true;
                    }
                    if (__instance is TrinketRing || ring is TrinketRing)
                    {
                        __result = false;
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Ring.CanCombine", "postfixed", true);
            }
        }

        public static void behaviorOnCollisionWithOther_Postfix(BasicProjectile __instance, GameLocation location)
        {
            try
            {
                if (TalentUtility.CurrentPlayerHasTalent("DazzlingStrike") && __instance.itemId.Value is not null)
                {
                    var obj = ItemRegistry.Create<StardewValley.Object>(__instance.itemId.Value);
                    if (obj.HasContextTag("category_gem") && Game1.random.NextBool(0.8))
                    {
                        Game1.createObjectDebris(__instance.itemId.Value, (int)__instance.position.X / 64, (int)__instance.position.Y / 64, location);
                    }
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "BasicProjectile.behaviorOnCollisionWithOther", "postfixed", true);
            }
        }

        public static void behaviorOnCollisionWithMonster_Postfix(NPC n, BasicProjectile __instance, GameLocation location)
        {
            try
            {
                if (__instance.damagesMonsters.Value && __instance.itemId.Value is not null)
                {
                    if (n is Monster monster && monster is not GreenSlime or BigSlime && ItemContextTagManager.HasBaseTag(__instance.itemId.Value, "slime_item") && !monster.modData.ContainsKey(TalentCore.Key_SlowerSliming))
                    {
                        monster.speed -= 1;
                        monster.modData[TalentCore.Key_SlowerSliming] = "slimed";
                        monster.startGlowing(Color.Green, false, 0.5f);
                    }
                }
                behaviorOnCollisionWithOther_Postfix(__instance, location);
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "BasicProjectile.behaviorOnCollisionWithMonster", "postfixed", true);
            }
        }

        public static void takeStep_Postfix()
        {
            try
            {
                foreach (var trinketRing in TalentUtility.GetAllTrinketRings(Game1.player))
                {
                    trinketRing.Trinket.OnFootstep(Game1.player);
                }
            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Farmer.takeStep", "postfixed", true);
            }
        }

        public static void CanAutoFire_Postfix(Slingshot __instance, ref bool __result)
        {
            try
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
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "Slingshot.CanAutoFire", "postfixed", true);
            }
        }
        public static void GetAvailableEnchantments_Postfix(ref List<BaseEnchantment> __result)
        {
            try
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
                if (!containsSlingShots && TalentUtility.CurrentPlayerHasTalent("Enchanting"))
                {
                    __result.Add(new AutoFireEnchantment());
                    __result.Add(new RapidEnchantment());
                    __result.Add(new BatKillerEnchantment());
                    __result.Add(new ThriftyEnchantment());
                }
                if (!containsStarburst && TalentUtility.CurrentPlayerHasTalent("Starburst"))
                {
                    __result.Add(new MagicEnchantment());
                }

            }
            catch (Exception e)
            {
                CoreUtility.PrintError(e, PatcherName, "BaseEnchantment.GetAvailableEnchantments", "postfixed", true);
            }
        }
    }
}