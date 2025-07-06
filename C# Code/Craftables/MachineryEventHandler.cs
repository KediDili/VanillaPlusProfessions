using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryEventHandler
    {
        // What? Did you never want to fix things without bothering others?
        internal const string Key_IsLavaLocation = "KediDili.VanillaPlusProfessions/IsLavaLocation"; 
        internal const string Key_IsConsistentMineLocation = "KediDili.VanillaPlusProfessions/IsConsistentMineLocation";
        internal const string Key_LastInput = "KediDili.VanillaPlusProfessions/LastInput";
        internal const string Key_NodeMakerData = "KediDili.VanillaPlusProfessions/NodeMakerData";
        internal const string Key_IsLavaLocation2 = "KediDili.VanillaPlusProfessions_IsLavaLocation";
        internal const string Key_IsConsistentMineLocation2 = "KediDili.VanillaPlusProfessions_IsConsistentMineLocation";
        internal const string Key_LastInput2 = "KediDili.VanillaPlusProfessions_LastInput";
        internal const string Key_NodeMakerData2 = "KediDili.VanillaPlusProfessions_NodeMakerData";

        internal static Dictionary<string, List<Vector2>> DrillLocations = new();
        internal static Dictionary<string, List<Vector2>> NodeMakerLocations = new();
        internal static Dictionary<string, List<Vector2>> ThermalReactorLocations = new();

        internal static Dictionary<string, List<Bird>> BirdsOnFeeders = new();

        internal static Building MineTent;

        public static void OnPlayerWarp()
        {
            if (BirdsOnFeeders.ContainsKey(Game1.player.currentLocation.Name))
            {
                List<Bird> sdsd = new();
                foreach (var item in Game1.player.currentLocation.Objects.Values)
                {
                    if (item.QualifiedItemId == "(BC)KediDili.VPPData.CP_BirdFeeder" && item.heldObject.Value is not null)
                    {
                        List<Vector2> v = new() { new(-1, 1), new(1, 1), new(1, -1), new(-1, -1), new(0, 1), new(0, 1), new(1, 0), new(-1, 0) };
                        for (int i = 0; i < Game1.random.Next(1, 4); i++)
                        {
                            Bird bird = new(new((int)(item.TileLocation.X - 0.5f), (int)(item.TileLocation.Y - 0.5f)), new(Game1.birdsSpriteSheet, Game1.random.Next(0, 4), 16, 16, item.TileLocation, Array.Empty<Point>(), Array.Empty<Point>()));
                            
                            var offset = Game1.random.ChooseFrom(v);
                            bird.position += offset * 64;
                            v.Remove(offset);
                            sdsd.Add(bird);
                        }
                    }
                }
                if (sdsd.Count > 0)
                {
                    BirdsOnFeeders[Game1.player.currentLocation.Name] = sdsd;
                }
            }
        }

        public static void OnWorldDrawn(SpriteBatch b)
        {
            if (BirdsOnFeeders.TryGetValue(Game1.player.currentLocation.Name, out var value))
            {
                for (int i = 0; i < value.Count; i++)
                {
                    value[i].Draw(b);
                }
            }
        }

        public static void OnTimeChanged(TimeChangedEventArgs e)
        {
            foreach (var item in DrillLocations)
            {
                var loc = Game1.getLocationFromName(item.Key);

                foreach (var item2 in item.Value) 
                {
                    Chest inputChest = new();
                    Chest batteryChest = new();
                    if (loc.Objects.TryGetValue(item2, out var obj) && obj?.QualifiedItemId == "(BC)KediDili.VPPData.CP_ProgrammableDrill" && IsThereAContainerNearby(obj, out List<Chest> container))
                    {
                        var validPool = (from @object in Game1.objectData
                                         where @object.Value.ContextTags?.Contains("ore_item") is true || @object.Key == "382" || @object.Key == "390" || @object.Value.Category == StardewValley.Object.GemCategory || @object.Value.GeodeDrops is not null || @object.Value.GeodeDropsDefaultItems
                                         select @object.Key).ToList();
                        foreach (var chest in container)
                        {
                            if (obj.MinutesUntilReady > 0)
                            {
                                foreach (var output in validPool)
                                {
                                    var extraOutput = ItemRegistry.Create("(O)" + output);
                                    if (Game1.random.NextBool(0.0005) && obj.heldObject.Value?.ItemId != output && Utility.canItemBeAddedToThisInventoryList(extraOutput, chest.Items, chest.GetActualCapacity()))
                                    {
                                        chest.Items.Add(extraOutput);
                                        ItemGrabMenu.organizeItemsInList(chest.Items);
                                        break;
                                    }
                                }
                            }
                            else if (obj.heldObject.Value is null)
                            {
                                if (chest.Items.ContainsId("(O)787"))
                                {
                                    batteryChest = chest;
                                }
                                foreach (var validItem in validPool)
                                {
                                    if (obj.modData.TryGetValue(Key_LastInput, out string value) && value is not null and "")
                                    {
                                        if (chest.Items.ContainsId(value))
                                        {
                                            inputChest = chest;
                                        }
                                        else
                                        {
                                            foreach (var containers in container)
                                            {
                                                if (containers.Items.ContainsId(value))
                                                {
                                                    inputChest = containers;
                                                }
                                            }
                                        }
                                        if (obj.OutputMachine(obj.GetMachineData(), obj.GetMachineData().OutputRules[0], ItemRegistry.Create("(O)" + value), Game1.MasterPlayer, loc, false))
                                        {
                                            inputChest.Items.ReduceId(value, 1);
                                            batteryChest.Items.ReduceId("(O)787", 1);
                                            ItemGrabMenu.organizeItemsInList(inputChest.Items);
                                            ItemGrabMenu.organizeItemsInList(batteryChest.Items);
                                            break;
                                        }
                                    }
                                    if (chest.Items.ContainsId(validItem))
                                    {
                                        inputChest = chest;
                                        if (obj.OutputMachine(obj.GetMachineData(), obj.GetMachineData().OutputRules[0], ItemRegistry.Create("(O)" + validItem), Game1.MasterPlayer, loc, false))
                                        {
                                            inputChest.Items.ReduceId(validItem, 1);
                                            batteryChest.Items.ReduceId("(O)787", 1);
                                            ItemGrabMenu.organizeItemsInList(inputChest.Items);
                                            ItemGrabMenu.organizeItemsInList(batteryChest.Items);
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (obj.heldObject.Value is not null)
                            {
                                if (Utility.canItemBeAddedToThisInventoryList(obj.heldObject.Value, chest.Items, chest.GetActualCapacity()))
                                {
                                    if (chest.Items.Count > 0 && chest.Items is not null && obj.heldObject.Value is not null)
                                    {
                                        foreach (var containerItem in chest.Items)
                                        {
                                            if (obj.heldObject.Value is not null && containerItem.canStackWith(obj.heldObject.Value))
                                            {
                                                obj.heldObject.Value.Stack = containerItem.addToStack(obj.heldObject.Value);
                                            }
                                        }
                                    }
                                    if (obj.modData.TryAdd(Key_LastInput, obj.heldObject.Value.QualifiedItemId))
                                    {
                                        obj.modData[Key_LastInput] = obj.heldObject.Value.QualifiedItemId;
                                    }
                                    if (obj.heldObject.Value.Stack is not 0)
                                    {
                                        chest.Items.Add(obj.heldObject.Value);
                                        ItemGrabMenu.organizeItemsInList(chest.Items);
                                        obj.readyForHarvest.Value = false;
                                        obj.heldObject.Value = null;
                                        break;
                                    }

                                    if (obj.heldObject.Value.Stack == 0)
                                    {
                                        ItemGrabMenu.organizeItemsInList(chest.Items);
                                        obj.readyForHarvest.Value = false;
                                        obj.heldObject.Value = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var item in ThermalReactorLocations)
            {
                var loc = Game1.getLocationFromName(item.Key);

                foreach (var item2 in item.Value)
                {
                    if (loc.Objects.TryGetValue(item2, out var obj) && obj?.QualifiedItemId == "(BC)KediDili.VPPData.CP_ThermalReactor" && IsThereAContainerNearby(obj, out List<Chest> container))
                    {
                        foreach (var chest in container)
                        {
                            if (obj.MinutesUntilReady == 0 && obj.heldObject.Value is not null && Utility.canItemBeAddedToThisInventoryList(obj.heldObject.Value, chest.Items, chest.GetActualCapacity()))
                            {
                                if (chest.Items.Count > 0)
                                {
                                    foreach (var containerItem in chest.Items)
                                    {
                                        if (containerItem.canStackWith(obj.heldObject.Value))
                                        {
                                            obj.heldObject.Value.Stack = containerItem.addToStack(obj.heldObject.Value);
                                        }
                                    }
                                }
                                if (obj.heldObject.Value.Stack is not 0)
                                {
                                    chest.Items.Add(obj.heldObject.Value);
                                    obj.heldObject.Value = null;
                                    obj.readyForHarvest.Value = false;
                                    obj.OutputMachine(obj.GetMachineData(), obj.GetMachineData().OutputRules[0], null, Game1.MasterPlayer, loc, false);
                                    break;
                                }

                                if (obj.heldObject.Value.Stack == 0)
                                {
                                    obj.heldObject.Value = null;
                                    obj.readyForHarvest.Value = false;
                                    obj.OutputMachine(obj.GetMachineData(), obj.GetMachineData().OutputRules[0], null, Game1.MasterPlayer, loc, false);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            foreach (var item in NodeMakerLocations)
            {
                var loc = Game1.getLocationFromName(item.Key);

                foreach (var item2 in item.Value)
                {
                    if (loc.Objects.TryGetValue(item2, out var obj) && obj is not null)
                    {
                        if (obj.QualifiedItemId == "(BC)KediDili.VPPData.CP_NodeMaker" && IsThereAContainerNearby(obj, out List<Chest> container))
                        {
                            foreach (var chest in container)
                            {
                                ModEntry.Helper.Reflection.GetField<IInventory>(typeof(StardewValley.Object), "autoLoadFrom", true).SetValue(chest.Items);

                                if (obj.heldObject.Value is null)
                                {
                                    if (obj.AttemptAutoLoad(chest.Items, Game1.player))
                                    {
                                        break;
                                    }
                                }
                                else if (obj.heldObject.Value is not null && obj.readyForHarvest.Value)
                                {
                                    if (Utility.canItemBeAddedToThisInventoryList(obj.heldObject.Value, chest.Items, chest.GetActualCapacity()))
                                    {
                                        if (chest.Items.Count > 0)
                                        {
                                            foreach (var containerItem in chest.Items)
                                            {
                                                if (containerItem.canStackWith(obj.heldObject.Value))
                                                {
                                                    obj.heldObject.Value.Stack = containerItem.addToStack(obj.heldObject.Value);
                                                }
                                            }
                                        }

                                        if (obj.heldObject.Value.Stack is not 0)
                                        {
                                            chest.Items.Add(obj.heldObject.Value);
                                            obj.readyForHarvest.Value = false;
                                            obj.heldObject.Value = null;
                                            break;
                                        }

                                        if (obj.heldObject.Value.Stack == 0)
                                        {
                                            obj.readyForHarvest.Value = false;
                                            obj.heldObject.Value = null;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void OnMenuChanged(MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu menu2)
            {
                if (menu2.context is Building mineTent && mineTent.buildingType.Value == "KediDili.VPPData.CP_MineTent" && mineTent.GetBuildingChest("Default_Chest") is not null and Chest buildingChest)
                {
                    //Should I make it in initial creation of the building? - Nah, looks like it works
                    buildingChest.SpecialChestType = Chest.SpecialChestTypes.BigChest;
                    ItemGrabMenu newItemGrabMenu = new(buildingChest.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, buildingChest.grabItemFromInventory, null, buildingChest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, buildingChest, -1, buildingChest)
                    {
                        chestColorPicker = null,
                        colorPickerToggleButton = null,
                        discreteColorPickerCC = null
                    };
                    Game1.activeClickableMenu = newItemGrabMenu;
                }
                else if (menu2.context is Chest chest && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_MinecartChest")
                {
                    menu2.exitFunction = OnMenuExit;
                }
            }

            else if (e.OldMenu is ItemGrabMenu menu && menu.context is Chest chest && (chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_MinecartChest" || chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector"))
            {
                chest.fixLidFrame();
                if (chest.Items.Count > 0 && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_MinecartChest")
                {
                    OnMenuExit();
                }
            }
        }
        public static void OnMenuExit()
        {
            if (MineTent is null || Context.IsMultiplayer)
            {
                Utility.ForEachBuilding(building => ShouldKeepSearching(building), true);
                Game1.playSound("wand");
            }
            else if (!ShouldKeepSearching(MineTent))
            {
                Game1.playSound("wand");
            }
        }

        public static bool IsThereAContainerNearby(StardewValley.Object drill, out List<Chest> container, bool hasToBeNotFull = false)
        {
            container = new();
            if (drill is null)
            {
                return false;
            }
            if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X + 1, drill.TileLocation.Y), out var chest) && chest is not null && chest is Chest chst && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector" && (!hasToBeNotFull || chst.Items.CountItemStacks() < chst.GetActualCapacity()))
            {
                container.Add(chst);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X - 1, drill.TileLocation.Y), out chest) && chest is not null && chest is Chest chst2 && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector" && (!hasToBeNotFull || chst2.Items.CountItemStacks() < chst2.GetActualCapacity()))
            {
                container.Add(chst2);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X, drill.TileLocation.Y + 1), out chest) && chest is not null && chest is Chest chst3 && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector" && (!hasToBeNotFull || chst3.Items.CountItemStacks() < chst3.GetActualCapacity()))
            {
                container.Add(chst3);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X, drill.TileLocation.Y - 1), out chest) && chest is not null && chest is Chest chst4 && chest.QualifiedItemId == "(BC)KediDili.VPPData.CP_DrillCollector" && (!hasToBeNotFull || chst4.Items.CountItemStacks() < chst4.GetActualCapacity()))
            {
                container.Add(chst4);
            }
            return container.Count > 0;
        }
        public static bool ShouldKeepSearching(Building building)
        {
            if (Game1.activeClickableMenu is not null)
                return false;
            if (building.buildingType.Value == "KediDili.VPPData.CP_MineTent")
            {
                Chest defaultChest = (MineTent ?? building).GetBuildingChest("Default_Chest");
                foreach (var objs in Game1.player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryId_Minecarts))
                {
                    if (objs is null)
                        continue;
                    if (Utility.canItemBeAddedToThisInventoryList(objs.getOne(), defaultChest.Items, defaultChest.GetActualCapacity()) && objs is not Tool)
                    {
                        if (defaultChest.Items.Count > 0)
                        {
                            foreach (var item in defaultChest.Items)
                            {
                                if (item.canStackWith(objs))
                                {
                                    objs.Stack = item.addToStack(objs);
                                }
                            }
                        }
                        if (objs.Stack is not 0)
                        {
                            defaultChest.Items.Add(objs);
                            Game1.player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryId_Minecarts).RemoveButKeepEmptySlot(objs);
                        }
                        
                        if (objs.Stack == 0)
                        {
                            Game1.player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryId_Minecarts).RemoveButKeepEmptySlot(objs);
                        }
                    }
                }
                Game1.player.team.GetOrCreateGlobalInventoryMutex(ModEntry.GlobalInventoryId_Minecarts).Update(building.GetParentLocation());
                MineTent = building;
                return false;
            }
            return true;
        }

        public static void OnMachineInteract(StardewValley.Object machine, Farmer who)
        {
            if (machine.QualifiedItemId == "(BC)KediDili.VPPData.CP_MinecartChest")
            {
                
            }
        }
    }
}
