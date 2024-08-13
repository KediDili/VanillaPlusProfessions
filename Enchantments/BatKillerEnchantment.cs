using System.Xml.Serialization;
using StardewValley;
using StardewValley.Monsters;

namespace VanillaPlusProfessions.Enchantments
{
    [XmlType("Mods_Kedi.VPP.BatKillerEnchantment")]
    public class BatKillerEnchantment : SlingshotEnchantment
    {

        public BatKillerEnchantment() { }
        public override string GetName() => "Bat Killer";

        protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
        {
            if (monster is Bat)
            {
                amount *= 2;
            }
        }
    }
}
