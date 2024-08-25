using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Machines;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using VanillaPlusProfessions.Compatibility;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Talents.Patchers;

namespace VanillaPlusProfessions.Utilities
{
    public static class TalentUtility
    {
        private readonly static string[] GemNodes = new string[] { "2", "4", "6", "8", "10", "12", "14", "44", "46" };
        private readonly static string[] GeodeNodes = new string[] { "75", "76", "77" };
        private readonly static string[] OreNodes = new string[] { "290", "752", "764", "765", "95"};
        public readonly static string[] BlandStones = new string[] { "32", "34", "36", "38", "40", "42", "48", "50", "52", "54", "56", "58", "343", "450", "760" };

        public static StardewValley.Object GetBigTapperOutput(in StardewValley.Object smallOutput)
        {
            return smallOutput.QualifiedItemId is "(O)724" or "(O)725" or "(O)726"
                ? ItemRegistry.Create<StardewValley.Object>("Kedi.VPP.Large" + smallOutput.Name.Replace(" ", ""))
                : smallOutput;
        }

        public static void MakeFarmerInvincible(Farmer player)
        {
            player.temporarilyInvincible = true;
            player.temporaryInvincibilityTimer = 0;
            player.currentTemporaryInvincibilityDuration = 1200;
            player.flashDuringThisTemporaryInvincibility = true;
        }
        public static int FlowersInBeeHouseRange(GameLocation location, Vector2 startTileLocation)
        {
            List<Vector2> validCrops = new();
            List<Vector2> CheckTiles = GetTilesAroundBeeHouse(startTileLocation.X, startTileLocation.Y);
            for (int i = 0; i < CheckTiles.Count; i++)
            {
                if (location.terrainFeatures.TryGetValue(startTileLocation, out var terrainFeature) && terrainFeature is HoeDirt dirt && dirt.crop != null)
                {
                    ParsedItemData data = ItemRegistry.GetData(dirt.crop.indexOfHarvest.Value);
                    if (data != null && data.Category == StardewValley.Object.flowersCategory && dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && !dirt.crop.dead.Value)
                        validCrops.Add(dirt.crop.tilePosition);
                }
            }
            return validCrops.Count;
        }

        public static bool CurrentPlayerHasTalent(string flag, long farmerID = -1, Farmer who = null)
        {
            if (!Context.IsWorldReady)
            {
                return false;
            }
            if (farmerID is not -1)
            {
                who = Game1.getFarmer(farmerID);
            }
            if (who is null)
            {
                who = Game1.player;
                if (who is null)
                    return false;
            }
            return who.mailReceived.Contains(GetFlag(flag));
        }

        static string GetFlag(string name) => TalentCore.Talents.TryGetValue(name, out Talent talent) ? talent.MailFlag : name;

        public static bool AllPlayersHaveTalent(string flag)
        {
            if (!Context.IsWorldReady)
            {
                return false;
            }

            List<bool> bools = new();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (flag is not null)
                {
                    bools.Add(farmer.mailReceived.Contains(GetFlag(flag)));
                }
            }
            return bools.Count > 0 && !bools.Contains(false);
        }
        public static bool AnyPlayerHasTalent(string flag)
        {
            if (!Context.IsWorldReady)
            {
                return false;
            }

            List<bool> bools = new();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (flag is not null)
                {
                    return farmer.mailReceived.Contains(GetFlag(flag));
                }
            }
            return false;
        }
        public static bool HostHasTalent(string flag) => Game1.MasterPlayer?.mailReceived.Contains(GetFlag(flag)) is true;

        public static List<Vector2> GetTilesAroundBeeHouse(float xStart, float yStart)
        {
            return new()
            {
                new(xStart, yStart - 2),
                new(xStart - 1, yStart - 1),
                new(xStart, yStart - 1),
                new(xStart + 1, yStart - 1),
                new(xStart - 2, yStart),
                new(xStart - 1, yStart),
                new(xStart + 1, yStart),
                new(xStart + 2, yStart),
                new(xStart - 1, yStart + 1),
                new(xStart, yStart + 1),
                new(xStart + 1, yStart + 1),
                new(xStart, yStart + 2)
            };
        }

        public static void DetermineGeodeDrop(Item geode, bool update = true)
        {
            if (Utility.IsGeode(geode, true))
            {
                MiningPatcher.IsUpdating = update;
                Item drop = Utility.getTreasureFromGeode(geode);
                MiningPatcher.IsUpdating = false;
                if (!geode.modData.TryAdd(TalentCore.Key_XrayDrop, drop.QualifiedItemId))
                {
                    geode.modData[TalentCore.Key_XrayDrop] = drop.QualifiedItemId;
                }
            }
        }

        public static bool AreConditionsTrueForFish(string query, StardewValley.Object bait, long whoID, Season? season, GameLocation gameLocation)
        {
            Farmer who = Game1.getFarmer(whoID);
            if (who is null)
                return false;

            if (query is null or "")
                return true;

            bool fallback = GameStateQuery.CheckConditions(query, gameLocation) && (season is null || (season is not null && Game1.season == season));

            if (CurrentPlayerHasTalent("Fishing_Fishs_Wishes", farmerID:whoID))
            {
                if (who.professions.Contains(10))
                    return bait is not null;
                else if (bait is not null)
                    return bait.QualifiedItemId == "(O)908" || fallback;
            }
            return fallback;
        }

        public static bool isFavoredMonster(Monster monster, Farmer who)
        {
            CustomMonsterData monsterData = null;
            foreach (var item in ModEntry.VanillaPlusProfessionsAPI.CustomMonsters)
            {
                if (item.Type.Equals(monster.GetType()))
                {
                    monsterData = item;
                    break;
                }
            }

            if (who.mailReceived.Contains("Combat_Monster_Specialist_Ground"))
            {
                if (monsterData is not null)
                    return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Ground;
                else
                    return monster is GreenSlime or BigSlime or Grub or Duggy or LavaLurk or Leaper;
            }
            else if (who.mailReceived.Contains("Combat_Monster_Specialist_Humanoid"))
            {
                if (monsterData is not null)
                    return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Humanoid;
                else
                    return monster is RockGolem or Skeleton or Mummy or ShadowBrute or Shooter or ShadowShaman;
            }
            else if (who.mailReceived.Contains("Combat_Monster_Specialist_Flying"))
            {
                if (monsterData is not null)
                    return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Flying;
                else
                    return monster is Bat or Ghost or AngryRoger or Serpent or BlueSquid or SquidKid or Fly;
            }
            else if (who.mailReceived.Contains("Combat_Monster_Specialist_Armoured"))
            {
                if (monsterData is not null)
                    return monsterData.MonsterType == IVanillaPlusProfessions.MonsterType.Armoured;
                else
                    return monster is DwarvishSentry or RockCrab or HotHead or MetalHead or DinoMonster;
            }

            return false;
        }
        public static void GiftOfTheTalented_OnApply(string name, bool appliedOrUnapplied)
        {
            if (name is "GiftOfTheTalented")
            {
                Game1.activeClickableMenu.exitThisMenu(true);
                Game1.activeClickableMenu.exitThisMenu(true);
                Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)434"), null, true);
            }
        }
        public static void GiftOfTheTalented_OnUnApply(string name, bool appliedOrUnapplied)
        {
            if (name is "GiftOfTheTalented")
            {
                Game1.player.maxStamina.Value -= 40;
                Game1.player.Stamina -= 40;
                Game1.player.maxHealth -= 40;
            }
        }

        internal static void OnItemBasedTalentBoughtOrRefunded(string name, bool appliedOrUnapplied)
        {
            if (name is "SapSipper" or "SugarRush" or "BigFishSmallPond" or "Accessorise")
            {
                Utility.ForEachItem(item =>
                {
                    if (item is StardewValley.Object obj)
                    {
                        if (obj.Category is StardewValley.Object.FishCategory or StardewValley.Object.CookingCategory && name is "SugarRush" or "BigFishSmallPond")
                        {
                            obj.MarkContextTagsDirty();
                        }
                        else if (obj.QualifiedItemId == "(O)92" && name is "SapSipper")
                        {
                            if (AnyPlayerHasTalent("SapSipper"))
                            {
                                obj.Edibility = 3;
                            }
                            else
                            {
                                obj.Edibility = -1;
                            }
                        }
                    }
                    else if (!appliedOrUnapplied && name is "Accessorise" && item is TrinketRing ring)
                    {
                        Game1.player.team.returnedDonations.Add(ring.Trinket);
                        Game1.player.team.newLostAndFoundItems.Value = true;
                        item.ConsumeStack(1);
                    }
                    return true;
                });
            }
            if (name is "Accessorise")
            {
                foreach (var trinketRing in GetAllTrinketRings(Game1.player))
                {
                    Game1.player.team.returnedDonations.Add(trinketRing.Trinket);
                    Game1.player.team.newLostAndFoundItems.Value = true;
                }
            }
        }

        public static bool IsBlandStone(this StardewValley.Object obj) => BlandStones.Contains(obj.ItemId) || obj.HasContextTag(TalentCore.ContextTag_BlandStone);

        public static string GetNodeForRoomAndPillar()
        {
            return Game1.random.ChooseFrom(OreNodes);
        }

        public static string GetIDOfRing(Trinket trinket)
        {
            TrinketData data = trinket.GetTrinketData();

            if (data.TrinketMetadata?.TryGetValue(TalentCore.Key_AccessoriseRing, out string value) is true && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            ModEntry.ModMonitor.Log($"Cannot find ring ID for trinket {trinket.ItemId}. This should be reported to the author of the mod its coming from and not to KediDili.", LogLevel.Warn);
            return null;
        }

        public static int BuffDescriptionLength(string name)
        {
            int minimum_size = 272;
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                minimum_size = 384;
            }
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
            {
                minimum_size = 336;
            }
            return Math.Max(minimum_size, (int)Game1.dialogueFont.MeasureString(name ?? "").X);
        }
        public static List<TrinketRing> GetAllTrinketRings(Farmer who)
        {
            List<TrinketRing> result = new();
            if (ModEntry.WearMoreRingsAPI.Value is not null)
            {
                for (int i = 0; i < ModEntry.WearMoreRingsAPI.Value.RingSlotCount(); i++)
                {
                    if (ModEntry.WearMoreRingsAPI.Value.GetRing(i) is TrinketRing trinketRing)
                        result.Add(trinketRing);
                }
            }
            else
            {
                if (who.rightRing.Value is TrinketRing ring1)
                {
                    result.Add(ring1);
                }
                if (who.leftRing.Value is TrinketRing ring2)
                {
                    result.Add(ring2);
                }
            }
            return result;
        }

        public static Item AccessoriseMachineRule(StardewValley.Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            
            if (inputItem is Trinket trinket)
            {
                if (probe is false)
                {
                    Game1.player.addItemToInventory(new TrinketRing(trinket));
                    inputItem = null;
                    Game1.player.ActiveObject = null;
                    return null;
                }
                else
                {
                    return inputItem;
                }
            }
            else if (inputItem is TrinketRing ring)
            {
                return ring.Trinket;
            }
            
            return null;
        }

        public static void GemAndGeodeNodes(bool flag, List<Vector2> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string str = nodeID(flag);
                if (str is not "-1")
                    Game1.player.currentLocation.Objects[list[i]] = ItemRegistry.Create<StardewValley.Object>(str);
            }
        }

        private static string nodeID(bool flag)
        {
            if (Game1.player.currentLocation is MineShaft shaft)
            {
                return shaft.getMineArea(shaft.mineLevel) switch
                {
                    10 => flag ? Game1.random.ChooseFrom(GemNodes) : GeodeNodes[0],
                    40 => flag ? Game1.random.ChooseFrom(GemNodes) : GeodeNodes[1],
                    80 => flag ? Game1.random.ChooseFrom(GemNodes) : GeodeNodes[2],
                    121 => Game1.random.ChooseFrom(flag ? GemNodes : GeodeNodes),
                    _ => "-1",
                };
            }
            else if (Game1.player.currentLocation is VolcanoDungeon)
            {                
                return flag ? Game1.random.ChooseFrom(GemNodes) : "819";
            }
            else
            {
                string locdata = Game1.player.currentLocation.GetData().CustomFields[flag ? TalentCore.Key_CrystalCavern : TalentCore.Key_Upheaval].Trim();
                if (!string.IsNullOrEmpty(locdata))
                {
                    string[] strings = locdata.Split('/', StringSplitOptions.TrimEntries);
                    return Game1.random.ChooseFrom(strings);
                }
                else
                {
                    return Game1.random.ChooseFrom(flag ? GemNodes : GeodeNodes);
                }
            }
        } 
    }
}
