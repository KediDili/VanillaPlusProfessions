using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
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

namespace VanillaPlusProfessions
{
    //Class used to manage professions that are data related.
    //Classes under VanillaPlusProfessions.Skills namespace are used to manage professions that are not doable without Harmony (or I have a skill issue, which is more likely)
    //ModEntry and Patches classes contain things that are generic to all professions, both data and code wise, and professions that don't require Harmony.
    internal static class ContentEditor
    {
        private static List<WildTreeItemData> ShakerData = new();
        private static Dictionary<string, bool?> RangerAdventurerData = new();
        private static List<MachineOutputRule> ArcheologistData = new();
        private static List<MachineItemOutput> RecyclerData = new();
        private static List<SpawnForageData> GleanerData = new();
        private static List<SpawnForageData> WayfarerData = new();
        private static Dictionary<string, string> CrafterData = new();
        private static Dictionary<string, ObjectData> CustomObjectData = new();
        internal static Dictionary<string, string> BuccaneerData = new();
        internal static void Initialize()
        {
            ModEntry.Helper.Events.Content.AssetRequested += OnAssetRequested;
            ModEntry.Helper.Events.Content.AssetRequested += OnAssetRequested_LowPriority;
            ShakerData = ModEntry.Helper.ModContent.Load<List<WildTreeItemData>>("assets\\ShakerData.json");
            RangerAdventurerData = ModEntry.Helper.ModContent.Load<Dictionary<string, bool?>>("assets\\RangerAdventurerData.json");
            ArcheologistData = ModEntry.Helper.ModContent.Load<List<MachineOutputRule>>("assets\\ArcheologistData.json");
            RecyclerData = ModEntry.Helper.ModContent.Load<List<MachineItemOutput>>("assets\\RecyclerData.json");
            GleanerData = ModEntry.Helper.ModContent.Load<List<SpawnForageData>>("assets\\GleanerData.json");
            WayfarerData = ModEntry.Helper.ModContent.Load<List<SpawnForageData>>("assets\\WayfarerData.json");
            CrafterData = ModEntry.Helper.ModContent.Load<Dictionary<string, string>>("assets\\CrafterData.json");
            CustomObjectData = ModEntry.Helper.ModContent.Load<Dictionary<string, ObjectData>>("assets\\CustomObjectData.json");
            BuccaneerData = ModEntry.Helper.ModContent.Load<Dictionary<string, string>>("assets\\BuccaneerData.json");
        }
        internal static void HandleRecycleMachine(ref IDictionary<string, MachineData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(44)) //Recycling Machine +/+/+/+/+
            {
                for (int i = 0; i < editor["(BC)20"].OutputRules.Count; i++)
                {
                    switch (editor["(BC)20"].OutputRules[i].Id)
                    {
                        case "Default_Trash":
                            editor["(BC)20"].OutputRules[i].UseFirstValidOutput = false;
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[0]);
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[1]);
                            break;
                        case "Default_Driftwood":
                            editor["(BC)20"].OutputRules[i].UseFirstValidOutput = false;
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[2]);
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[3]);
                            break;
                        case "Default_SoggyNewspaper":
                            editor["(BC)20"].OutputRules[i].UseFirstValidOutput = false;
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[4]);
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[5]);
                            break;
                        case "Default_BrokenCd":
                            editor["(BC)20"].OutputRules[i].UseFirstValidOutput = false;
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[6]);
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[7]);
                            break;
                        case "Default_BrokenGlasses":
                            editor["(BC)20"].OutputRules[i].UseFirstValidOutput = false;
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[8]);
                            editor["(BC)20"].OutputRules[i].OutputItem.Add(RecyclerData[9]);
                            break;
                    }
                }
            }
            if (CoreUtility.AnyPlayerHasProfession(58)) //Recycling Machine
            {
                editor["(BC)20"].OutputRules.AddRange(ArcheologistData);
            }
        }
        internal static void HandleFurnaces(ref IDictionary<string, MachineData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(54)) //Furnace +
            {
                //Furnaces require less resources. +
                editor["(BC)13"].OutputRules[0].Triggers[0].RequiredCount = 3;
                editor["(BC)13"].OutputRules[1].Triggers[0].RequiredCount = 3;
                editor["(BC)13"].OutputRules[2].Triggers[0].RequiredCount = 3;
                editor["(BC)13"].OutputRules[3].Triggers[0].RequiredCount = 3;
                editor["(BC)13"].OutputRules[7].Triggers[0].RequiredCount = 3;

                if (Game1.versionLabel.Contains("beta"))
                {
                    editor["(BC)HeavyFurnace"].OutputRules[0].Triggers[0].RequiredCount = 20;
                    editor["(BC)HeavyFurnace"].OutputRules[1].Triggers[0].RequiredCount = 20;
                    editor["(BC)HeavyFurnace"].OutputRules[2].Triggers[0].RequiredCount = 20;
                    editor["(BC)HeavyFurnace"].OutputRules[3].Triggers[0].RequiredCount = 20;
                    editor["(BC)HeavyFurnace"].OutputRules[4].Triggers[0].RequiredCount = 4;
                    editor["(BC)HeavyFurnace"].OutputRules[5].Triggers[0].RequiredCount = 4;
                    editor["(BC)HeavyFurnace"].OutputRules[7].Triggers[0].RequiredCount = 20;
                }
            }
            if (CoreUtility.AnyPlayerHasProfession(56)) //Furnace +
            {
                for (int i = 0; i < 8; i++)
                {
                    editor["(BC)13"].OutputRules[i].MinutesUntilReady *= 3 / 4;
                    if (Game1.versionLabel.Contains("beta"))
                    {
                        editor["(BC)HeavyFurnace"].OutputRules[i].MinutesUntilReady *= 3 / 4;
                    }
                }
            }
        }
        internal static void HandleCropBasedMachinery(ref IDictionary<string, MachineData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(34)) //Preserves Jar, Keg, Oil maker, Cask
            {
                //12 keg
                for (int i = 0; i < editor["(BC)15"].OutputRules.Count; i++)
                    if (!editor["(BC)15"].OutputRules[i].Id.Contains("Roe")) //Preserves Jar +
                        editor["(BC)15"].OutputRules[i].MinutesUntilReady *= 3 / 4;

                for (int i = 0; i < editor["(BC)12"].OutputRules.Count; i++) //Keg +
                    editor["(BC)12"].OutputRules[i].MinutesUntilReady *= 3 / 4;

                for (int i = 0; i < editor["(BC)19"].OutputRules.Count; i++)
                    if (!editor["(BC)19"].OutputRules[i].Id.Contains("Truffle")) //Oil maker +
                        editor["(BC)19"].OutputRules[i].MinutesUntilReady *= 3 / 4;
            }
        }
        internal static void HandleAnimalProductMachinery(ref IDictionary<string, MachineData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(30)) //Mayonnaise Machine/Loom ++
            {
                for (int i = 0; i < editor["(BC)17"].OutputRules.Count; i++)
                    editor["(BC)17"].OutputRules[i].OutputItem[0].MaxStack++;

                for (int i = 0; i < editor["(BC)24"].OutputRules.Count; i++)
                    if (!editor["(BC)24"].OutputRules[i].Id.Contains("Ostrich"))
                        editor["(BC)24"].OutputRules[i].OutputItem[0].MaxStack++;
            }
            if (CoreUtility.AnyPlayerHasProfession(32)) //Cheese Press/Loom + + +
            {
                if (Game1.player?.professions.Contains(30) is false)
                    for (int i = 0; i < editor["(BC)17"].OutputRules.Count; i++)
                        editor["(BC)17"].OutputRules[i].MinutesUntilReady *= 3 / 4;

                for (int i = 0; i < editor["(BC)16"].OutputRules.Count; i++)
                    editor["(BC)16"].OutputRules[i].MinutesUntilReady *= 3 / 4;

                for (int i = 0; i < editor["(BC)24"].OutputRules.Count; i++)
                {
                    if (editor["(BC)24"].OutputRules[i].Id.Contains("Ostrich"))
                    {
                        editor["(BC)24"].OutputRules[i].MinutesUntilReady *= 3 / 4;
                        break;
                    }
                }

                //17 loom
                //16 cheese press
                //24 mayonnaise machine -- only ostrich eggs
            }
        }
        internal static void HandleWildTrees(ref IDictionary<string, WildTreeData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(47) is true) //Shaker ++
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
            if (CoreUtility.AnyPlayerHasProfession(46) is true) //Arborist ++
                foreach (var item in editor)
                    if (item.Value.GrowthChance * 3 / 2 <= 1)
                        item.Value.GrowthChance *= 3 / 2;
        }
        internal static void HandleObjects(ref IDictionary<string, ObjectData> editor)
        {
            if (CoreUtility.CurrentPlayerHasProfession(50)) //Ranger ++
            {
                foreach (var item in RangerAdventurerData)
                    if (item.Value is not false)
                        editor[item.Key].Price *= 2;
            }
            if (CoreUtility.CurrentPlayerHasProfession(51)) //Adventurer ++
            {
                foreach (var item in RangerAdventurerData)
                    if (item.Value is true)
                        editor[item.Key].Price *= 2;
            }
            if (CoreUtility.CurrentPlayerHasProfession(55)) //Ironmonger ++
            {
                var ores = from KeyValuePair<string, ObjectData> item in editor
                           where item.Value.Name.ToLower().EndsWith("ore")
                           select item;
                foreach (var ore in ores)
                    editor[ore.Key].Price *= 2;
            }
            if (CoreUtility.CurrentPlayerHasProfession(59)) //Mineralogist
            {
                editor["749"].GeodeDropsDefaultItems = false;
                editor["537"].GeodeDropsDefaultItems = false;
                editor["536"].GeodeDropsDefaultItems = false;
                editor["535"].GeodeDropsDefaultItems = false;
                editor["537"].GeodeDrops = editor["749"].GeodeDrops;
                editor["536"].GeodeDrops = editor["749"].GeodeDrops;
                editor["535"].GeodeDrops = editor["749"].GeodeDrops;
            }
            CustomObjectData["Kedi.SMP.FruitSyrup"].Description = ModEntry.Helper.Translation.Get("Item.Syrup.Description");
            CustomObjectData["Kedi.SMP.GemDust"].Description = ModEntry.Helper.Translation.Get("Item.Dust.Description");

            CustomObjectData["Kedi.SMP.FruitSyrup"].DisplayName = ModEntry.Helper.Translation.Get("Item.Syrup.DisplayName");
            CustomObjectData["Kedi.SMP.GemDust"].DisplayName = ModEntry.Helper.Translation.Get("Item.Dust.DisplayName");

            foreach (var item in CustomObjectData)
            {
                editor.Add(item.Key, item.Value);
            }
        }
        internal static void HandleLocations(ref IDictionary<string, LocationData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(52)) //+
            {
                editor["Forest"].Forage.AddRange(GleanerData);
                editor["Backwoods"].Forage.AddRange(GleanerData);
                editor["Mountain"].Forage.AddRange(GleanerData);
            }
            if (CoreUtility.AnyPlayerHasProfession(53)) //+
            {
                editor["Forest"].Forage.AddRange(WayfarerData);
                editor["Backwoods"].Forage.AddRange(WayfarerData);
                editor["Mountain"].Forage.AddRange(WayfarerData);
            }
        }
        internal static void HandleCraftingRecipes(ref IDictionary<string,string> editor)
        {
            if (CoreUtility.CurrentPlayerHasProfession(57)) //Crafter ++
            {
                foreach (var item in CrafterData)
                    editor[item.Key] = item.Value;
            }
        }
        internal static void HandlFishPonds(ref List<FishPondData> editor)
        {
            if (CoreUtility.AnyPlayerHasProfession(42)) //
            {
                for (int i = 0; i < editor.Count; i++)
                {
                    for (int f = 0; f < editor[i].ProducedItems.Count; f++)
                    {
                        if (editor[i].ProducedItems[f].ItemId.Contains("812"))
                        {
                            editor[i].ProducedItems[f].MinQuantity *= 2;
                            editor[i].ProducedItems[f].MaxQuantity *= 2;
                        }
                    }
                }
            }
        }
        [EventPriority(EventPriority.Low - 1)]
        private static void OnAssetRequested_LowPriority(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    if (Game1.activeClickableMenu is LevelUpMenu)
                        editor.PatchImage(DisplayHandler.ProfessionIcons, null, new(0, 704, 96, 144));

                    if (ModEntry.ModConfig.Value.ColorBlindnessChanges)
                        editor.PatchImage(DisplayHandler.SkillIcons, new(0, 18, 43, 9), new(129, 338, 43, 9));

                }, AssetEditPriority.Late);
            }
        }
        [EventPriority(EventPriority.High + 1)]
        private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets\\Kedi.SMP.Items"))
            {
                e.LoadFromModFile<Texture2D>("assets\\ItemIcons.png", AssetLoadPriority.Exclusive);
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Strings/UI"))
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
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, ObjectData>().Data;

                    HandleObjects(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, MachineData>().Data;

                    HandleAnimalProductMachinery(ref editor);
                    HandleCropBasedMachinery(ref editor);
                    HandleFurnaces(ref editor);
                    HandleRecycleMachine(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/WildTrees"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, WildTreeData>().Data;
                    HandleWildTrees(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/FishPondData"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.GetData<List<FishPondData>>();
                    HandlFishPonds(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Weapons") && CoreUtility.CurrentPlayerHasProfession(67))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, WeaponData>();

                    foreach (var item in editor.Data)
                        item.Value.Speed *= 2;
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/FarmAnimals") && CoreUtility.CurrentPlayerHasProfession(31))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, FarmAnimalData>();

                    foreach (var animal in editor.Data)
                        if (animal.Value.House.EndsWith("Coop"))
                            animal.Value.SellPrice *= 2;
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>().Data;

                    HandleCraftingRecipes(ref editor);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, LocationData>().Data;

                    HandleLocations(ref editor);
                });
            }
        }
    }
}
