using System.Linq;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;

namespace VanillaPlusProfessions.Enchantments
{
    [XmlType("Mods_Kedi.VPP.AutoFireEnchantment")]
    public class AutoFireEnchantment : SlingshotEnchantment
    {
        public AutoFireEnchantment() { }

        public override string GetName() => ModEntry.CoreModEntry.Value.Helper.Translation.Get("Enchantments.AutoFire.Name");

        protected override void _OnShoot(GameLocation gameLocation, Slingshot slingshot)
        {
            BasicProjectile basicProjectile = gameLocation.projectiles.LastOrDefault() as BasicProjectile;
            if (basicProjectile is not null && basicProjectile.uniqueID is not null)
            {
                basicProjectile.maxVelocity.Value /= 2f;
                basicProjectile.xVelocity.Value /= 2f;
                basicProjectile.yVelocity.Value /= 2f;
            }
        }
    }
}
