using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.SpecialOrders;

namespace VanillaPlusProfessions.Talents
{
    internal class TalentCore
    {
        internal static readonly PerScreen<List<string>> GainedTalents = new(createNewState: () => new());
        internal static readonly PerScreen<int> TalentPointCount = new(createNewState: () => 0);

        internal const string Key_GainedTalents = "Kedi.SMP.GainedTalentNames";
        internal const string Key_TalentPoints = "Kedi.SMP.TalentPointCount";

        internal const char Seperator_GainedTalents = '/';

        internal static List<Talent> Talents = new();

        internal static Dictionary<string, SkillTree> TalentTrees = new();

        internal static void Initialize()
        {
            ModEntry.Helper.Events.GameLoop.DayEnding += OnDayEnding;
            ModEntry.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            ModEntry.Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        internal static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.modData.TryGetValue(Key_TalentPoints, out string value))
            {
                if (int.TryParse(value, out int result) && result >= 0)
                {
                    TalentPointCount.Value = result;
                }
            }
            else
            {
                Game1.player.modData.TryAdd(Key_TalentPoints, "0");
            }
            if (Game1.player.modData.TryGetValue(Key_GainedTalents, out string value2))
            {
                if (!string.IsNullOrEmpty(value2))
                {
                    string[] gainedTalents = value2.Split(Seperator_GainedTalents);
                    for (int i = 0; i < gainedTalents.Length; i++)
                        if (!string.IsNullOrEmpty(gainedTalents[i]))
                            GainedTalents.Value.Add(gainedTalents[i]);
                }
            }
            else
            {
                Game1.player.modData.TryAdd(Key_GainedTalents, "");
            }
            Game1.player.achievements.OnValueAdded += OnAchievementAdded;
            Game1.player.team.specialOrders.OnElementChanged += OnSpecialOrderChanged;
        }
        internal static void OnAchievementAdded(int value)
        {
            TalentPointCount.Value++;
        }
        internal static void OnSpecialOrderChanged(NetList<SpecialOrder, NetRef<SpecialOrder>> list, int index, SpecialOrder OldOrder, SpecialOrder NewOrder)
        {
            if (NewOrder.ShouldDisplayAsComplete() && list.Contains(NewOrder) && NewOrder.orderType.Value is "Qi")
            {
                TalentPointCount.Value++;
            }
        }
        internal static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Game1.player.modData.TryAdd(Key_TalentPoints, TalentPointCount.Value.ToString()))
                Game1.player.modData[Key_TalentPoints] = TalentPointCount.Value.ToString();

            if (GainedTalents.Value.Count > 0)
            {
                if (string.IsNullOrEmpty(SerializeGainedTalents()))
                    return;
                if (!Game1.player.modData.TryAdd(Key_GainedTalents, SerializeGainedTalents()))
                    Game1.player.modData[Key_GainedTalents] = SerializeGainedTalents();
            }
        }
        internal static void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            TalentTrees = new();
            GainedTalents.Value = new();
            TalentPointCount.Value = 0;
            ModEntry.ProfessionsChosen.Value = new();
        }
        internal static string SerializeGainedTalents()
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < GainedTalents.Value.Count; i++)
            { 
                stringBuilder.Append(GainedTalents.Value[i]);
                stringBuilder.Append(Seperator_GainedTalents);
            }

            return stringBuilder.ToString();
        }
    }
}
