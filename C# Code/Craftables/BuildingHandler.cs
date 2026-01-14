using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VanillaPlusProfessions.Utilities;
namespace VanillaPlusProfessions.Craftables
{
    public class BuildingHandler
    {
        public static void OnDayStarted(Building building)
        {
            var interior = building.GetIndoors();
            if (building.buildingType.Value == Constants.Id_MineralCavern)
            {
                foreach (var item in interior.Objects.Pairs)
                {
                    bool isValid = true;
                    string clumpToSpawn = "";
                    if (item.Value.Category == StardewValley.Object.litterCategory)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                if (interior.Objects.TryGetValue(new(item.Value.TileLocation.X + x, item.Value.TileLocation.Y + y), out var obj) && obj.Category == StardewValley.Object.litterCategory)
                                {
                                    var dataOther = ItemRegistry.GetData(obj.QualifiedItemId).RawData;
                                    var dataThis = ItemRegistry.GetData(item.Value.QualifiedItemId).RawData;

                                    if (!(dataOther is ObjectData objDataOther && dataThis is ObjectData objDataThis && objDataOther.CustomFields?.TryGetValue(Constants.Key_MineralCavern_NodeToBoulder, out string value2) is true && objDataThis.CustomFields?.TryGetValue(Constants.Key_MineralCavern_NodeToBoulder, out string value3) is true && value2 == value3))
                                    {
                                        isValid = false;
                                        clumpToSpawn = "";
                                        break;
                                    }
                                    else
                                    {
                                        clumpToSpawn = Game1.random.ChooseFrom(value2.Split(' '));
                                        isValid = true;
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }
                            }
                            if (!isValid)
                            { 
                                break; 
                            }
                        }//Game1.random.NextBool(0.3) &&
                        if ( isValid && !string.IsNullOrEmpty(clumpToSpawn))
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                for (int x = 0; x < 2; x++)
                                {
                                    Vector2 v = new(x + item.Value.TileLocation.X, y + item.Value.TileLocation.Y);
                                    interior.Objects.Remove(v);
                                }
                            }

                            if (int.TryParse(clumpToSpawn, out int result))
                            {
                                if (!interior.modData.TryAdd(Constants.Key_ClumpSaveName, $"{result}/{item.Value.TileLocation.X}+{item.Value.TileLocation.Y}"))
                                {
                                    interior.modData[Constants.Key_ClumpSaveName] += $"||{result}/{item.Value.TileLocation.X.ToString()}+{item.Value.TileLocation.Y}";
                                }
                                var clump = new ResourceClump(result, 2, 2, item.Value.TileLocation);
                                clump.modData.Add(Constants.Key_ParsedClumpType, result.ToString());
                                interior.resourceClumps.Add(clump);
                                //break;
                            }
                            else if (ModEntry.CoreModEntry.Value.ItemExtensionsAPI is not null)
                            {
                                if (!interior.modData.TryAdd(Constants.Key_ClumpSaveName, $"{clumpToSpawn}/{item.Value.TileLocation.X}+{item.Value.TileLocation.Y}"))
                                {
                                    interior.modData[Constants.Key_ClumpSaveName] += $"||{clumpToSpawn}/{item.Value.TileLocation.X}+{item.Value.TileLocation.Y}";
                                }
                                ModEntry.CoreModEntry.Value.ItemExtensionsAPI.TrySpawnClump(clumpToSpawn, item.Value.TileLocation, interior, out string error, true);
                                if (!string.IsNullOrEmpty(error))
                                {
                                    ModEntry.CoreModEntry.Value.ModMonitor.Log(error, StardewModdingAPI.LogLevel.Error);
                                }
                                foreach (var clump in interior.resourceClumps)
                                {
                                    //imperfect, but could work
                                    if (clump.Tile == item.Value.TileLocation)
                                    {
                                        clump.modData.Add(Constants.Key_ParsedClumpType, clumpToSpawn);
                                    }
                                }                                
                                break;
                            }
                        }
                    }
                }
            }
            else if (building.buildingType.Value == Constants.Id_SecretGlade)
            {
                var validForages = (from obj in TalentUtility.FilterObjectData(new() { StardewValley.Object.GreensCategory, StardewValley.Object.flowersCategory, StardewValley.Object.FruitsCategory, StardewValley.Object.VegetableCategory }, excludeTags: new() { "forage_item_desert", "forage_item_beach" })
                                    where TalentUtility.EligibleForForagePerks(obj.Key, Constants.Id_SecretGlade)
                                    select obj.Key).ToList();

                if (validForages?.Any() is true)
                {
                    for (int x = 0; x < interior.map.DisplayWidth / 64; x++)
                    {
                        for (int y = 0; y < interior.map.DisplayHeight / 64; y++)
                        {
                            Vector2 tile = new(x, y);
                            if (!Game1.random.NextBool(0.15) || interior.Objects.ContainsKey(tile) || interior.doesTileHaveProperty(x, y, "Spawnable", "Back") == null || interior.doesEitherTileOrTileIndexPropertyEqual(x, y, "Spawnable", "Back", "F") || !interior.CanItemBePlacedHere(tile) || interior.hasTileAt(x, y, "Front") || interior.isBehindBush(tile) || (!Game1.random.NextBool(0.1) && interior.isBehindTree(tile)))
                            {
                                continue;
                            }
                            var obj = ItemRegistry.Create<Object>(Game1.random.ChooseFrom(validForages));
                            obj.IsSpawnedObject = true;
                            interior.Objects.Add(tile, obj);
                        }
                    }
                }
            }
        }
        public static void OnDayEnding()
        {
            Utility.ForEachBuilding(building =>
            {
                if (building.buildingType.Value == Constants.Id_MineralCavern && building.GetIndoors() is GameLocation interior)
                {
                    if (interior.modData.TryGetValue(Constants.Key_ClumpSaveName, out string value))
                    {
                        StringBuilder stringBuilder = new();
                        foreach (var item in interior.resourceClumps)
                        {
                            stringBuilder.Append($"{item.modData[Constants.Key_ParsedClumpType]}/{item.Tile.X}+{item.Tile.Y}||");
                        }
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Replace("||", "", stringBuilder.Length - 2, 2);
                        }
                        if (value != stringBuilder.ToString())
                        {
                            interior.modData[Constants.Key_ClumpSaveName] = stringBuilder.ToString();
                        }
                    }
                }
                return true;
            }
            );
        }
        public static void OnSaveLoaded()
        {
            Utility.ForEachBuilding(building =>
            {
                if (building.buildingType.Value == Constants.Id_MineralCavern && building.GetIndoors() is GameLocation interior && interior.modData.TryGetValue(Constants.Key_ClumpSaveName, out string value))
                {
                    string[] BouldersAndTiles = value.Split("||");
                    if (BouldersAndTiles.Length == 1 && string.IsNullOrEmpty(BouldersAndTiles[0]))
                    {
                        return true;
                    }
                    foreach (var item in BouldersAndTiles)
                    {
                        string[] Seperated = item.Split("/");
                        string[] X_Y = Seperated[1].Split('+');
                        Vector2 tile = new(float.Parse(X_Y[0]), float.Parse(X_Y[1]));

                        if (int.TryParse(Seperated[0], out int result))
                        {
                            interior.resourceClumps.Add(new(result, 2, 2, tile));
                        }
                        else if(ModEntry.CoreModEntry.Value.ItemExtensionsAPI is not null)
                        {
                            ModEntry.CoreModEntry.Value.ItemExtensionsAPI.TrySpawnClump(Seperated[0], tile, interior, out string error, true);
                            if (error is not null)
                            {
                                ModEntry.CoreModEntry.Value.ModMonitor.Log(error, StardewModdingAPI.LogLevel.Error);
                            }
                        }
                    }
                }
                return true;
            }
            );
        }
    }
}
