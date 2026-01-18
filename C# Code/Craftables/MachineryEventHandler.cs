using System;
using System.Collections.Generic;
using System.Linq;
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
using StardewModdingAPI.Utilities;

namespace VanillaPlusProfessions.Craftables
{
    public class MachineryEventHandler
    {
        internal static MachineryEventHandler ThisIsMe = new();

        internal static Dictionary<string, List<Vector2>> DrillLocations = new();
        internal static Dictionary<string, List<Vector2>> NodeMakerLocations = new();
        internal static Dictionary<string, List<Vector2>> ThermalReactorLocations = new();

        internal static Dictionary<string, List<Critter>> BirdsOnFeeders = new();

        internal static Building MineTent;

        public void OnPlayerWarp()
        {
            if (Context.IsMainPlayer && BirdsOnFeeders?.ContainsKey(Game1.player.currentLocation.NameOrUniqueName) is false && !Game1.player.currentLocation.IsRainingHere() && !Game1.player.currentLocation.IsGreenRainingHere())
            {
                List<Critter> sdsd = new();
                foreach (var item in Game1.player.currentLocation.Objects.Values)
                {
                    if (item.ItemId == Constants.Id_BirdFeeder && item.lastInputItem.Value is not null)
                    {
                        string feedID = item.lastInputItem.Value.ItemId;
                        List<Vector2> v = new() { new(-1, 1), new(1, 1), new(1, -1), new(-1, -1), new(0, 1), new(0, 1), new(1, 0), new(-1, 0) };
                        for (int i = 0; i < Game1.random.Next(1, 4); i++)
                        {
                            Critter bird = feedID switch
                            {
                                "270" => new Crow((int)item.TileLocation.X, (int)item.TileLocation.Y),
                                "431" => new Parrot(item.TileLocation * 64, false),
                                "384" => new Parrot(item.TileLocation * 64, true),
                                _ => null
                            };
                            Season season = Game1.player.currentLocation.GetSeason();
                            int whichBird = ((season == Season.Fall) ? 45 : 25);
                            if (Game1.random.NextBool() && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
                            {
                                whichBird = ((season == Season.Fall) ? 135 : 125);
                            }
                            if (whichBird == 25 && Game1.random.NextDouble() < 0.05)
                            {
                                whichBird = 165;
                            }
                            bird ??= item.lastInputItem.Value.Category switch
                            {
                                StardewValley.Object.FishCategory => new Seagull(item.TileLocation * 64, 0),
                                StardewValley.Object.baitCategory => new Birdie(item.TileLocation * 64, 0, whichBird),
                                _ => null
                            };
                            if (bird is null)
                                break;

                            var offset = Game1.random.ChooseFrom(v);
                            bird.position.X += offset.X * 64;
                            bird.position.Y += offset.Y * 64;
                            if (Game1.player.currentLocation.CanItemBePlacedHere(new Vector2(item.TileLocation.X + offset.X, item.TileLocation.Y + offset.Y)))
                            {
                                v.Remove(offset);
                                sdsd.Add(bird);
                            }
                        }
                    }
                }
                if (sdsd.Count > 0 && BirdsOnFeeders is not null)
                {
                    BirdsOnFeeders[Game1.player.currentLocation.NameOrUniqueName] = sdsd;
                    if (Context.HasRemotePlayers)
                    {
                        ModEntry.CoreModEntry.Value.Helper.Multiplayer.SendMessage(BirdsOnFeeders[Game1.player.currentLocation.NameOrUniqueName], "KediDili.VanillaPlusProfessions/BirdFeederData", new string[] { "KediDili.VanillaPlusProfessions" });
                    }
                }
            }
        }

        public void OnWorldDrawn(SpriteBatch b)
        {
            if (Context.IsWorldReady)
            {
                if (BirdsOnFeeders?.TryGetValue(Game1.player.currentLocation.NameOrUniqueName, out var value) is true)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        value[i].draw(b);
                    }
                }
            }
        }

        public void OnTimeChanged(TimeChangedEventArgs e)
        {
            Utility.ForEachItem(item =>
            {
                if (item is StardewValley.Object obj)
                {
                    if (obj.Location is not null && Game1.isTimeToTurnOffLighting(obj.Location))
                    {
                        if (obj.ItemId == Constants.Id_GlowingCrystal && obj.modData.ContainsKey(Constants.Key_GlowingCrystalColor))
                        {
                            string[] colorcodes = obj.modData[Constants.Key_GlowingCrystalColor].Split(',');
                            obj.lightSource.color.Value = new Color(byte.Parse(colorcodes[0]), byte.Parse(colorcodes[1]), byte.Parse(colorcodes[2]), byte.Parse(colorcodes[3]));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            });
            foreach (var item in DrillLocations)
            {
                var loc = Game1.getLocationFromName(item.Key);

                foreach (var item2 in item.Value) 
                {
                    Chest inputChest = new();
                    Chest batteryChest = new();
                    if (loc.Objects.TryGetValue(item2, out var obj) && obj?.ItemId == Constants.Id_ProgrammableDrill && IsThereAContainerNearby(obj, out List<Chest> container))
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
                                    if (obj.modData.TryGetValue(Constants.Key_LastInput, out string value) && value is not null and "")
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
                                            if (obj.heldObject.Value is not null && containerItem?.canStackWith(obj.heldObject.Value) is true)
                                            {
                                                obj.heldObject.Value.Stack = containerItem.addToStack(obj.heldObject.Value);
                                            }
                                        }
                                    }
                                    if (obj.modData.TryAdd(Constants.Key_LastInput, obj.heldObject.Value.QualifiedItemId))
                                    {
                                        obj.modData[Constants.Key_LastInput] = obj.heldObject.Value.QualifiedItemId;
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
                            if (obj.MinutesUntilReady <= 0 && obj.heldObject.Value is not null && Utility.canItemBeAddedToThisInventoryList(obj.heldObject.Value, chest.Items, chest.GetActualCapacity()))
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
                                ModEntry.CoreModEntry.Value.Helper.Reflection.GetField<IInventory>(typeof(StardewValley.Object), "autoLoadFrom", true).SetValue(chest.Items);

                                if (obj.heldObject.Value is null)
                                {
                                    if (obj.AttemptAutoLoad(chest.Items, Game1.player))
                                    {
                                        break;
                                    }
                                }
                                if (obj.heldObject.Value is not null && obj.readyForHarvest.Value && obj.MinutesUntilReady <= 0)
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

        public void OnMenuChanged(MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu menu2)
            {
                if (menu2.context is Building mineTent && mineTent.buildingType.Value == Constants.Id_MinecartRepository && mineTent.GetBuildingChest("Default_Chest") is not null and Chest buildingChest)
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
                else if (menu2.context is Chest chest && chest.ItemId == Constants.Id_MinecartChest)
                {
                    menu2.exitFunction = OnMenuExit;
                }
            }

            else if (e.OldMenu is ItemGrabMenu menu && menu.context is Chest chest && (chest.ItemId == Constants.Id_MinecartChest || chest.ItemId == Constants.Id_MachineryCollector))
            {
                chest.fixLidFrame();
                if (chest.Items.Count > 0 && chest.ItemId == Constants.Id_MinecartChest)
                {
                    OnMenuExit();
                }
            }
        }
        public void OnMenuExit()
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

        public bool IsThereAContainerNearby(StardewValley.Object drill, out List<Chest> container, bool hasToBeNotFull = false)
        {
            container = new();
            if (drill is null)
            {
                return false;
            }
            if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X + 1, drill.TileLocation.Y), out var chest) && chest is not null && chest is Chest chst && chest.ItemId == Constants.Id_MachineryCollector && (!hasToBeNotFull || chst.Items.CountItemStacks() < chst.GetActualCapacity()))
            {
                container.Add(chst);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X - 1, drill.TileLocation.Y), out chest) && chest is not null && chest is Chest chst2 && chest.ItemId == Constants.Id_MachineryCollector && (!hasToBeNotFull || chst2.Items.CountItemStacks() < chst2.GetActualCapacity()))
            {
                container.Add(chst2);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X, drill.TileLocation.Y + 1), out chest) && chest is not null && chest is Chest chst3 && chest.ItemId == Constants.Id_MachineryCollector && (!hasToBeNotFull || chst3.Items.CountItemStacks() < chst3.GetActualCapacity()))
            {
                container.Add(chst3);
            }
            else if (drill.Location.Objects.TryGetValue(new(drill.TileLocation.X, drill.TileLocation.Y - 1), out chest) && chest is not null && chest is Chest chst4 && chest.ItemId == Constants.Id_MachineryCollector && (!hasToBeNotFull || chst4.Items.CountItemStacks() < chst4.GetActualCapacity()))
            {
                container.Add(chst4);
            }
            return container.Count > 0;
        }
        public bool ShouldKeepSearching(Building building)
        {
            if (Game1.activeClickableMenu is not null)
                return false;
            if (building?.buildingType.Value == Constants.Id_MinecartRepository)
            {
                Chest defaultChest = (MineTent ?? building).GetBuildingChest("Default_Chest");
                
                foreach (var objs in Game1.player.team.GetOrCreateGlobalInventory(Constants.GlobalInventoryId_Minecarts))
                {
                    if (objs is null)
                        continue;
                    
                    if (Utility.canItemBeAddedToThisInventoryList(objs.getOne(), defaultChest.Items, defaultChest.GetActualCapacity()) && objs is not Tool)
                    {
                        foreach (var item in defaultChest.Items)
                        {
                            if (item?.canStackWith(objs) is true)
                            {
                                objs.Stack = item.addToStack(objs);
                            }
                        }
                        if (objs.Stack is not 0)
                        {
                            defaultChest.Items.Add(objs);
                            Game1.player.team.GetOrCreateGlobalInventory(Constants.GlobalInventoryId_Minecarts).RemoveButKeepEmptySlot(objs);
                        }
                        
                        if (objs.Stack == 0)
                        {
                            Game1.player.team.GetOrCreateGlobalInventory(Constants.GlobalInventoryId_Minecarts).RemoveButKeepEmptySlot(objs);
                        }
                    }
                }
                Game1.player.team.GetOrCreateGlobalInventoryMutex(Constants.GlobalInventoryId_Minecarts).Update(building.GetParentLocation());
                MineTent = building;
                return false;
            }
            return true;
        }
    }
}
