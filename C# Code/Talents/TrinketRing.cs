using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using VanillaPlusProfessions.Utilities;

namespace VanillaPlusProfessions.Talents
{
    [XmlType("Mods_Kedi.VPP.TrinketRing")]
    public class TrinketRing : Ring
    {
        public Trinket Trinket;

        public TrinketRing()
        {

        }

        public TrinketRing(Trinket trinket) : this()
        {
            Trinket = trinket;
            string ringID = TalentUtility.GetIDOfRing(trinket);
            if (ringID is not null)
            {
                ObjectData data = Game1.objectData[ringID];
                ItemId = ringID;
                Category = Object.ringCategory;
                Name = data.Name ?? ItemRegistry.GetDataOrErrorItem(QualifiedItemId).InternalName;
                price.Value = data.Price;
                loadDisplayFields();
            }
        }

        public override bool CanCombine(Ring ring) => false;

        public override void onEquip(Farmer who)
        {
            base.onEquip(who);
            Trinket.Apply(who);
        }

        public override void update(GameTime time, GameLocation environment, Farmer who)
        {
            base.update(time, environment, who);
            Trinket.Update(who, time, environment);
        }

        public override void onUnequip(Farmer who)
        {
            base.onUnequip(who);
            Trinket.Unapply(who);
        }
    }
}
