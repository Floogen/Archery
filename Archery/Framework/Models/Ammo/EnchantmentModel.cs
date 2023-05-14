using Archery.Framework.Interfaces.Internal;
using Archery.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using System.Collections.Generic;

namespace Archery.Framework.Models.Ammo
{
    public class EnchantmentModel
    {
        public string Id { get; set; }
        public TriggerType TriggerType { get; set; }
        public float TriggerChance { get; set; }
        public List<object> Arguments { get; set; }

        internal IEnchantment Generate(BasicProjectile projectile, GameTime time, GameLocation currentLocation, Farmer who)
        {
            return new Enchantment()
            {
                Projectile = projectile,
                Time = time,
                Location = currentLocation,
                Farmer = who,
                Arguments = this.Arguments
            };
        }
    }
}
