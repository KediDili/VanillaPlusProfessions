﻿using StardewValley;
using StardewValley.Delegates;
using VanillaPlusProfessions.Utilities;
using StardewValley.Locations;

namespace VanillaPlusProfessions.Compatibility
{
    public class CustomQueries
    {
        public void Initialize()
        {
            GameStateQuery.Register(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_WasRainingHereYesterday", WasRainingHereYesterday);
            GameStateQuery.Register(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_PlayerHasTalent", PlayerHasTalent);
            GameStateQuery.Register(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_PlayerHasProfession", PlayerHasProfession);
            GameStateQuery.Register(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_IsLavaLocation", IsLavaLocation);
            GameStateQuery.Register(ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_IsConsistentMineLocation", IsConsistentMineLocation);
        }

        bool WasRainingHereYesterday(string[] query, GameStateQueryContext context)
        {
            if (context.Location is null)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Null location was provided to {ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_" + nameof(WasRainingHereYesterday)} query.\n - Query string: {string.Join(" ", query)}", StardewModdingAPI.LogLevel.Warn);
                return false;
            }

            if (context.Location.modData.TryGetValue(Constants.Key_WasRainingHere, out string val) && bool.TryParse(val, out bool result))
            {
                return result;
            }
            if (ModEntry.CoreModEntry.Value.ModMonitor.IsVerbose || ModEntry.CoreModEntry.Value.ModConfig.DeveloperOrTestingMode)
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Location named {context.Location.NameOrUniqueName} doesn't have the metadata for VPP to know whether it rained yesterday or not but it was passed to the {nameof(WasRainingHereYesterday)} query. This message is not an error and only intended for troubleshooting purposes in case of bugs.\n - Query string: {string.Join(" ", query)}", StardewModdingAPI.LogLevel.Warn);
            }
            return false;
        }

        bool PlayerHasTalent(string[] query, GameStateQueryContext context)
        {
            if (!ArgUtility.TryGet(query, 1, out var farmer, out var error) || !ArgUtility.TryGet(query, 2, out var talentToCheck, out error))
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Invalid values were provided to {ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_" + nameof(PlayerHasTalent)} query.\n - Query string: {string.Join(" ", query)}\n - Error: {error}", StardewModdingAPI.LogLevel.Warn);
            }
            else if ((context.Player ?? Game1.player).mailReceived.Count > 0)
            {
                return GameStateQuery.Helpers.WithPlayer(context.Player ?? Game1.player, farmer, farmer =>
                {
                    bool value = !ModEntry.CoreModEntry.Value.ModConfig.ProfessionsOnly && TalentUtility.CurrentPlayerHasTalent(talentToCheck, who: farmer);
                    return value;
                });
            }
            return false;
        }

        bool PlayerHasProfession(string[] query, GameStateQueryContext context)
        {
            if (!ArgUtility.TryGet(query, 1, out var farmer, out var error) || !ArgUtility.TryGet(query, 2, out var professionToCheck, out error))
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Invalid values were provided to {ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_" + nameof(PlayerHasProfession)} query.\n - Query string: {string.Join(" ", query)}\n - Error: {error}", StardewModdingAPI.LogLevel.Warn);
                return false;
            }

            return GameStateQuery.Helpers.WithPlayer(context.Player ?? Game1.player, farmer, farmer =>
            {
                return CoreUtility.CurrentPlayerHasProfession(professionToCheck, useThisInstead: farmer);
            });
        }

        bool IsLavaLocation(string[] query, GameStateQueryContext context)
        {
            GameLocation location = null;
            if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string error))
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Invalid values were provided to {ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_" + nameof(IsLavaLocation)} query.\n - Query string: {string.Join(" ", query)}\n - Error: {error}", StardewModdingAPI.LogLevel.Warn);
                ModEntry.CoreModEntry.Value.ModMonitor.Log(error, StardewModdingAPI.LogLevel.Warn);
            }
            else if (location is Caldera || ((location.GetData()?.CustomFields?.TryGetValue(Constants.Key_IsLavaLocation, out string value) is true || location.GetData()?.CustomFields?.TryGetValue(Constants.Key_IsLavaLocation2, out value) is true) && value.ToLower() is "true"))
            {
                return true;
            }
            return false;
        }

        bool IsConsistentMineLocation(string[] query, GameStateQueryContext context)
        {
            GameLocation location = null;
            if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string error))
            {
                ModEntry.CoreModEntry.Value.ModMonitor.Log($"Invalid values were provided to {ModEntry.CoreModEntry.Value.Manifest.UniqueID + "_" + nameof(IsConsistentMineLocation)} query.\n - Query string: {string.Join(" ", query)}\n - Error: {error}", StardewModdingAPI.LogLevel.Warn);
                ModEntry.CoreModEntry.Value.ModMonitor.Log(error, StardewModdingAPI.LogLevel.Warn);
            }
            else if (location is Mine || (location.GetData()?.CustomFields?.TryGetValue(Constants.Key_IsConsistentMineLocation, out string value) is true || (location.GetData()?.CustomFields?.TryGetValue(Constants.Key_IsConsistentMineLocation2, out value) is true) && value.ToLower() is "true"))
            {
                return true;
            }
            return false;
        }
    }
}
