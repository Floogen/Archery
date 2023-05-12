using Archery.Framework.Models.Generic;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Weapons;
using Archery.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Interfaces.Internal
{
    public interface IApi
    {
        KeyValuePair<bool, string> SetChargePercentage(IManifest callerManifest, Slingshot slingshot, float percentage);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, BasicProjectile projectile, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, string ammoId, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, Func<string> getDisplayName, Func<string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler);
        KeyValuePair<bool, string> DeregisterSpecialAttack(IManifest callerManifest, string name);
    }

    #region interface objects
    public interface ISpecialAttack
    {
        public Slingshot Slingshot { get; init; }
        public GameTime Time { get; init; }
        public GameLocation Location { get; init; }
        public Farmer Farmer { get; init; }

        public List<object> Arguments { get; init; }
    }

    public interface ISound
    {
        public string Name { get; set; }
        public int Pitch { get; set; }
        public RandomRange PitchRandomness { get; set; }
        public float Volume { get; set; }
        public float MaxDistance { get; set; }
    }
    #endregion
}
