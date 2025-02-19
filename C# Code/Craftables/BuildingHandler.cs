using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.TerrainFeatures;

namespace VanillaPlusProfessions.Craftables
{
    public class BuildingHandler
    {
        internal const string Key_ResourceClumpName = "KediDili.VanillaPlusProfessions/ResourceClumpName";

        public static void OnDayStarted(Building building)
        {
            if (building.buildingType.Value == "KediDili.VPPData.CP_MineralCavern")
            {
                var interior = building.GetIndoors();

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

                                    if (!(dataOther is ObjectData objDataOther && dataThis is ObjectData objDataThis && objDataOther.CustomFields?.TryGetValue(Key_ResourceClumpName, out string value2) is true && objDataThis.CustomFields?.TryGetValue(Key_ResourceClumpName, out string value3) is true && value2 == value3))
                                    {
                                        if (!ContentEditor.ResourceClumpData.TryGetValue(item.Value.ItemId, out ClumpData clumpData))
                                        {
                                            isValid = false;
                                            clumpToSpawn = "";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        clumpToSpawn = value2;
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
                        }
                        if (isValid && Game1.random.NextBool(0.3))
                        {
                            for (int y = 0; y < 2; y++)
                            {
                                for (int x = 0; x < 2; x++)
                                {
                                    Vector2 v = new(x + item.Value.TileLocation.X, y + item.Value.TileLocation.Y);
                                    interior.Objects.Remove(v);
                                }
                            }
                            bool found = false;
                            foreach (var value in ContentEditor.NodeMakerData.Values)
                            {
                                if (value.Contains(item.Value.ItemId) && ContentEditor.ResourceClumpData.TryGetValue(item.Value.ItemId, out ClumpData clumpData))
                                {
                                    found = true;
                                    interior.resourceClumps.Add(new ResourceClump(clumpData.Id, 2, 2, item.Value.TileLocation));
                                }
                            }
                            if (!found && ModEntry.ItemExtensionsAPI.Value is not null && !string.IsNullOrEmpty(clumpToSpawn))
                            {
                                ModEntry.ItemExtensionsAPI.Value.TrySpawnClump(clumpToSpawn, item.Value.TileLocation, interior, out string error, true);
                                if (error is not null and "")
                                {
                                    ModEntry.ModMonitor.Log(error, StardewModdingAPI.LogLevel.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<Vector2> GetSurroundingTiles(Vector2 position)
        {
            return new List<Vector2>()
            {
                new(position.X - 1, position.Y - 1), new(position.X, position.Y - 1), new(position.X + 1, position.Y - 1),
                new(position.X - 1, position.Y), /*------ original position -------*/ new(position.X + 1, position.Y),
                new(position.X - 1, position.Y + 1), new(position.X, position.Y + 1), new(position.X + 1, position.Y + 1),
            };
        }
    }
}
