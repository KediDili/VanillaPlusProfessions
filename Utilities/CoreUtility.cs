using StardewValley;
using StardewModdingAPI;
using System.Linq;
using System.Collections.Generic;
using VanillaPlusProfessions.Talents;
using StardewValley.Projectiles;
using System;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using VanillaPlusProfessions.Enchantments;
using System.Text;

namespace VanillaPlusProfessions.Utilities
{
    public static class CoreUtility
    {
        public static bool IsOverlayValid()
        {
            if (Game1.player.FarmingLevel > 10 || Game1.player.FishingLevel > 10 || Game1.player.ForagingLevel > 10 || Game1.player.MiningLevel > 10 || Game1.player.CombatLevel > 10)
            {
                return true;
            }
            else
            {
                foreach (var item in ModEntry.SpaceCoreAPI.Value.GetCustomSkills())
                {
                    if (ModEntry.SpaceCoreAPI.Value.GetLevelForCustomSkill(Game1.player, item) > 10)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void PrintError(Exception e, string @class, string method, string typeOfPatch)
        {
            ModEntry.ModMonitor.Log($"There has been an error while {typeOfPatch} {method} in {@class}, details below:", LogLevel.Error);
            ModEntry.ModMonitor.Log(e.ToString(), LogLevel.Error);
        }
        public static bool AnyPlayerHasProfession(int profession)
        {
            if (!Context.IsWorldReady)
                return ModEntry.ModConfig.Value.DeveloperOrTestingMode;

            var team = Game1.getOnlineFarmers();
            foreach (var farmer in team)
                if (farmer.isActive() && farmer.professions.Contains(profession))
                    return true;
            return ModEntry.ModConfig.Value.DeveloperOrTestingMode;
        }
        public static void PerformFire(GameLocation location, Farmer who, Slingshot slingshot)
        {
            if (slingshot.attachments[0] != null)
            {
                slingshot.canPlaySound = false;
                ModEntry.Helper.Reflection.GetMethod(slingshot, "updateAimPos").Invoke(null);
                int mouseX = slingshot.aimPos.X;
                int mouseY = slingshot.aimPos.Y;
                int backArmDistance = slingshot.GetBackArmDistance(who);

                Vector2 shoot_origin = slingshot.GetShootOrigin(who);
                Vector2 v = Utility.getVelocityTowardPoint(slingshot.GetShootOrigin(who), slingshot.AdjustForHeight(new Vector2(mouseX, mouseY)), (15 + Game1.random.Next(4, 6)) * (1f + who.buffs.WeaponSpeedMultiplier));

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
                Vector2 vecPlayerToTarget = target - Game1.player.Position;

                Vector2 normalizethis = new(vecPlayerToTarget.Y, -vecPlayerToTarget.X);
                normalizethis.Normalize();

                Vector2 target1 = target + (normalizethis * 24);
                Vector2 target2 = target - (normalizethis * 24);

                Vector2 thedirection = target1 - shoot_origin;
                Vector2 thedirection2 = target2 - shoot_origin;

                thedirection.Normalize();
                thedirection2.Normalize();

                if (backArmDistance > 4 && !slingshot.canPlaySound)
                {
                    StardewValley.Object ammunition = (StardewValley.Object)slingshot.attachments[0].getOne();
                    slingshot.attachments[0].Stack--;
                    slingshot.attachments[0].Stack--;
                    if (slingshot.attachments[0].Stack <= 0)
                    {
                        slingshot.attachments[0] = null;
                    }
                    int damage = 1;
                    float damageMod = (slingshot.ItemId == "33") ? 2f : ((!(slingshot.ItemId == "34")) ? 1f : 4f);
                    if (!Game1.options.useLegacySlingshotFiring)
                    {
                        v.X *= -1f;
                        v.Y *= -1f;
                    }

                    location.projectiles.Add(new BasicProjectile((int)(damageMod * (damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(Math.PI / (double)(64f + Game1.random.Next(-63, 64))), thedirection2.X * 16, thedirection2.Y * 16, shoot_origin - new Vector2(32f, 32f), "hammer", null, null, explode: false, damagesMonsters: true, location, who, null, ammunition.ItemId)
                    { IgnoreLocationCollision = false });

                    foreach (var item in slingshot.enchantments)
                        if (item is SlingshotEnchantment slingshotEnchantment)
                            slingshotEnchantment.OnShoot(location, slingshot);

                    location.projectiles.Add(new BasicProjectile((int)(damageMod * (damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(Math.PI / (double)(64f + Game1.random.Next(-63, 64))), thedirection.X * 16, thedirection.Y * 16, shoot_origin - new Vector2(32f, 32f), "hammer", null, null, explode: false, damagesMonsters: true, location, who, null, ammunition.ItemId)
                    { IgnoreLocationCollision = false });

                    foreach (var item in slingshot.enchantments)
                        if (item is SlingshotEnchantment slingshotEnchantment)
                            slingshotEnchantment.OnShoot(location, slingshot);
                }
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
            }
            slingshot.canPlaySound = true;
        }
        internal static void remove(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                Utility.ForEachBuilding(Building =>
                {
                    Building.modData.Remove(ModEntry.Key_FishRewardOrQuestDayLeft);
                    Building.modData.Remove(ModEntry.Key_IsSlimeHutchWatered);
                    return true;
                });
                Utility.ForEachLocation(Loc =>
                {
                    Loc.modData.Remove(TalentCore.Key_WasRainingHere);
                    Loc.modData.Remove(TalentCore.Key_FaeBlessings);
                    return true;
                });
                TalentCore.TalentPointCount.ResetAllScreens();
                foreach (var farmer in Game1.getAllFarmers())
                {
                    foreach (var item in ModEntry.Professions.Values)
                        farmer.professions.Remove(item.ID);
                }
            }
            else
            {
                ModEntry.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static void details(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("    - Skill Levels -    ");
                stringBuilder.AppendLine("Farming: " + Game1.player.farmingLevel.Value);
                stringBuilder.AppendLine("Fishing: " + Game1.player.fishingLevel.Value);
                stringBuilder.AppendLine("Mining: " + Game1.player.miningLevel.Value);
                stringBuilder.AppendLine("Combat: " + Game1.player.combatLevel.Value);
                stringBuilder.AppendLine("Foraging: " + Game1.player.foragingLevel.Value);
                foreach (var item in ModEntry.SpaceCoreAPI.Value.GetCustomSkills())
                {
                    stringBuilder.AppendLine(item + ": " + ModEntry.SpaceCoreAPI.Value.GetLevelForCustomSkill(Game1.player, item));
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Skill Experience -    ");
                stringBuilder.AppendLine("Farming: " + Game1.player.experiencePoints[0]);
                stringBuilder.AppendLine("Fishing: " + Game1.player.experiencePoints[1]);
                stringBuilder.AppendLine("Mining: " + Game1.player.experiencePoints[3]);
                stringBuilder.AppendLine("Combat: " + Game1.player.experiencePoints[4]);
                stringBuilder.AppendLine("Foraging: " + Game1.player.experiencePoints[2]);
                foreach (var item in ModEntry.SpaceCoreAPI.Value.GetCustomSkills())
                {
                    stringBuilder.AppendLine(item + ": " + ModEntry.SpaceCoreAPI.Value.GetExperienceForCustomSkill(Game1.player, item));
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Config Options -    ");
                stringBuilder.AppendLine($"Color Blindness Changes: {ModEntry.ModConfig.Value.ColorBlindnessChanges}");
                stringBuilder.AppendLine($"Developer Or Testing Mode: {ModEntry.ModConfig.Value.DeveloperOrTestingMode}");
                stringBuilder.AppendLine($"Mastery Cave Changes: {ModEntry.ModConfig.Value.MasteryCaveChanges}");
                stringBuilder.AppendLine($"Talent Hint Level: {ModEntry.ModConfig.Value.TalentHintLevel}");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Talents & Professions -    ");
                stringBuilder.AppendLine($"Talent Points: {TalentCore.TalentPointCount.Value}");
                stringBuilder.AppendLine($"Save Changes Applied: {Game1.player.mailReceived.Contains(TalentCore.Key_PointsCalculated)}");
                foreach (var item in TalentCore.Talents)
                {
                    if (Game1.player.mailReceived.Contains(item.Value.MailFlag))
                    {
                        if (item.Value.Branches is not null && item.Value.Branches.Length > 0)
                        {
                            for (int i = 0; i < item.Value.Branches.Length; i++)
                                if (Game1.player.mailReceived.Contains(item.Value.Branches[i].Flag))
                                    stringBuilder.AppendLine($"{item.Value.Name}: {item.Value.Branches[i].Flag}");
                        }
                        else
                            stringBuilder.AppendLine($"{item.Value.Name}: {item.Value.MailFlag}");
                    }
                }
                foreach (var item in ModEntry.GetProfessions())
                {
                    stringBuilder.AppendLine(item);
                }
                ModEntry.ModMonitor.Log(stringBuilder.ToString(), LogLevel.Debug);
            }
            else
            {
                ModEntry.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static void recalculate(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                if (!Game1.player.modData.TryGetValue(TalentCore.Key_PointsCalculated, out string _))
                {
                    Game1.player.farmingLevel.Value = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
                    Game1.player.fishingLevel.Value = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
                    Game1.player.miningLevel.Value = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
                    Game1.player.combatLevel.Value = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
                    Game1.player.foragingLevel.Value = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);

                    TalentCore.TalentPointCount.Value = 0;

                    int number = 0;

                    number += Game1.player.farmingLevel.Value;
                    number += Game1.player.fishingLevel.Value;
                    number += Game1.player.miningLevel.Value;
                    number += Game1.player.combatLevel.Value;
                    number += Game1.player.foragingLevel.Value;

                    foreach (var item in ModEntry.SpaceCoreAPI.Value.GetCustomSkills())
                    {
                        number += ModEntry.SpaceCoreAPI.Value.GetLevelForCustomSkill(Game1.player, item);
                    }

                    number += Game1.player.achievements.Count;
                    foreach (var item in Game1.player.team.completedSpecialOrders)
                        if (item.StartsWith("QiChallenge"))
                            number++;

                    if (Game1.player.mailReceived.Contains("Farm_Eternal"))
                        number += 10;

                    TalentCore.TalentPointCount.Value = number;
                    Game1.player.mailReceived.Add(TalentCore.Key_PointsCalculated);
                }
                else
                {
                    ModEntry.ModMonitor.Log("Nice try, " + Game1.player.displayName, LogLevel.Warn);
                }
            }
            else
            {
                ModEntry.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static int GetMaxLevel()
        {
            return ModEntry.ModConfig.Value.MasteryCaveChanges ? 20 : 10;
        }

        public static bool DoesDictHaveID(string value, out KeyValuePair<string, Profession> result)
        {
            var list = from profession in ModEntry.Professions
                       where profession.Value.ID.ToString() == value
                       select profession;
            result = list.FirstOrDefault();

            return list.Any();
        }

        public static bool CurrentPlayerHasProfession(int profession, long farmerID = -1, Farmer useThisInstead = null, bool ignoreMode = false)
        {
            if (farmerID is not -1)
            {
                useThisInstead = Game1.getFarmer(farmerID);
            }
            useThisInstead ??= Game1.player;

            if (useThisInstead is null)
                return false;

            return useThisInstead.professions.Contains(profession) is true || (ModEntry.ModConfig.Value.DeveloperOrTestingMode && !ignoreMode);
        }        
    }
}
