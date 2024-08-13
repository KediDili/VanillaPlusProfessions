using System.Linq;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;

namespace VanillaPlusProfessions.Enchantments
{
    [XmlType("Mods_Kedi.VPP.RapidEnchantment")]
    public class RapidEnchantment : SlingshotEnchantment
    {
        public RapidEnchantment() { }

        public override string GetName() => "Rapid";

        protected override void _OnShoot(GameLocation gameLocation, Slingshot slingshot)
        {
            BasicProjectile basicProjectile = gameLocation.projectiles.LastOrDefault() as BasicProjectile;
            if (basicProjectile is not null && basicProjectile.uniqueID is not null && slingshot.getLastFarmerToUse().CurrentTool == slingshot)
            {
                basicProjectile.maxVelocity.Value *= 2f;
                basicProjectile.xVelocity.Value *= 2f;
                basicProjectile.yVelocity.Value *= 2f;
            }
        }
    }
}