using Archery.Framework.Interfaces.Internal;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Models.Ammo
{
    public class EnchantmentModel
    {
        public string Id { get; set; }
        public TriggerType TriggerType { get; set; }
        public float TriggerChance { get; set; }
        public List<object> Arguments { get; set; }

        internal bool ShouldTrigger(Random random)
        {
            return random.NextDouble() <= TriggerChance;
        }

        internal IEnchantment Generate(BasicProjectile projectile, GameTime time, GameLocation currentLocation, Farmer who, Monster? monster)
        {
            return new Enchantment()
            {
                Projectile = projectile,
                Time = time,
                Location = currentLocation,
                Farmer = who,
                Monster = monster,
                Arguments = this.Arguments
            };
        }
    }
}
