using StardewValley;
using StardewValley.Delegates;
using VanillaPlusProfessions.Talents;

namespace VanillaPlusProfessions.Compatibility
{
    public class CustomQueries
    {
        public void Initialize()
        {
            GameStateQuery.Register(ModEntry.Manifest.UniqueID + "_WasRainingHereYesterday", WasRainingHereYesterday);
        }

        public bool WasRainingHereYesterday(string[] query, GameStateQueryContext context)
        {
            if (context.Location is null)
            {
                ModEntry.ModMonitor.Log($"Null location was provided to {ModEntry.Manifest.UniqueID + "_" + nameof(WasRainingHereYesterday)} query.\n - Query string: {string.Join(',', query)}", StardewModdingAPI.LogLevel.Warn);
                return false;
            }

            if (context.Location.modData.TryGetValue(TalentCore.Key_WasRainingHere, out string val))
            {
                return bool.TryParse(val, out bool result) && result;
            }

            ModEntry.ModMonitor.Log($"Location named {context.Location.NameOrUniqueName} doesn't have the metadata for VPP to know but it was passed to the {nameof(WasRainingHereYesterday)} query. This message might not always mean there is a bug with the mod that adds this location.\n - Query string: {string.Join(',',query)}");
            return false;
        }
    }
}
