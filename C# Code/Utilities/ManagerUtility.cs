using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using System.Linq;
using StardewValley.Internal;

namespace VanillaPlusProfessions.Utilities
{
    internal class ManagerUtility
    {
        public static void reset(string command, string[] args)
        {
            if (args.Length < 2)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Insufficient arguments.", LogLevel.Warn);
                return;
            }
            if (!Context.IsWorldReady)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log("Load a save first (or wait until your save loads).", LogLevel.Warn);
                return;
            }
            int skill = -1;
            if (int.TryParse(args[0], out int level) && (int.TryParse(args[1], out skill) || Farmer.getSkillNumberFromName(args[1]) is not -1))
            {
                if (Farmer.getSkillNumberFromName(args[1]) is not -1)
                {
                    skill = Farmer.getSkillNumberFromName(args[1]);
                }
                if (level is 15)
                {
                    foreach (var item in ModEntry.Professions.Values)
                    {
                        if (item.Skill.ToString() == args[1] && item.ID < 467870)
                        {
                            Game1.player.professions.Remove(item.ID);
                        }
                    }
                }
                else if (level is 20)
                {
                    foreach (var item in ModEntry.Managers[5].RelatedProfessions.Values)
                    {
                        if (item.Skill.ToString() == args[1] && item.ID >= 467870)
                        {
                            Game1.player.professions.Remove(item.ID);
                        }
                    }
                }

                Game1.player.newLevels.Add(new(skill, level));
                for (int i = 0; i < Game1.player.newLevels.Count; i++)
                {
                    if (Game1.activeClickableMenu is null)
                    {
                        Game1.activeClickableMenu = new LevelUpMenu(Game1.player.newLevels[i].X, Game1.player.newLevels[i].Y);
                    }
                    else
                    {
                        Game1.nextClickableMenu.Add(new LevelUpMenu(Game1.player.newLevels[i].X, Game1.player.newLevels[i].Y));
                    }
                }
            }
        }
        public static string GetProduceTimeBasedOnPrice(TerrainFeature feature, out Object produce)
        {
            int price = 140;
            produce = null;
            if (feature is FruitTree tree)
            {
                ItemQueryContext context = new(feature.Location, Game1.player, Game1.random, "Farming-Foraging FruitTree context");

                var chosenData = Game1.random.ChooseFrom(tree.GetData().Fruit);
                if (GameStateQuery.CheckConditions(chosenData.Condition, feature.Location, Game1.player) && Game1.random.NextBool(chosenData.Chance) && !chosenData.IsRecipe)
                {
                    object chosenProduce = chosenData.RandomItemId?.Any() is true
                        ? Game1.random.ChooseFrom(ItemQueryResolver.TryResolve(Game1.random.ChooseFrom(chosenData.RandomItemId), context, ItemQuerySearchMode.All, chosenData.PerItemCondition, chosenData.MaxItems)).Item
                        : (object)Game1.random.ChooseFrom(ItemQueryResolver.TryResolve(chosenData.ItemId, context, ItemQuerySearchMode.All, chosenData.PerItemCondition, chosenData.MaxItems)).Item;

                    if (chosenProduce is Object obj)
                    {
                        price = obj.Price;
                        produce = obj;
                    }
                }
            }
            else if (feature is GiantCrop crop)
            {
                ItemQueryContext context = new(feature.Location, Game1.player, Game1.random, "Farming-Foraging GiantCrop context");

                var chosenData = Game1.random.ChooseFrom(crop.GetData().HarvestItems);
                if (GameStateQuery.CheckConditions(chosenData.Condition, feature.Location, Game1.player) && Game1.random.NextBool(chosenData.Chance) && chosenData.ForShavingEnchantment is not true && !chosenData.IsRecipe)
                {
                    object chosenProduce = chosenData.RandomItemId?.Any() is true
                        ? Game1.random.ChooseFrom(ItemQueryResolver.TryResolve(Game1.random.ChooseFrom(chosenData.RandomItemId), context, ItemQuerySearchMode.All, chosenData.PerItemCondition, chosenData.MaxItems)).Item
                        : (object)Game1.random.ChooseFrom(ItemQueryResolver.TryResolve(chosenData.ItemId, context, ItemQuerySearchMode.All, chosenData.PerItemCondition, chosenData.MaxItems)).Item;
                    
                    if (chosenProduce is Object obj)
                    {
                        price = obj.Price / 2;
                        produce = obj;
                    }
                }
            }
            return (price / 20).ToString();
        }

        public static Vector2 GetFeatureTile(TerrainFeature terrainFeature)
        {
            if (terrainFeature is GiantCrop)
            {
                return new(terrainFeature.Tile.X + 1, terrainFeature.Tile.Y + 2);
            }
            return terrainFeature.Tile;
        }

        public static Object CreateFlavoredSyrupOrDust(Object ingredient)
        {
            if (ingredient?.modData?.TryGetValue("Kedi.VPP.CurrentPreserveType", out string value) is true)
            {
                if (value is Constants.Id_FruitSyrup or Constants.Id_GemDust)
                {
                    Object result = new ColoredObject(value, 1, TailoringMenu.GetDyeColor(ingredient) ?? Color.White);

                    result.heldObject.Value = ingredient;
                    result.preserve.Value = (Object.PreserveType)1115;
                    result.preservedParentSheetIndex.Value = ingredient.ParentSheetIndex.ToString();
                    result.Name = result.displayName = ingredient.Name + " " + (value is Constants.Id_FruitSyrup ? "Syrup" : "Dust");
                    result.displayName = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Item." + (value is Constants.Id_FruitSyrup ? "Syrup" : "Dust") + ".ProduceNameFormat").ToString().Replace("{0}", ingredient.DisplayName);
                    result.displayNameFormat = ModEntry.CoreModEntry.Value.Helper.Translation.Get("Item." + (value is Constants.Id_FruitSyrup ? "Syrup" : "Dust") + ".ProduceNameFormat").ToString().Replace("{0}", ingredient.DisplayName); ;
                    result.Price += ingredient.Price / 2;
                    if (value is Constants.Id_FruitSyrup && ingredient.Edibility != -300)
                        result.Edibility = ingredient.Edibility * 2;
                    return result;
                }
            }
            return ingredient;
        }

        public static void FertilizerStackEffects()
        {
            if (!CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Agronomist) || (CoreUtility.CurrentPlayerHasProfession(Constants.Profession_Agronomist) && !Game1.random.NextBool(0.3)))
                Game1.player.ActiveObject.Stack--;
            if (Game1.player.ActiveObject.Stack == 0)
                Game1.player.ActiveObject = null;
        }
        
        public static bool IsSlimeWhite(GreenSlime slime) => slime.color.Value.R > 230 && slime.color.Value.G > 230 && slime.color.Value.B > 230;

        public static Object CreateColoredPetrifiedSlime(Color slimeColor)
        {
            Color newSlimeColor = slimeColor;
            Object result = new ColoredObject("Kedi.VPP.ColoredPetrifiedSlime", 1, newSlimeColor);
            return result;
        }
        public static string GemDustToFertilizer(Object @object)
        {
            if (@object.heldObject.Value is null)
                return "";
            for (int i = 0; i < Constants.Fertilizer_Color_Tags.Count; i++)
            {
                for (int l = 0; l < Constants.Fertilizer_Color_Tags[i].Count; l++)
                {
                    if (@object.heldObject.Value.HasContextTag(Constants.Fertilizer_Color_Tags[i][l]))
                    {
                        if (i == 3)
                        {
                            return "(O)805";
                        }
                        return @object.Price switch
                        {
                            < 120 => Constants.Fertilizer_Ids[i * 3],
                            > 280 => Constants.Fertilizer_Ids[i * 3 + 1],
                            _ => Constants.Fertilizer_Ids[i * 3 + 2]
                        };
                    }
                }
            }
            return "";
        }
    }
}
