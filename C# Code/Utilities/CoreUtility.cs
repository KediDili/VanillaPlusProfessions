using StardewValley;
using StardewModdingAPI;
using VanillaPlusProfessions.Talents;
using StardewValley.Projectiles;
using System;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using VanillaPlusProfessions.Enchantments;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.GameData.Objects;
using StardewValley.TerrainFeatures;
using System.Reflection;
using HarmonyLib;
using System.IO;

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

        public static void PrintError(Exception e, string @class, string method, string typeOfPatch, bool isRunning = false)
        {
            if (isRunning)
            {
                ModEntry.ModMonitor.Log($"There has been an error while running {method} in {@class} which has been {typeOfPatch}, details below:", LogLevel.Error);
            }
            else
            {
                ModEntry.ModMonitor.Log($"There has been an error while {typeOfPatch} {method} in {@class}, details below:", LogLevel.Error);
            }
            ModEntry.ModMonitor.Log(e.ToString(), LogLevel.Error);
        }
        public static void PatchMethod(string patcherName, string methodName, MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
        {
            bool success = true;
            try
            {
                ModEntry.Harmony.Patch(original, prefix, postfix, transpiler);
            }
            catch (Exception e)
            {
                string patchType = "";
                if (prefix is not null)
                {
                    patchType += "prefixing";
                }
                if (postfix is not null)
                {
                    patchType += (patchType.Length > 0 ? " ," : "") + "postfixing";
                }
                if (transpiler is not null)
                {
                    patchType += (patchType.Length > 0 ? " ," : "") + "transpiling";
                }
                success = false;
                PrintError(e, patcherName, $"{original.DeclaringType.Name}.{original.Name}", patchType, false);
            }
            finally
            {
                if (success)
                {
                    if (ModEntry.ModConfig.Value.DeveloperOrTestingMode)
                    {
                        ModEntry.ModMonitor.Log($"{patcherName} successfully patched {methodName}.");
                    }
                }
                else
                {
                    ModEntry.ModMonitor.Log($"This is an error thrown by VPP. Some features may not work, but this shouldn't break your game. Reproduce this with only VPP before you make a bug report and make sure it hasn't been reported before.", LogLevel.Warn);
                }
            }
        }

        public static bool AnyPlayerHasProfession(string prof)
        {
            if (!Context.IsWorldReady)
                return ModEntry.ModConfig.Value.DeveloperOrTestingMode;
            int profession = 0;
            foreach (var item in ModEntry.Professions)
            {
                if (prof == item.Key)
                {
                    profession = item.Value.ID;
                    break;
                }
            }
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
                    if (ammunition.QualifiedItemId == "(TR)MagicQuiver")
                    {
                        
                    }
                    else
                    {
                        if (ammunition.ConsumeStack(2) is null)
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
                    }

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
                if (args.Length > 0 && args[1].ToLower() == "true")
                {
                    Utility.ForEachBuilding(Building =>
                    {
                        Building.modData.Remove(ModEntry.Key_FishRewardOrQuestDayLeft);
                        Building.modData.Remove(TalentCore.Key_HiddenBenefit_FrogEggs);
                        Building.modData.Remove(ModEntry.Key_IsSlimeHutchWatered);
                        if (Building.GetIndoors() is AnimalHouse house)
                        {
                            foreach (var (id, animal) in house.Animals.Pairs)
                            {
                                if (!house.animalsThatLiveHere.Contains(id))
                                    continue;

                                animal.modData.Remove(TalentCore.Key_WildGrowth);
                            }
                        }
                        else if (Building.GetIndoors() is SlimeHutch hutch)
                        {
                            foreach (var slime in hutch.characters)
                            {
                                slime.modData.Remove(ModEntry.Key_SlimeWateredDaysSince);
                            }
                        }
                        return true;
                    });
                    Utility.ForEachLocation(Loc =>
                    {
                        Loc.modData.Remove(TalentCore.Key_WasRainingHere);
                        Loc.modData.Remove(TalentCore.Key_FaeBlessings);
                        foreach (var item in Loc.terrainFeatures.Pairs)
                        {
                            if (item.Value is FruitTree tree)
                            {
                                tree.modData.Remove(ModEntry.Key_TFHasTapper);
                                tree.modData.Remove(ModEntry.Key_TFTapperID);
                                tree.modData.Remove(ModEntry.Key_TFTapperDaysLeft);
                            }
                            else if (item.Value is GiantCrop crop)
                            {
                                crop.modData.Remove(ModEntry.Key_TFHasTapper);
                                crop.modData.Remove(ModEntry.Key_TFTapperID);
                                crop.modData.Remove(ModEntry.Key_TFTapperDaysLeft);
                            }
                            else if (item.Value is HoeDirt dirt)
                            {
                                dirt.modData.Remove(TalentCore.Key_FaeBlessings);
                            }
                        }
                        return true;
                    });
                    Utility.ForEachCrop(crop =>
                    {
                        crop.modData.Remove(TalentCore.Key_HiddenBenefit_Crop);
                        return true;
                    });
                    Utility.ForEachItem(item =>
                    {
                        item.modData.Remove(TalentCore.Key_XrayDrop);
                        item.modData.Remove(TalentCore.Key_HiddenBenefit_FairyBox);
                        item.modData.Remove(TalentCore.Key_Resurgence);
                        return true;
                    });
                }                
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (args.Length > 0 && args[1].ToLower() == "true")
                    {
                        farmer.modData.Remove(ModEntry.Key_ForageGuessItemID);
                        farmer.modData.Remove(ModEntry.Key_DaysLeftForForageGuess);
                        farmer.modData.Remove(ModEntry.Key_HasFoundForage);
                        farmer.modData.Remove(TalentCore.Key_TalentPoints);
                        farmer.modData.Remove(TalentCore.Key_DisabledTalents);
                    }

                    foreach (var item in ModEntry.Professions.Values)
                        farmer.professions.Remove(item.ID);

                    foreach (var item in TalentCore.Talents.Values)
                    {
                        if (item.Branches is not null && item.Branches.Length > 0)
                        {
                            foreach (var branch in item.Branches)
                            {
                                farmer.mailReceived.Remove(branch.Flag);
                            }
                        }
                        farmer.mailReceived.Remove(item.MailFlag);
                        farmer.mailReceived.Remove(item.MailFlag + "_disabled");
                    }

                    farmer.mailReceived.Remove(TalentCore.Key_PointsCalculated);
                    if (farmer.farmingLevel.Value > 10)
                    {
                        farmer.farmingLevel.Value = 10;
                    }
                    if (farmer.foragingLevel.Value > 10)
                    {
                        farmer.foragingLevel.Value = 10;
                    }
                    if (farmer.miningLevel.Value > 10)
                    {
                        farmer.miningLevel.Value = 10;
                    }
                    if (farmer.fishingLevel.Value > 10)
                    {
                        farmer.fishingLevel.Value = 10;
                    }
                    if (farmer.combatLevel.Value > 10)
                    {
                        farmer.combatLevel.Value = 10;
                    }
                }
                TalentCore.DisabledTalents.Clear();
                ModEntry.IsUninstalling.Value = true;
                TalentCore.TalentPointCount.ResetAllScreens();
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
                stringBuilder.AppendLine("");
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
                stringBuilder.AppendLine("Foraging: " + Game1.player.experiencePoints[2]);
                stringBuilder.AppendLine("Mining: " + Game1.player.experiencePoints[3]);
                stringBuilder.AppendLine("Combat: " + Game1.player.experiencePoints[4]);
                foreach (var item in ModEntry.SpaceCoreAPI.Value.GetCustomSkills())
                {
                    stringBuilder.AppendLine(item + ": " + ModEntry.SpaceCoreAPI.Value.GetExperienceForCustomSkill(Game1.player, item));
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Config Options -    ");
                stringBuilder.AppendLine($"Color Blindness Changes: {ModEntry.ModConfig.Value.ColorBlindnessChanges}");
                stringBuilder.AppendLine($"Developer Or Testing Mode: {ModEntry.ModConfig.Value.DeveloperOrTestingMode}");
                stringBuilder.AppendLine($"Mastery Cave Changes: {ModEntry.ModConfig.Value.MasteryCaveChanges}");
                stringBuilder.AppendLine($"Stamina Cost Adjustments: {ModEntry.ModConfig.Value.StaminaCostAdjustments}");
                stringBuilder.AppendLine($"Professions Only: {ModEntry.ModConfig.Value.ProfessionsOnly}");
                stringBuilder.AppendLine($"Talent Hint Level: {ModEntry.ModConfig.Value.TalentHintLevel}");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Talents & Professions -    ");
                stringBuilder.AppendLine($"Talent Points: {TalentCore.TalentPointCount.Value}");
                stringBuilder.AppendLine($"Unlocked Achievement Count: {Game1.player.achievements.Count}");
                stringBuilder.AppendLine($"Save Changes Applied: {Game1.player.mailReceived.Contains(TalentCore.Key_PointsCalculated)}");
                stringBuilder.Append($"Talents Bought:");
                foreach (var item in TalentCore.Talents)
                {
                    if (Game1.player.mailReceived.Contains(item.Value.MailFlag))
                    {
                        bool disabled = Game1.player.mailReceived.Contains(item.Value.MailFlag + "_disabled");
                        if (item.Value.Branches is not null && item.Value.Branches.Length > 0)
                        {
                            for (int i = 0; i < item.Value.Branches.Length; i++)
                                if (Game1.player.mailReceived.Contains(item.Value.Branches[i].Flag))
                                    stringBuilder.Append($" {item.Value.Name}: {item.Value.Branches[i].Name}" + (disabled ? " (Disabled)" : "") + ",");
                        }
                        else
                            stringBuilder.Append($" {item.Value.Name}" + (disabled ? " (Disabled)" : "") + ",");
                    }
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine($"VPP Professions Chosen:");
                foreach (var item in ModEntry.GetProfessions())
                {
                    stringBuilder.Append($" {item},");
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
                if (!Game1.player.mailReceived.Contains(TalentCore.Key_PointsCalculated))
                {
                    int number = 0, newLevels;

                    //Farming
                    if (Game1.player.experiencePoints[0] > ModEntry.levelExperiences[0])
                    {
                        newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
                        for (int i = Game1.player.farmingLevel.Value + 1; i <= newLevels; i++)
                        {
                            Game1.player.newLevels.Add(new(0, i));
                        }
                        Game1.player.farmingLevel.Value = newLevels;
                    }

                    //Fishing
                    if (Game1.player.experiencePoints[1] > ModEntry.levelExperiences[0])
                    {
                        newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
                        for (int i = Game1.player.fishingLevel.Value + 1; i <= newLevels; i++)
                        {
                            Game1.player.newLevels.Add(new(1, i));
                        }
                        Game1.player.fishingLevel.Value = newLevels;
                    }

                    //Foraging
                    if (Game1.player.experiencePoints[2] > ModEntry.levelExperiences[0])
                    {
                        newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
                        for (int i = Game1.player.foragingLevel.Value + 1; i <= newLevels; i++)
                        {
                            Game1.player.newLevels.Add(new(2, i));
                        }
                        Game1.player.foragingLevel.Value = newLevels;
                    }

                    //Mining
                    if (Game1.player.experiencePoints[3] > ModEntry.levelExperiences[0])
                    {
                        newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
                        for (int i = Game1.player.miningLevel.Value + 1; i <= newLevels; i++)
                        {
                            Game1.player.newLevels.Add(new(3, i));
                        }
                        Game1.player.miningLevel.Value = newLevels;
                    }

                    //Combat
                    if (Game1.player.experiencePoints[4] > ModEntry.levelExperiences[0])
                    {
                        newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
                        for (int i = Game1.player.combatLevel.Value + 1; i <= newLevels; i++)
                        {
                            Game1.player.newLevels.Add(new(4, i));
                        }
                        Game1.player.combatLevel.Value = newLevels;
                    }

                    number += Game1.player.farmingLevel.Value;
                    number += Game1.player.fishingLevel.Value;
                    number += Game1.player.foragingLevel.Value;
                    number += Game1.player.miningLevel.Value;
                    number += Game1.player.combatLevel.Value;

                    number += Game1.player.achievements.Count;
                    foreach (var item in Game1.player.team.completedSpecialOrders)
                        if (item.StartsWith("QiChallenge"))
                            number++;

                    if (Game1.player.mailReceived.Contains("Farm_Eternal"))
                        number += 10;

                    ModEntry.IsUninstalling.Value = false;
                    TalentCore.AddTalentPoint(number - TalentCore.TalentPointCount.Value, false);
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

        public static bool CurrentPlayerHasProfession(string prof, long farmerID = -1, Farmer useThisInstead = null, bool ignoreMode = false)
        {
            if (farmerID is not -1)
            {
                useThisInstead = Game1.GetPlayer(farmerID) ?? Game1.MasterPlayer;
            }
            useThisInstead ??= Game1.player;

            if (useThisInstead is null)
                return false;
            int profession = -1;
            foreach (var item in ModEntry.Professions)
            {
                if (prof == item.Key)
                {
                    profession = item.Value.ID;
                    break;
                }
            }
            if (profession == -1 && int.TryParse(prof, out int result))
            {
                profession = result;
            }

            return useThisInstead.professions.Contains(profession) is true || (ModEntry.ModConfig.Value.DeveloperOrTestingMode && !ignoreMode);
        }

        public static bool IsGeode(this StardewValley.Object obj)
        {
            var data = ItemRegistry.GetData(obj.QualifiedItemId).RawData;
            if (data is ObjectData objData)
            {
                return objData.GeodeDrops is not null || objData.GeodeDropsDefaultItems;
            }
            return false;
        }
        public static Texture2D GetProfessionIconImage(LevelUpMenu menu)
        {
            int skill = ModEntry.Helper.Reflection.GetField<int>(menu, "currentSkill").GetValue();
            int level = ModEntry.Helper.Reflection.GetField<int>(menu, "currentLevel").GetValue();
            if (skill >= 0 && skill < 5 && level is 15 or 20)
            {
                return DisplayHandler.ProfessionIcons;
            }
            return Game1.mouseCursors;
        }

        public static Rectangle? GetProfessionSourceRect(int x, int y, int width, int height, int whichProfession)
        {
            if (whichProfession > 467800)
            {
                int realID = whichProfession - 467800;
                x = (realID - 30) % 6 * 16;
                y = (realID - 30) / 6 * 16;
            }
            return new(x, y, width, height);
        }
    }
}
