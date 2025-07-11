using StardewValley.Objects;
namespace VanillaPlusProfessions.Compatibility
{
    public interface IWearMoreRings
    {
        /// <summary>
        /// Get the mod's config setting for how many rings can be equipped.
        /// 
        /// Note that this value is not synchronized in multiplayer, so its only valid for the current player (Game1.player).
        /// </summary>
        /// <returns>Config setting for how many rings the local player can wear.</returns>
        int RingSlotCount();

        /// <summary>
        /// Get the ring that the local player has equipped in the given slot. 
        /// </summary>
        /// <param name="slot">The ring equipment slot being queried. Ranging from 0 to RingSlotCount()-1.</param>
        /// <returns>The ring equipped in the given slot or null if its empty.</returns>
        Ring GetRing(int slot);
    }
}
