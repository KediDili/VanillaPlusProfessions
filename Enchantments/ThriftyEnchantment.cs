using System.Xml.Serialization;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace VanillaPlusProfessions.Enchantments
{
    [XmlType("Mods_Kedi.VPP.ThriftyEnchantment")]
    public class ThriftyEnchantment : SlingshotEnchantment
    {

        public ThriftyEnchantment() { }
        public override string GetName() => "Thrifty";

        protected override void _OnShoot(GameLocation gameLocation, Slingshot slingshot)
        {
            if (Game1.random.NextBool(0.35) && slingshot.attachments[0] is not null)
            {
                slingshot.attachments[0].Stack++;
            }
        }
    }
}