using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Interfaces.Internal
{
    internal class Enchantment : IEnchantment
    {
        public BasicProjectile Projectile { get; init; }
        public GameTime Time { get; init; }
        public GameLocation Location { get; init; }
        public Farmer Farmer { get; init; }
        public List<object> Arguments { get; init; }

        internal AmmoType AmmoType { get; set; }
        internal TriggerType TriggerType { get; set; }
        internal Func<float> GetTriggerChance { get; set; }
        internal Func<string> GetName { get; set; }
        internal Func<string> GetDescription { get; set; }
    }
}
