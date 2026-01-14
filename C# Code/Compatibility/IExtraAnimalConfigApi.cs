<<<<<<< Updated upstream
using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.GameData;

namespace VanillaPlusProfessions.Compatibility;

public interface IExtraAnimalConfigApi
{
    // Get a list of every modded feed that is/can be stored;
    // the result is a dictionary of *qualified* item IDs
    // to an IFeedInfo object that can be used to get the capacity and modify count.
    // The IFeedInfo object is stateless so you can save it if you want.
    public IDictionary<string, IFeedInfo> GetModdedFeedInfo();
    // Get the qualified id of the feed override for this building type that replaces hay, if any.
    // Does not get any custom troughs placed alongside vanilla hay troughs in buildings
    public string? GetFeedOverride(string? buildingId);
}

public interface IFeedInfo
{
    // The total capacity
    public int capacity { get; }
    // The current count
    public int count { get; set; }
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanillaPlusProfessions.Compatibility
{
    public interface IExtraAnimalConfigApi
    {
        // Get a list of every modded feed that is/can be stored;
        // the result is a dictionary of *qualified* item IDs
        // to an IFeedInfo object that can be used to get the capacity and modify count.
        // The IFeedInfo object is stateless so you can save it if you want.
        public IDictionary<string, IFeedInfo> GetModdedFeedInfo();
        // Get the qualified id of the feed override for this building type that replaces hay, if any.
        // Does not get any custom troughs placed alongside vanilla hay troughs in buildings
        public string? GetFeedOverride(string? buildingId);
    }

    public interface IFeedInfo
    {
        // The total capacity
        public int capacity { get; }
        // The current count
        public int count { get; set; }
    }
>>>>>>> Stashed changes
}
