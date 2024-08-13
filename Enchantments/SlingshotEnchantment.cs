using System.Xml.Serialization;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace VanillaPlusProfessions.Enchantments
{
    [XmlType("Mods_Kedi.VPP.SlingshotEnchantment")]
    public class SlingshotEnchantment : BaseEnchantment
    {

        public SlingshotEnchantment() { }

        public override bool CanApplyTo(Item item) => item is Slingshot;

        public void OnShoot(GameLocation gameLocation, Slingshot slingshot)
        {
            if (slingshot.getLastFarmerToUse().CurrentTool == slingshot)
            {
                _OnShoot(gameLocation, slingshot);
            }
        }

        protected virtual void _OnShoot(GameLocation gameLocation, Slingshot slingshot)
        {

        }
    }
}
