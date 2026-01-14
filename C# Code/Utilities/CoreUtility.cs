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
using StardewValley.TerrainFeatures;
using System.Reflection;
using HarmonyLib;
using StardewValley.Objects.Trinkets;
using VanillaPlusProfessions.Talents.Patchers;
using StardewValley.Buildings;

namespace VanillaPlusProfessions.Utilities
{
    public class CoreUtility
    {
        internal static bool confirmTrinkets = false;

        public static bool IsOverlayValid()
        {
            if (Game1.player.FarmingLevel > 10 || Game1.player.FishingLevel > 10 || Game1.player.ForagingLevel > 10 || Game1.player.MiningLevel > 10 || Game1.player.CombatLevel > 10)
            {
                return true;
            }
            else if (ModEntry.CoreModEntry.Value.SpaceCoreAPI is not null)
            {
                foreach (var item in ModEntry.CoreModEntry.Value.SpaceCoreAPI?.GetCustomSkills())
                {
                    if (ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetLevelForCustomSkill(Game1.player, item) > 10)
                    {
                        return true;
                    }
                }
                return false;
            }
            return !(DisplayHandler.CoreDisplayHandler.Value.MyCustomSkillBars is null || DisplayHandler.CoreDisplayHandler.Value.LittlePlus is null);
        }

        public static void PrintError(Exception e, string @class, string method, string typeOfPatch, bool isRunning = false)
        {
            if (isRunning)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"There has been an error while running {method} in {@class} which has been {typeOfPatch}, details below:", LogLevel.Error);
            }
            else
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"There has been an error while {typeOfPatch} {method} in {@class}, details below:", LogLevel.Error);
            }
            ModEntry.CoreModEntry.Value.ModMonitor.Log(e.ToString(), LogLevel.Error);
        }
        public static void PatchMethod(string patcherName, string methodName, MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
        {
            bool success = true;
            try
            {
                ModEntry.CoreModEntry.Value.Harmony.Patch(original, prefix, postfix, transpiler);
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
                    if (ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode)
                    {
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"{patcherName} successfully patched {methodName}.");
                    }
                }
                else
                {
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"This is an error thrown by VPP. Some features may not work, but this shouldn't break your game. Reproduce this with only VPP before you make a bug report and make sure it hasn't been reported before.", LogLevel.Warn);
                }
            }
        }
        public static void Test(string command, string[] args)
        {

        }
        public static void clearTrinkets(string command, string[] args)
        {
            var monitor = ModEntry.GetMe().ModMonitor;
            if (Context.IsWorldReady)
            {
                if (confirmTrinkets)
                {
                    Game1.player.UnapplyAllTrinketEffects();
                    Game1.player.trinketItems.Clear();
                    monitor.Log("All non-ring trinket items and all trinket effects are now unapplied and cleared. Re-equip your previous trinkets/trinket rings to get the effects back.", LogLevel.Info);
                    confirmTrinkets = false;
                }
                else
                {
                    monitor.Log("Before running this command, you must unequip ALL of your regular trinkets. Otherwise they will be lost for good. Run the command again when you've unequipped all and ready to clear trinkets.", LogLevel.Warn);
                    confirmTrinkets = true;
                }
            }
            else
            {
                monitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static bool AnyPlayerHasProfession(string prof)
        {
            if (!Context.IsWorldReady)
                return ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode;
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
            return ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode;
        }
        public static void PerformFire(GameLocation location, Farmer who, Slingshot slingshot)
        {
            if (slingshot.attachments[0] != null)
            {

                int backArmDistance = slingshot.GetBackArmDistance(who);

                if (backArmDistance > 4 && !slingshot.canPlaySound)
                {
                    int mouseX = slingshot.aimPos.X;
                    int mouseY = slingshot.aimPos.Y;
                    Vector2 shoot_origin = slingshot.GetShootOrigin(slingshot.lastUser);
                    Vector2 v = Utility.getVelocityTowardPoint(shoot_origin, slingshot.AdjustForHeight(new Vector2(mouseX, mouseY)), 256f);

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

                    Vector2 target1 = target + (normalizethis * 96) - new Vector2(24f, 24f);
                    Vector2 target2 = target - (normalizethis * 96) - new Vector2(24f, 24f);


                    slingshot.canPlaySound = false;
                    ModEntry.CoreModEntry.Value.Helper.Reflection.GetMethod(slingshot, "updateAimPos").Invoke(null);

                    Vector2 shootOrigin = slingshot.GetShootOrigin(who) - new Vector2(32f, 32f);

                    if (slingshot.attachments[0] is Trinket trinket && trinket?.QualifiedItemId == "(TR)MagicQuiver")
                    {
                        Vector2 motion1 = Utility.getVelocityTowardPoint(shootOrigin, target1, 2f);
                        float projectileRotation1 = (float)Math.Atan2(motion1.Y, motion1.X) + (float)Math.PI / 2f;

                        BasicProjectile p1 = new(Game1.random.Next((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MinDamage ?? 10, ((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MaxDamage ?? 10) + 1), 16, 0, 0, 0f, motion1.X, motion1.Y, shootOrigin, null, null, null, explode: false, damagesMonsters: true, location, who)
                        {
                            IgnoreLocationCollision = true
                        };
                        p1.ignoreObjectCollisions.Value = true;
                        p1.acceleration.Value = motion1;
                        p1.maxVelocity.Value = 24f;
                        p1.projectileID.Value = 14;
                        p1.startingRotation.Value = projectileRotation1;
                        p1.alpha.Value = 0.001f;
                        p1.alphaChange.Value = 0.05f;
                        p1.light.Value = true;
                        p1.collisionSound.Value = "magic_arrow_hit";
                        location.projectiles.Add(p1);
                        location.playSound("magic_arrow");

                        Vector2 motion = Utility.getVelocityTowardPoint(shootOrigin, target2, 2f);
                        float projectileRotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;

                        BasicProjectile p = new(Game1.random.Next((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MinDamage ?? 10, ((trinket.GetEffect() as MagicQuiverTrinketEffect)?.MaxDamage ?? 10) + 1), 16, 0, 0, 0f, motion.X, motion.Y, shootOrigin, null, null, null, explode: false, damagesMonsters: true, location, who)
                        {
                            IgnoreLocationCollision = true
                        };
                        p.ignoreObjectCollisions.Value = true;
                        p.acceleration.Value = motion;
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
                    else if (slingshot.attachments[0]?.Stack > 1)
                    {
                        string Id = slingshot.attachments[0].ItemId;
                        slingshot.attachments[0].ConsumeStack(2);
                        int damage = 1;
                        float damageMod = (slingshot.ItemId == "33") ? 2f : ((!(slingshot.ItemId == "34")) ? 1f : 4f);
                        Vector2 motion1 = Utility.getVelocityTowardPoint(shootOrigin, target1, 2f);

                        location.projectiles.Add(new BasicProjectile((int)(damageMod * (damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(Math.PI / (double)(64f + Game1.random.Next(-63, 64))), motion1.X * 2, motion1.Y * 2, shootOrigin, "hammer", null, null, explode: false, damagesMonsters: true, location, who, null, Id)
                        { IgnoreLocationCollision = false });

                        foreach (var item in slingshot.enchantments)
                            if (item is SlingshotEnchantment slingshotEnchantment)
                                slingshotEnchantment.OnShoot(location, slingshot);

                        Vector2 motion2 = Utility.getVelocityTowardPoint(shootOrigin, target2, 2f);
                        location.projectiles.Add(new BasicProjectile((int)(damageMod * (damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(Math.PI / (double)(64f + Game1.random.Next(-63, 64))), motion2.X * 2, motion2.Y * 2, shootOrigin, "hammer", null, null, explode: false, damagesMonsters: true, location, who, null, Id)
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

        internal static void showXPLeft(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Load a save first!", LogLevel.Warn);
                return;
            }
            StringBuilder sb = new();
            int farming = Game1.player.GetUnmodifiedSkillLevel(0),
                foraging = Game1.player.GetUnmodifiedSkillLevel(2),
                mining = Game1.player.GetUnmodifiedSkillLevel(3),
                fishing = Game1.player.GetUnmodifiedSkillLevel(1),
                combat = Game1.player.GetUnmodifiedSkillLevel(4);
            sb.AppendLine("");
            sb.AppendLine("    - Skill Level Experiences -    ");
            sb.AppendLine($"Farming: {Game1.player.experiencePoints[0]}/" + (farming > 19 ? "120000 (Maxed)" : $"{(farming >= 10 ? ModEntry.CoreModEntry.Value.levelExperiences[farming - 10] : Farmer.getBaseExperienceForLevel(farming))} (To {farming + 1})"));
            sb.AppendLine($"Mining: {Game1.player.experiencePoints[3]}/" + (mining > 19 ? "120000 (Maxed)" : $"{(mining >= 10 ? ModEntry.CoreModEntry.Value.levelExperiences[mining - 10] : Farmer.getBaseExperienceForLevel(mining))} (To {mining + 1})"));
            sb.AppendLine($"Foraging: {Game1.player.experiencePoints[2]}/" + (foraging > 19 ? "120000 (Maxed)" : $"{(foraging >= 10 ? ModEntry.CoreModEntry.Value.levelExperiences[foraging - 10] : Farmer.getBaseExperienceForLevel(foraging))} (To {foraging + 1})"));
            sb.AppendLine($"Fishing: {Game1.player.experiencePoints[1]}/" + (fishing > 19 ? "120000 (Maxed)" : $"{(fishing >= 10 ? ModEntry.CoreModEntry.Value.levelExperiences[fishing - 10] : Farmer.getBaseExperienceForLevel(fishing))} (To {fishing + 1})"));
            sb.AppendLine($"Combat: {Game1.player.experiencePoints[4]}/" + (combat > 19 ? "120000 (Maxed)" : $"{(combat >= 10 ? ModEntry.CoreModEntry.Value.levelExperiences[combat - 10] : Farmer.getBaseExperienceForLevel(combat))} (To {combat + 1})"));
            ModEntry.CoreModEntry.Value.ModMonitor.Log(sb.ToString(), LogLevel.Info);
        }

        internal static void remove(string command, string[] args)
        {
            if (Context.IsWorldReady || !Context.IsMainPlayer)
            {
                if (!Context.IsMainPlayer)
                {
                    ModEntry.CoreModEntry.Value.ModMonitor.Log("This command can be only run by the host.", LogLevel.Error);
                    return;
                }
                if (args.Length > 0 && args[1].ToLower() == "true")
                {
                    int FishFarm = 0, FrogEggs = 0, SlimeHutchWater = 0, WildGrowth = 0, SlimeWaterLoss = 0;

                    Utility.ForEachBuilding(Building =>
                    {
                        if (Building is FishPond)
                        {
                            if (Building.modData.Remove(Constants.Key_FishRewardOrQuestDayLeft))
                                FishFarm++;

                            if (Building.modData.TryGetValue(Constants.Key_HiddenBenefit_FrogEggs, out string val))
                            {
                                Game1.player.team.newLostAndFoundItems.Value = true;
                                Game1.player.team.returnedDonations.Add(val.StringToTrinket());
                                if (Building.modData.Remove(Constants.Key_HiddenBenefit_FrogEggs))
                                {
                                    FrogEggs++;
                                }
                            }
                        }

                        if (Building.modData.Remove(Constants.Key_IsSlimeHutchWatered) && Building.GetIndoors() is SlimeHutch)
                            SlimeHutchWater++;

                        if (Building.GetIndoors() is AnimalHouse house)
                        {
                            foreach (var (id, animal) in house.Animals.Pairs)
                            {
                                if (!house.animalsThatLiveHere.Contains(id))
                                    continue;

                                if (animal.modData.Remove(Constants.Key_WildGrowth))
                                    WildGrowth++;
                            }
                        }
                        else if (Building.GetIndoors() is SlimeHutch hutch)
                        {
                            foreach (var slime in hutch.characters)
                            {
                                if (slime.modData.Remove(Constants.Key_SlimeWateredDaysSince))
                                    SlimeWaterLoss++;
                            }
                        }
                        return true;
                    });
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Fish-Farm data erased from {FishFarm} fish ponds.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"{FrogEggs} Frog Eggs erased and placed to Lost And Found box from fish ponds.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased custom watering data from {SlimeHutchWater} slime hutches.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased custom water loss data from {SlimeWaterLoss} slimes.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased Wild Growth data from {WildGrowth} farm animals.");

                    int RainLocs = 0, FruitTreeTappers = 0, GiantCropData = 0, XrayDrop = 0, FairyBoxData = 0, Resurgence = 0, Slingshots = 0,
                        Accessorise = 0, ParrotPerches = 0;
                    Utility.ForEachLocation(Loc =>
                    {
                        if (Loc.modData.Remove(Constants.Key_WasRainingHere))
                        {
                            RainLocs++;
                        }
                        foreach (var item in Loc.terrainFeatures.Pairs)
                        {
                            if (item.Value is FruitTree tree)
                            {
                                if (tree.modData.Remove(Constants.Key_TFHasTapper) & tree.modData.Remove(Constants.Key_TFTapperID) & tree.modData.Remove(Constants.Key_TFTapperDaysLeft))
                                    FruitTreeTappers++;
                            }
                            else if (item.Value is GiantCrop crop)
                            {
                                if (crop.modData.Remove(Constants.Key_TFHasTapper) & crop.modData.Remove(Constants.Key_TFTapperID) & crop.modData.Remove(Constants.Key_TFTapperDaysLeft))
                                    GiantCropData++;
                            }
                        }
                        return true;
                    });
                    Utility.ForEachCrop(crop =>
                    {
                        crop.modData.Remove(Constants.Key_HiddenBenefit_Crop);
                        return true;
                    });
                    Utility.ForEachItem(item =>
                    {
                        if (item.modData.Remove(Constants.Key_XrayDrop) | item.modData.Remove(Constants.Key_XrayDrop2))
                            XrayDrop++;

                        if (item.modData.Remove(Constants.Key_HiddenBenefit_FairyBox))
                            FairyBoxData++;

                        if (item.modData.Remove(Constants.Key_Resurgence))
                            Resurgence++;

                        if (item is Slingshot slingshot)
                        {
                            slingshot.ClearEnchantments();
                            Slingshots++;
                        }

                        if (item is TrinketRing ring)
                        {
                            Accessorise++;
                            Game1.player.team.returnedDonations.Add(ring.GetRingTrinket(true));
                            if (Game1.player.isWearingRing(ring.ItemId))
                            {
                                if (Game1.player.leftRing.Value?.GetsEffectOfRing(ring.ItemId) is true)
                                    Game1.player.leftRing.Value.onUnequip(Game1.player);

                                if (Game1.player.rightRing.Value?.GetsEffectOfRing(ring.ItemId) is true)
                                    Game1.player.rightRing.Value.onUnequip(Game1.player);
                            }
                            Game1.player.team.newLostAndFoundItems.Value = true;
                            item = null;
                        }

                        if (item is ParrotPerch perch)
                        {
                            if (perch.heldObject.Value is not null)
                            {
                                Game1.player.team.returnedDonations.Add(perch.heldObject.Value);
                                Game1.player.team.newLostAndFoundItems.Value = true;
                            }
                            item = null;
                            ParrotPerches++;
                        }

                        return true;
                    });
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Yesterday's weather data erased from {RainLocs} game locations.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Fae Blessings data erased from crops and the farm.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased tapper data from {FruitTreeTappers} fruit trees.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased tapper data from {GiantCropData} giant crops.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased X-ray predictions from {XrayDrop} geodes.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased Hidden Benefits data from {FairyBoxData} fairy boxes.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased Resurgence data from {Resurgence} watering cans.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased enchantment data from {Slingshots} slingshots. - This is crucial for save integrity.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Returned trinkets from {Accessorise} trinket rings, then destroyed the rings. - This is crucial for save integrity.");
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased {ParrotPerches} parrot perches, and returned parrot eggs if they had any. - This is crucial for save integrity.");
                }
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (args.Length > 0 && args[1].ToLower() == "true")
                    {
                        farmer.modData.Remove(Constants.Key_ForageGuessItemID);
                        farmer.modData.Remove(Constants.Key_DaysLeftForForageGuess);
                        farmer.modData.Remove(Constants.Key_HasFoundForage);
                        farmer.modData.Remove(Constants.Key_TalentPoints);
                        farmer.modData.Remove(Constants.Key_DisabledTalents);
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased Forage Guess mini game and talent points data from Farmer {farmer.Name}.");
                    }

                    foreach (var item in ModEntry.Professions.Values)
                        farmer.professions.Remove(item.ID);
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased VPP Professions from Farmer {farmer.Name}.");

                    foreach (var item in TalentCore.Talents.Values)
                    {
                        if (item.Branches is not null && item.Branches.Length > 0)
                        {
                            foreach (var branch in item.Branches)
                            {
                                farmer.mailReceived.Remove(branch.Flag);
                                break;
                            }
                        }
                        farmer.mailReceived.Remove(item.MailFlag);
                        farmer.mailReceived.Remove(item.MailFlag + "_disabled");
                    }
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Erased All Talent flags from Farmer {farmer.Name}.");

                    farmer.mailReceived.Remove(Constants.Key_PointsCalculated);
                    if (farmer.farmingLevel.Value > 10)
                    {
                        farmer.farmingLevel.Value = 10;
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {farmer.Name}'s farming level.");
                    }
                    if (farmer.foragingLevel.Value > 10)
                    {
                        farmer.foragingLevel.Value = 10;
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {farmer.Name}'s foraging level.");
                    }
                    if (farmer.miningLevel.Value > 10)
                    {
                        farmer.miningLevel.Value = 10;
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {farmer.Name}'s mining level.");
                    }
                    if (farmer.fishingLevel.Value > 10)
                    {
                        farmer.fishingLevel.Value = 10;
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {farmer.Name}'s fishing level.");
                    }
                    if (farmer.combatLevel.Value > 10)
                    {
                        farmer.combatLevel.Value = 10;
                        ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {farmer.Name}'s combat level.");
                    }
                }
                TalentCore.DisabledTalents.Clear();
                ModEntry.CoreModEntry.Value.IsUninstalling = true;
                TalentCore.TalentCoreEntry.Value.TalentPointCount = 0;
            }
            else
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Load a save first!", LogLevel.Warn);
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
                foreach (var item in ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetCustomSkills())
                {
                    stringBuilder.AppendLine(item + ": " + ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetLevelForCustomSkill(Game1.player, item));
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Skill Experience -    ");
                stringBuilder.AppendLine("Farming: " + Game1.player.experiencePoints[0]);
                stringBuilder.AppendLine("Fishing: " + Game1.player.experiencePoints[1]);
                stringBuilder.AppendLine("Foraging: " + Game1.player.experiencePoints[2]);
                stringBuilder.AppendLine("Mining: " + Game1.player.experiencePoints[3]);
                stringBuilder.AppendLine("Combat: " + Game1.player.experiencePoints[4]);
                foreach (var item in ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetCustomSkills())
                {
                    stringBuilder.AppendLine(item + ": " + ModEntry.CoreModEntry.Value.SpaceCoreAPI.GetExperienceForCustomSkill(Game1.player, item));
                }
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Main Config Options -    ");
                stringBuilder.AppendLine($"Color Blindness Changes: {ModEntry.CoreModEntry.Value.ModConfig.ColorBlindnessChanges}");
                stringBuilder.AppendLine($"Developer Or Testing Mode: {ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode}");
                stringBuilder.AppendLine($"Mastery Cave Changes: {ModEntry.CoreModEntry.Value.ModConfig.MasteryCaveChanges}");
                stringBuilder.AppendLine($"Stamina Cost Adjustments: {ModEntry.CoreModEntry.Value.ModConfig.StaminaCostAdjustments}");
                stringBuilder.AppendLine($"Professions Only: {ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly}");
                stringBuilder.AppendLine($"Talent Hint Level: {ModEntry.CoreModEntry.Value.ModConfig.TalentHintLevel}");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Balance Options -    ");
                stringBuilder.AppendLine($"Cycle Of Life Chance: {ModEntry.CoreModEntry.Value.ModConfig.CycleOfLife_Chance}");
                stringBuilder.AppendLine($"Wild Growth Chance: {ModEntry.CoreModEntry.Value.ModConfig.WildGrowth_Chance}");
                stringBuilder.AppendLine($"Fallout Chance: {ModEntry.CoreModEntry.Value.ModConfig.Fallout_Chance}");
                stringBuilder.AppendLine($"Volatility Chance: {ModEntry.CoreModEntry.Value.ModConfig.Volatility_Chance}");
                stringBuilder.AppendLine($"Crystal Cavern Chance: {ModEntry.CoreModEntry.Value.ModConfig.CrystalCavern_Chance}");
                stringBuilder.AppendLine($"Upheaval Chance: {ModEntry.CoreModEntry.Value.ModConfig.Upheaval_Chance}");
                stringBuilder.AppendLine($"Spawning Season Chance: {ModEntry.CoreModEntry.Value.ModConfig.SpawningSeason_Chance}");
                stringBuilder.AppendLine($"Aquaculturalist Multiplier: {ModEntry.CoreModEntry.Value.ModConfig.Aquaculturalist_Multiplier}");
                stringBuilder.AppendLine($"Admiration Multiplier: {ModEntry.CoreModEntry.Value.ModConfig.Admiration_Multiplier}");
                stringBuilder.AppendLine($"Meditation Health: {ModEntry.CoreModEntry.Value.ModConfig.Meditation_Health}");
                stringBuilder.AppendLine($"Down In The Depths Stones: {ModEntry.CoreModEntry.Value.ModConfig.DownInTheDepths_Stones}");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("    - Talents & Professions -    ");
                stringBuilder.AppendLine($"Talent Points: {TalentCore.TalentCoreEntry.Value.TalentPointCount}");
                stringBuilder.AppendLine($"Unlocked Achievement Count: {Game1.player.achievements.Count}");
                stringBuilder.AppendLine($"Save Changes Applied: {Game1.player.mailReceived.Contains(Constants.Key_PointsCalculated)}");
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
                ModEntry.CoreModEntry.Value.ModMonitor.Log(stringBuilder.ToString(), LogLevel.Debug);
            }
            else
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static void recalculate(string command, string[] args)
        {
            if (Context.IsWorldReady)
            {
                int number = 0, newLevels;
                ModEntry.CoreModEntry.Value.IsRecalculatingPoints = true;
                //Farming
                if (Game1.player.experiencePoints[0] > ModEntry.CoreModEntry.Value.levelExperiences[0])
                {
                    newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
                    for (int i = Game1.player.farmingLevel.Value + 1; i <= newLevels; i++)
                    {
                        Game1.player.newLevels.Add(new(0, i));
                    }
                    Game1.player.farmingLevel.Value = newLevels;
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {Game1.player.Name}'s farming level.");
                }

                //Fishing
                if (Game1.player.experiencePoints[1] > ModEntry.CoreModEntry.Value.levelExperiences[0])
                {
                    newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
                    for (int i = Game1.player.fishingLevel.Value + 1; i <= newLevels; i++)
                    {
                        Game1.player.newLevels.Add(new(1, i));
                    }
                    Game1.player.fishingLevel.Value = newLevels;
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {Game1.player.Name}'s fishing level.");
                }

                //Foraging
                if (Game1.player.experiencePoints[2] > ModEntry.CoreModEntry.Value.levelExperiences[0])
                {
                    newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
                    for (int i = Game1.player.foragingLevel.Value + 1; i <= newLevels; i++)
                    {
                        Game1.player.newLevels.Add(new(2, i));
                    }
                    Game1.player.foragingLevel.Value = newLevels;
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {Game1.player.Name}'s foraging level.");
                }

                //Mining
                if (Game1.player.experiencePoints[3] > ModEntry.CoreModEntry.Value.levelExperiences[0])
                {
                    newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
                    for (int i = Game1.player.miningLevel.Value + 1; i <= newLevels; i++)
                    {
                        Game1.player.newLevels.Add(new(3, i));
                    }
                    Game1.player.miningLevel.Value = newLevels;
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {Game1.player.Name}'s mining level.");
                }

                //Combat
                if (Game1.player.experiencePoints[4] > ModEntry.CoreModEntry.Value.levelExperiences[0])
                {
                    newLevels = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
                    for (int i = Game1.player.combatLevel.Value + 1; i <= newLevels; i++)
                    {
                        Game1.player.newLevels.Add(new(4, i));
                    }
                    Game1.player.combatLevel.Value = newLevels;
                    ModEntry.CoreModEntry.Value.ModMonitor.Log($"Readjusted Farmer {Game1.player.Name}'s combat level.");
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

                foreach (var item in TalentCore.Talents.Keys)
                {
                    if (TalentUtility.CurrentPlayerHasTalent(item, ignoreDisabledTalents: false))
                    {
                        number--;
                    }
                }

                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Farmer {Game1.player.Name}'s supposed talent point count: {number}.", LogLevel.Debug);
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Farmer {Game1.player.Name}'s current talent point count: {TalentCore.TalentCoreEntry.Value.TalentPointCount}.", LogLevel.Debug);
                TalentCore.TalentCoreEntry.Value.AddTalentPoint(number - TalentCore.TalentCoreEntry.Value.TalentPointCount, false);
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Farmer {Game1.player.Name}'s new talent point count: {TalentCore.TalentCoreEntry.Value.TalentPointCount}.", LogLevel.Debug);
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"(Negative numbers dont mean that there's a bug with this command, just that you had more points than you were supposed to have, likely because of a bug in point rewarding code. Reset your talent trees to get rid of it.)", LogLevel.Debug);
                ModEntry.CoreModEntry.Value.IsUninstalling = false;
                Game1.player.mailReceived.Add(Constants.Key_PointsCalculated);
            }
            else
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Load a save first!", LogLevel.Warn);
            }
        }

        public static int GetMaxLevel()
        {
            return ModEntry.CoreModEntry.Value.ModConfig.MasteryCaveChanges;
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

            return useThisInstead.professions.Contains(profession) is true || (ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode && !ignoreMode);
        }

        public static Texture2D GetProfessionIconImage(LevelUpMenu menu)
        {
            int skill = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<int>(menu, "currentSkill").GetValue();
            int level = ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<int>(menu, "currentLevel").GetValue();
            if (skill >= 0 && skill < 5 && level is 15 or 20)
            {
                return DisplayHandler.CoreDisplayHandler.Value.ProfessionIcons;
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