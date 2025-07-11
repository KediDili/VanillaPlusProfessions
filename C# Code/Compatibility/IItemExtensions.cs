using StardewValley;
using Microsoft.Xna.Framework;
namespace VanillaPlusProfessions.Compatibility
{
    public interface IItemExtensions
    {
        /// <summary>
        /// Checks for resource data with the Stone type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsStone(string id);

        /// <summary>
        /// Checks for resource data in the mod.
        /// </summary>
        /// <param name="id">Qualified item ID</param>
        /// <param name="health">MinutesUntilReady value</param>
        /// <param name="itemDropped">Item dropped by ore</param>
        /// <returns>Whether the object has resource data.</returns>
        bool IsResource(string id, out int? health, out string itemDropped);

        /// <summary>
        /// Checks for a qualified id in modded clump data (vanilla not included).
        /// </summary>
        /// <param name="qualifiedItemId">Qualified item ID.</param>
        /// <returns>Whether this id is a clump's.</returns>
        bool IsClump(string qualifiedItemId);

        /// <summary>
        /// Tries to spawn a clump.
        /// </summary>
        /// <param name="itemId">The clump ID.</param>
        /// <param name="position">Tile position.</param>
        /// <param name="location">Location to use.</param>
        /// <param name="error">Error string, if applicable.</param>
        /// <param name="avoidOverlap">Avoid overlapping with other clumps.</param>
        /// <returns>Whether spawning succeeded.</returns>
        bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string error, bool avoidOverlap = false);

        bool GetResourceData(string id, bool isClump, out object data);
    }
}
