using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley;
using System.Collections.Generic;
using StardewValley.GameData.WildTrees;
using System.Linq;
using StardewValley.GameData.FishPonds;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Weapons;
using StardewValley.Menus;
using VanillaPlusProfessions.Utilities;
using System.Data;
using StardewModdingAPI.Utilities;
using VanillaPlusProfessions.Talents;
using VanillaPlusProfessions.Craftables;

namespace VanillaPlusProfessions
{
    static class ContentEditor
    {
        private static List<WildTreeItemData> ShakerData = new();
        internal static Dictionary<string, string> BuccaneerData = new();
        internal static Dictionary<string, string> ContentPaths = new();

        internal static Dictionary<string, string> NodeMakerData = new();
        internal static Dictionary<string, ClumpData> ResourceClumpData = new();

        internal static void Initialize()
        {
            ModEntry.Helper.Events.Content.AssetRequested += OnAssetRequested;
            ShakerData = ModEntry.Helper.ModContent.Load<List<WildTreeItemData>>("assets\\ShakerData.json");
            BuccaneerData = ModEntry.Helper.ModContent.Load<Dictionary<string, string>>("assets\\BuccaneerData.json");
            NodeMakerData = ModEntry.Helper.ModContent.Load<Dictionary<string, string>>("assets\\NodeMakerData.json");
            ResourceClumpData = ModEntry.Helper.ModContent.Load<Dictionary<string, ClumpData>> ("assets\\ResourceClumpData.json");
            ContentPaths = new()
            {
                { "ItemSpritesheet", "TileSheets\\KediDili.VPPData.CP\\ItemIcons" },
                { "MachinerySpritesheet", "TileSheets\\KediDili.VPPData.CP\\Machinery" },
                { "ProfessionIcons", "VanillaPlusProfessions\\ProfessionIcons" },
                { "InsiderInfo", "VanillaPlusProfessions\\InsiderInfo" },
                { "TalentBG", "VanillaPlusProfessions\\TalentBG" },
                { "TalentSchema", "VanillaPlusProfessions\\TalentSchema" },
                { "BundleIcons", "VanillaPlusProfessions\\BundleIcons" },
                { "SkillBars", "VanillaPlusProfessions\\SkillBars" },
            };
        }
        internal static void HandleWildTrees(ref IDictionary<string, WildTreeData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession("Shaker") is true) //Shaker ++
            {
                foreach (var item in editor)
                    if (item.Value.IsLeafy || item.Value.IsLeafyInWinter)
                    {
                        if (item.Value.ShakeItems is null)
                            item.Value.ShakeItems = ShakerData;
                        else
                            item.Value.ShakeItems.AddRange(ShakerData);
                    }
            }
            if (CoreUtility.AnyPlayerHasProfession("Arborist") is true) //Arborist ++
                foreach (var item in editor)
                    if (item.Value.GrowthChance * 3 / 2 <= 1)
                        item.Value.GrowthChance *= 3 / 2;
            WildTreeChopItemData data = new()
            {
                Id = "Kedi.VPP.NatureTalents",
                ItemId = "RANDOM_ITEMS (O)",
                ForStump = false,
                PerItemCondition = "ITEM_CONTEXT_TAG Target category_forage",
                Condition = "PLAYER_HAS_MAIL Current Foraging_Nature_Secrets",
                Chance = 1
            };
            foreach (var item in editor)
            {
                item.Value.ChopItems ??= new();
                item.Value.ChopItems.Add(data);
            }
        }
        internal static void HandleObjects(ref IDictionary<string, ObjectData> editor)
        {
            if (CoreUtility.CurrentPlayerHasProfession("Ranger")) //Ranger ++
            {
                var list = (from element in editor
                            where element.Value.Category == Object.GreensCategory && element.Value.ContextTags.Contains("forage_item")
                            select element.Key).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    editor[list[i]].Price *= 2;
                }
            }
            if (CoreUtility.CurrentPlayerHasProfession("Adventurer")) //Adventurer ++
            {
                var list = (from element in editor
                           where element.Value.Category == Object.sellAtFishShopCategory || element.Value.ContextTags.Contains("forage_item_beach") || element.Value.ContextTags.Contains("forage_item_secret") || element.Value.ContextTags.Contains("forage_item_mines")
                           select element.Key).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    editor[list[i]].Price *= 2;
                }
            }
            if (CoreUtility.CurrentPlayerHasProfession("Mineralogist")) //Mineralogist
            {
                editor["749"].GeodeDropsDefaultItems = false;
                editor["537"].GeodeDropsDefaultItems = false;
                editor["536"].GeodeDropsDefaultItems = false;
                editor["535"].GeodeDropsDefaultItems = false;
                editor["537"].GeodeDrops = editor["749"].GeodeDrops;
                editor["536"].GeodeDrops = editor["749"].GeodeDrops;
                editor["535"].GeodeDrops = editor["749"].GeodeDrops;
            }            
            if (TalentUtility.CurrentPlayerHasTalent("Fishing_Roemance"))
            {
                editor["812"].Price *= 5 / 4;
            }
            if (TalentUtility.CurrentPlayerHasTalent("Misc_HauteCuisine"))
            {
                foreach (var item in editor)
                    if (item.Value.Category == Object.CookingCategory)
                        item.Value.Price *= 2;
            }
            if (TalentUtility.CurrentPlayerHasTalent("Misc_InsiderInfo"))
            {
                Dictionary<string, string> InsiderInfo = ModEntry.Helper.GameContent.Load<Dictionary<string, string>>(ContentPaths["InsiderInfo"]);
                foreach (var item in InsiderInfo)
                {
                    if (Game1.player.friendshipData.TryGetValue(item.Key, out Friendship val) && val.Points >= 1500)
                    {
                        string[] items = ArgUtility.SplitBySpace(item.Value.Replace(",", " "));
                        for (int i = 0; i < items.Length; i++)
                        {
                            editor[items[i]].Price = (int)(editor[items[i]].Price * 0.8f);
                        }
                    }
                }
            }
            editor["420"].ContextTags.Add(TalentCore.ContextTag_PoisonousMushroom);
        }
        internal static void HandleLocations(ref IDictionary<string, LocationData> editor)
        {
            if (TalentUtility.AnyPlayerHasTalent("Foraging_Clear_As_Mud") || (TalentUtility.AnyPlayerHasTalent("Fishing_Crab_Rave") && Game1.MasterPlayer?.isWearingRing("(O)810") is true))
            {
                foreach (var loc in editor)
                {
                    if (TalentUtility.HostHasTalent("Fishing_Crab_Rave") && Game1.MasterPlayer?.isWearingRing("(O)810") is true)
                    {
                        foreach (var fish in loc.Value.Fish)
                        {
                            if (fish.Chance * 2 <= 1f && fish.ItemId is "(O)717" or "(O)716" or "(O)715")
                            {
                                fish.Chance *= 2;
                            }
                        }
                    }
                    if (TalentUtility.HostHasTalent("Foraging_Clear_As_Mud"))
                    {
                        loc.Value.ChanceForClay = 0.1;
                    }
                }
            }
            if (TalentUtility.AnyPlayerHasTalent("Foraging_Sea_Change") && Game1.Date?.DayOfWeek is System.DayOfWeek.Friday)
            {
                editor["Beach"].MaxDailyForageSpawn *= 2;
                editor["Beach"].MaxSpawnedForageAtOnce *= 2;
                foreach (var item in editor["Beach"].Forage)
                {
                    if (item.Chance * 2 < 1)
                        item.Chance *= 2;
                    else
                        item.Chance = 1;
                }
            }
            if (TalentUtility.AnyPlayerHasTalent("Foraging_Renewing_Mist") && !Game1.isFestival())
            {
                foreach (var item in Game1.locations)
                {
                    if (GameStateQuery.CheckConditions($"{ModEntry.Manifest.UniqueID}_WasRainingHereYesterday", item))
                    {
                        var data = item.GetData();
                        if (data is not null && item.currentEvent?.isFestival is false)
                        {
                            data.MaxDailyForageSpawn *= 2;
                            data.MinDailyForageSpawn *= 2;
                            data.MaxSpawnedForageAtOnce *= 2;
                        }
                    }
                }
            }
        }
        internal static void HandlFishPonds(ref List<FishPondData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession("Aquaculturalist"))
            {
                for (int i = 0; i < editor.Count; i++)
                {
                    for (int f = 0; f < editor[i].ProducedItems.Count; f++)
                    {
                        if (editor[i].ProducedItems[f].ItemId.Contains("812"))
                        {
                            editor[i].ProducedItems[f].MinStack *= 2;
                            editor[i].ProducedItems[f].MinStack *= 2;
                        }
                    }
                }
            }
        }
        private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("LooseSprites/Cursors")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    if (Game1.activeClickableMenu is LevelUpMenu && !DisplayHandler.WasSkillMenuRaised.Value)
                    {
                        editor.PatchImage(DisplayHandler.ProfessionIcons, null, new(0, 704, 96, 144));
                    }

                    if (ModEntry.ModConfig.Value.ColorBlindnessChanges)
                        editor.PatchImage(DisplayHandler.SkillIcons, new(0, 18, 43, 9), new(129, 338, 43, 9));

                }, AssetEditPriority.Late);
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Strings/UI")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();

                    foreach (var item in ModEntry.Professions.Keys)
                    {
                        editor.Data.Add("LevelUp_ProfessionName_" + item, ModEntry.Helper.Translation.Get("Profession." + item + ".Name"));
                        editor.Data.Add("LevelUp_ProfessionDescription_" + item, ModEntry.Helper.Translation.Get("Profession." + item + ".Desc").ToString().Replace('_', ' '));
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Strings/1_6_Strings")) && ModEntry.ModConfig.Value.MasteryCaveChanges)
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string,string>();

                    editor.Data["MasteryCave"] = ModEntry.Helper.Translation.Get("Message.MasteryCave");

                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/Objects")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ObjectData>().Data;

                    HandleObjects(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/WildTrees")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, WildTreeData>().Data;
                    HandleWildTrees(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/FishPondData")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.GetData<List<FishPondData>>();
                    HandlFishPonds(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/Weapons")) && CoreUtility.CurrentPlayerHasProfession("Speedster"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, WeaponData>();

                    foreach (var item in editor.Data)
                        item.Value.Speed *= 2;
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/FarmAnimals")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, FarmAnimalData>().Data;

                    if (CoreUtility.CurrentPlayerHasProfession("Breeder"))
                    {
                        var list = from animal in editor
                                   where animal.Value.House is "Coop" or "Big Coop" or "Deluxe Coop"
                                   select animal.Key;
                        foreach (var animal in list)
                            editor[animal].SellPrice *= 2;
                    }
                    if (TalentUtility.AnyPlayerHasTalent("Farming_Breed_Like_Rabbits"))
                    {
                        editor["Rabbit"].CanGetPregnant = true;
                        editor["Rabbit"].BirthText = ModEntry.Helper.Translation.Get("Message.RabbitBirth");
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/Locations")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, LocationData>().Data;

                    HandleLocations(ref editor);
                });
            }
            /*if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Data/Buffs")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, BuffData>().Data;

                    foreach (var buffData in editor)
                    {
                        if (!buffData.Value.IsDebuff && CoreUtility.CurrentPlayerHasProfession(467867))
                        {
                            if (buffData.Value.Duration != -2)
                            {
                                buffData.Value.Duration *= 2;
                            }
                        }
                        else if (buffData.Value.IsDebuff && CoreUtility.CurrentPlayerHasProfession(467867))
                        {
                            if (buffData.Value.Duration != -2)
                            {
                                buffData.Value.Duration /= 2;
                            }
                        }
                    }
                });
            }*/

            /*if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["ItemSpritesheet"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\ItemIcons.png"), AssetLoadPriority.Exclusive);
            }*/

            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["InsiderInfo"]))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["SkillBars"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\skillbars.png"), AssetLoadPriority.Exclusive);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["ProfessionIcons"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\ProfessionIcons.png"), AssetLoadPriority.Exclusive);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["TalentBG"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\TalentBG.png"), AssetLoadPriority.Exclusive);
            }

            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["TalentSchema"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\TalentSchema.png"), AssetLoadPriority.Exclusive);
            }
            if (e.NameWithoutLocale.IsEquivalentTo(ContentPaths["BundleIcons"]))
            {
                e.LoadFromModFile<Texture2D>(PathUtilities.NormalizePath("assets\\BundleIcons.png"), AssetLoadPriority.Exclusive);
            }
        }
    }
}
