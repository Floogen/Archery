using Archery.Framework.Interfaces.Internal;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;

namespace Archery.Framework.Models.Weapons
{
    public class SpecialAttackModel
    {
        public string Id { get; set; }
        public bool TriggerAfterButtonRelease { get; set; }
        public List<object> Arguments { get; set; }

        internal ISpecialAttack Generate(Slingshot slingshot, GameTime time, GameLocation currentLocation, Farmer who)
        {
            return new SpecialAttack()
            {
                Slingshot = slingshot,
                Time = time,
                Location = currentLocation,
                Farmer = who,
                Arguments = this.Arguments
            };
        }
    }
}
