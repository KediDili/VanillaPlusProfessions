using System.Collections.Generic;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace VanillaPlusProfessions.Managers
{
    internal class ManagerUtility
    {
        public static Object CreateFlavoredSyrupOrDust(Object ingredient)
        {
            if (ingredient.modData.TryGetValue("Kedi.SMP.CurrentPreserveType", out string value))
            {
                if (value is "Kedi.SMP.GemDust" or "Kedi.SMP.FruitSyrup")
                {
                    Object result = new ColoredObject(value, 1, TailoringMenu.GetDyeColor(ingredient) ?? Color.White);

                    result.heldObject.Value = ingredient;
                    result.preserve.Value = (Object.PreserveType)11;
                    result.preservedParentSheetIndex.Value = ingredient.ParentSheetIndex.ToString();
                    result.displayName = ModEntry.Helper.Translation.Get("Item." + (value is "Kedi.SMP.FruitSyrup" ? "Syrup" : "Dust") + ".ProduceNameFormat").ToString().Replace("{0}", ingredient.DisplayName); ;
                    result.displayNameFormat = ModEntry.Helper.Translation.Get("Item." + (value is "Kedi.SMP.FruitSyrup" ? "Syrup" : "Dust") + ".ProduceNameFormat").ToString().Replace("{0}", ingredient.DisplayName); ;
                    result.Price += ingredient.Price / 2;
                    if (value is "Kedi.SMP.FruitSyrup")
                        result.Edibility = ingredient.Edibility * 2;
                    return result;
                }
            }
            return null;
        }

        public static void TapperOnTerrainFeature(TerrainFeature __instance, SpriteBatch b)
        {
            if (__instance.modData.TryGetValue("KediDili.SomeMoreProfessions.IsTapped", out string value) && value.ToLower() is "true")
            {
                var data = ItemRegistry.GetData(__instance.modData["KediDili.SomeMoreProfessions.TapperID"]);
                var rect = data.GetSourceRect();
                rect.Height /= 2;
                b.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.Tile.X * 64f, __instance.Tile.Y * 64f - 64f)), rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.getBoundingBox().Bottom + 10000) / 87000f - __instance.Tile.X / 1000000f);
            }
        }

        public static void FertilizerStackEffects()
        {
            if (!CoreUtility.CurrentPlayerHasProfession(37) || (CoreUtility.CurrentPlayerHasProfession(37) && !Game1.random.NextBool(0.3)))
                Game1.player.ActiveObject.Stack--;
            if (Game1.player.ActiveObject.Stack == 0)
                Game1.player.ActiveObject = null;
        }
        
        public static bool IsSlimeWhite(GreenSlime slime) => slime.color.Value.R > 230 && slime.color.Value.G > 230 && slime.color.Value.B > 230;

        public static Object CreateColoredPetrifiedSlime(Color slimeColor)
        {
            Object result = new ColoredObject("Kedi.SMP.ColoredPetrifiedSlime", 1, slimeColor);
            return result;
        }
        public static string GemDustToFertilizer(Object @object)
        {
            List<List<string>> tags = new() {
            new() { "color_orange", "color_yellow", "color_brown", "color_sand", "color_poppyseed", "color_dark_orange", "color_dark_yellow", "color_gold", "color_copper" },
            new() { "color_blue", "color_cyan", "color_aquamarine", "color_light_cyan", "color_dark_blue", "color_dark_cyan", "color_white", "color_gray" },
            new() { "color_red", "color_purple", "color_pink", "color_pale_violet_red", "color_salmon", "color_dark_red", "color_dark_purple", "color_dark_pink", "color_iridium" },
            new() { "color_green", "color_sea_green", "color_lime", "color_yellow_green", "color_jade", "color_dark_green", "color_black", "color_dark_brown" } };

            List<string> ids = new() { "(O)368", "(O)369", "(O)919", "(O)370", "(O)371", "(O)920", "(O)465", "(O)466", "(O)918" };
            if (@object.heldObject.Value is null)
                return "";
            for (int i = 0; i < tags.Count; i++)
            {
                for (int l = 0; l < tags[i].Count; l++)
                {
                    if (@object.heldObject.Value.HasContextTag(tags[i][l]))
                    {
                        if (i == 3)
                        {
                            return "(O)805";
                        }
                        return @object.Price switch
                        {
                            < 120 => ids[i * 3],
                            > 280 => ids[i * 3 + 1],
                            _ => ids[i * 3 + 2]
                        };
                    }
                }
            }
            return "";
        }
    }
}
