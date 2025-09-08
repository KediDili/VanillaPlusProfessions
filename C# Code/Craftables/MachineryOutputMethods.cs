﻿using StardewValley.GameData.Machines;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Extensions;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryOutputMethods
    {
        public static Item ProgrammableDrillOutput(StardewValley.Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            if (inputItem.HasContextTag("ore_item") || inputItem.Category == StardewValley.Object.GemCategory || inputItem.QualifiedItemId is "(O)382" or "(O)390" || (inputItem is StardewValley.Object obj && obj.HasContextTag("geode")))
            {
                int minStack = 0;
                int maxStack = 0;
                if (inputItem.HasContextTag("ore_item"))
                {
                    minStack = 35;
                    maxStack = 70;
                }
                else if (inputItem.Category == StardewValley.Object.GemCategory)
                {
                    minStack = 2;
                    maxStack = 10;
                }
                else if (inputItem is StardewValley.Object obj2 && obj2.HasContextTag("geode"))
                {
                    minStack = 10;
                    maxStack = 30;
                }
                else if (inputItem.QualifiedItemId is "(O)382")
                {
                    minStack = 100;
                    maxStack = 450;
                }
                else if (inputItem.QualifiedItemId is "(O)390")
                {
                    minStack = 300;
                    maxStack = 750;
                }
                return ItemRegistry.Create(inputItem.QualifiedItemId, Game1.random.Next(minStack, maxStack + 1), inputItem.Quality);
            }
            return null;
        }

        public static Item NodeMakerOutput(StardewValley.Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
        {
            overrideMinutesUntilReady = null;
            if (inputItem.HasContextTag("ore_item") || inputItem.Category == StardewValley.Object.GemCategory || inputItem.QualifiedItemId is "(O)382" or "(O)390" or "(O)881" || (inputItem is StardewValley.Object obj && obj.HasContextTag("geode")))
            {
                string nodeToCreate = null;
                if (ItemRegistry.GetData(inputItem.QualifiedItemId).RawData is ObjectData data)
                {
                    if (data.CustomFields?.TryGetValue(Constants.Key_NodeMaker_OreToNode, out string nodes) is true && !string.IsNullOrEmpty(nodes))
                    {
                        string[] strings = ArgUtility.SplitBySpace(nodes);
                        if (strings.Length > 0)
                        {
                            nodeToCreate = Game1.random.ChooseFrom(strings);
                        }
                    }
                }
                if (nodeToCreate is not null)
                {
                    return ItemRegistry.Create(nodeToCreate, Game1.random.Next(2, 6), inputItem.Quality);
                }
            }
            return null;
        }

        public static bool BirdFeederInteraction(Object machine, GameLocation location, Farmer player)
        {
            if (machine.heldObject.Value is null && (player.ActiveObject?.ItemId is "270" or "431" or "384" || player.ActiveObject?.Category is StardewValley.Object.FishCategory or StardewValley.Object.baitCategory))
            {
                machine.lastInputItem.Value = player.ActiveObject.getOne() as Object;
                machine.showNextIndex.Value = true;
                player.ActiveObject = player.ActiveObject.ConsumeStack(1) as Object;
            }

            return true;
        }
    }
}
