using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Interfaces.Internal
{
    internal class SpecialAttack : ISpecialAttack
    {
        public Slingshot Slingshot { get; init; }
        public GameTime Time { get; init; }
        public GameLocation Location { get; init; }
        public Farmer Farmer { get; init; }
        public List<object> Arguments { get; init; }

        internal WeaponType WeaponType { get; set; }
        internal Func<string> GetName { get; set; }
        internal Func<string> GetDescription { get; set; }
        internal Func<int> GetCooldownInMilliseconds { get; set; }
    }
}
