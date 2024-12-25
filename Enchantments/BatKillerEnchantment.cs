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
        public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool isBomb, ref int amount)
        {
            if (monster is Bat)
            {
                amount *= 2;
            }
        }
    }
}
