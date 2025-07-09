using System;
using System.Linq;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents
{
    [XmlType("Mods_Kedi.VPP.TrinketRing")]
    public class TrinketRing : Ring
    {
        [Obsolete("As of version 1.0.8, this field is no longer needed. Only kept to make sure existing saves don't break. Use <GetRingTrinket()> instead.")]
        public Trinket Trinket = new();

        public TrinketRing()
        {
            
        }

        public TrinketRing(Trinket trinket) : this()
        {
            string ringID = TalentUtility.GetIDOfRing(trinket);
            if (ringID is not null && Game1.objectData.ContainsKey(ringID))
            {                
                ObjectData data = Game1.objectData[ringID];
                ItemId = ringID;
                Category = StardewValley.Object.ringCategory;
                Name = data.Name ?? ItemRegistry.GetDataOrErrorItem(QualifiedItemId).InternalName;
                price.Value = data.Price;
                loadDisplayFields();
            }
        }

        public Trinket GetRingTrinket(bool remove = false)
        {
            string guid = modData[ModEntry.Key_RingTrinkets];
            Item trinketFound = Game1.player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryID_RingTrinkets).Where(trinket => { return trinket.modData.TryGetValue(ModEntry.Key_RingTrinkets, out string trinketGuid) && trinketGuid == guid; }).FirstOrDefault();
            if (remove)
            {
                Game1.player.team.GetOrCreateGlobalInventory(ModEntry.GlobalInventoryID_RingTrinkets).Remove(trinketFound);
            }

            return trinketFound as Trinket;
        }

        public override bool CanCombine(Ring ring) => false;

        public override void onEquip(Farmer who)
        {
            if (!Game1.player.trinketItems.Any())
                Game1.player.trinketItems.Add(null);
            if (modData.ContainsKey(ModEntry.Key_RingTrinkets))
            {
                Trinket trinket1 = GetRingTrinket();
                if (ModEntry.WearMoreRingsAPI is not null && Game1.player.trinketItems.Count == 1)
                    Game1.player.trinketItems.Add(null);

                Game1.player.trinketItems.Add(trinket1);
            }
            
            base.onEquip(Game1.player);
        }
        public override void onUnequip(Farmer who)
        {
            if (modData.ContainsKey(ModEntry.Key_RingTrinkets))
            {
                Trinket trinket = GetRingTrinket();
                Game1.player.trinketItems.Remove(trinket);
            }
            base.onUnequip(who);
        }
    }
}
