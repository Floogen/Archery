﻿using Archery.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Interfaces.Internal
{
    public interface IApi
    {
        KeyValuePair<bool, string> PlaySound(IManifest callerManifest, ISound sound, Vector2 position);
        KeyValuePair<bool, Vector2> GetProjectileVelocity(IManifest callerManifest, BasicProjectile projectile);
        KeyValuePair<bool, string> SetProjectileVelocity(IManifest callerManifest, BasicProjectile projectile, Vector2 velocity);
        KeyValuePair<bool, string> SetChargePercentage(IManifest callerManifest, Slingshot slingshot, float percentage);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, BasicProjectile projectile, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, string ammoId, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false);
        KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, WeaponType whichWeaponTypeCanUse, Func<string> getDisplayName, Func<string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler);
        KeyValuePair<bool, string> DeregisterSpecialAttack(IManifest callerManifest, string name);

        event EventHandler<WeaponFiredEventArgs> OnWeaponFired;
        event EventHandler<WeaponChargeEventArgs> OnWeaponCharging;
        event EventHandler<WeaponChargeEventArgs> OnWeaponCharged;
        event EventHandler<CrossbowLoadedEventArgs> OnCrossbowLoaded;
        event EventHandler<AmmoChangedEventArgs> OnAmmoChanged;
        event EventHandler<AmmoHitMonsterEventArgs> OnAmmoHitMonster;
    }

    #region Interface objects
    public interface ISpecialAttack
    {
        public Slingshot Slingshot { get; init; }
        public GameTime Time { get; init; }
        public GameLocation Location { get; init; }
        public Farmer Farmer { get; init; }

        public List<object> Arguments { get; init; }
    }

    public interface IProjectileData
    {
        public string AmmoId { get; set; }
        public Vector2? Velocity { get; set; }
        public int? BaseDamage { get; set; }
        public float? CriticalChance { get; set; }
        public float? CriticalDamageMultiplier { get; set; }
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

    #region Enums
    public enum WeaponType
    {
        Any,
        [Obsolete("Not currently used")]
        Slingshot,
        Bow,
        Crossbow
    }
    #endregion

    #region Events
    public class BaseEventArgs : EventArgs
    {
        public Vector2 Origin { get; init; }
    }

    public class WeaponFiredEventArgs : BaseEventArgs
    {
        public string WeaponId { get; init; }
        public string AmmoId { get; init; }
        public BasicProjectile Projectile { get; init; }
    }

    public class WeaponChargeEventArgs : BaseEventArgs
    {
        public string WeaponId { get; init; }
        public float ChargePercentage { get; init; }
    }

    public class CrossbowLoadedEventArgs : BaseEventArgs
    {
        public string WeaponId { get; init; }
        public string AmmoId { get; init; }
    }

    public class AmmoChangedEventArgs : BaseEventArgs
    {
        public string WeaponId { get; init; }
        public string AmmoId { get; init; }
    }

    public class AmmoHitMonsterEventArgs : WeaponFiredEventArgs
    {
        public Monster Monster { get; init; }
    }
    #endregion
}
