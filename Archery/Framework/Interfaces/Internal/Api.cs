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
    public class Api : IApi
    {
        private IMonitor _monitor;
        private Dictionary<string, Func<ISpecialAttack, bool>> _registeredSpecialAttackMethods;
        private Dictionary<string, WeaponType> _registeredSpecialAttackFilter;
        private Dictionary<string, Func<string>> _registeredSpecialAttackNames;
        private Dictionary<string, Func<string>> _registeredSpecialAttackDescriptions;
        private Dictionary<string, Func<int>> _registeredSpecialAttackCooldowns;

        public event EventHandler<WeaponFiredEventArgs> OnWeaponFired;
        public event EventHandler<WeaponChargeEventArgs> OnWeaponCharging;
        public event EventHandler<WeaponChargeEventArgs> OnWeaponCharged;
        public event EventHandler<CrossbowLoadedEventArgs> OnCrossbowLoaded;
        public event EventHandler<AmmoChangedEventArgs> OnAmmoChanged;
        public event EventHandler<AmmoHitMonsterEventArgs> OnAmmoHitMonster;


        internal Api(IMonitor monitor)
        {
            _monitor = monitor;
            _registeredSpecialAttackMethods = new Dictionary<string, Func<ISpecialAttack, bool>>();
            _registeredSpecialAttackFilter = new Dictionary<string, WeaponType>();
            _registeredSpecialAttackNames = new Dictionary<string, Func<string>>();
            _registeredSpecialAttackDescriptions = new Dictionary<string, Func<string>>();
            _registeredSpecialAttackCooldowns = new Dictionary<string, Func<int>>();
        }

        #region Events
        internal void TriggerOnWeaponFired(WeaponFiredEventArgs weaponFiredEventArgs)
        {
            var handler = OnWeaponFired;
            if (handler is not null)
            {
                handler(this, weaponFiredEventArgs);
            }
        }

        internal void TriggerOnWeaponCharging(WeaponChargeEventArgs weaponChargeEventArgs)
        {
            var handler = OnWeaponCharging;
            if (handler is not null)
            {
                handler(this, weaponChargeEventArgs);
            }
        }

        internal void TriggerOnWeaponCharged(WeaponChargeEventArgs weaponChargeEventArgs)
        {
            var handler = OnWeaponCharged;
            if (handler is not null)
            {
                handler(this, weaponChargeEventArgs);
            }
        }

        internal void TriggerOnCrossbowLoaded(CrossbowLoadedEventArgs crossbowLoadedEventArgs)
        {
            var handler = OnCrossbowLoaded;
            if (handler is not null)
            {
                handler(this, crossbowLoadedEventArgs);
            }
        }

        internal void TriggerOnAmmoChanged(AmmoChangedEventArgs ammoChangedEventArgs)
        {
            var handler = OnAmmoChanged;
            if (handler is not null)
            {
                handler(this, ammoChangedEventArgs);
            }
        }

        internal void TriggerOnAmmoHitMonster(AmmoHitMonsterEventArgs ammoHitMonsterEventArgs)
        {
            var handler = OnAmmoHitMonster;
            if (handler is not null)
            {
                handler(this, ammoHitMonsterEventArgs);
            }
        }
        #endregion

        internal bool HandleSpecialAttack(WeaponType weaponType, string specialAttackId, ISpecialAttack specialAttack)
        {
            if (_registeredSpecialAttackMethods.ContainsKey(specialAttackId) is false)
            {
                return false;
            }

            var registeredWeaponTypeFilter = _registeredSpecialAttackFilter[specialAttackId];
            if (registeredWeaponTypeFilter != weaponType && registeredWeaponTypeFilter != WeaponType.Any)
            {
                return false;
            }

            var specialAttackMethod = _registeredSpecialAttackMethods[specialAttackId];
            if (specialAttackMethod(specialAttack) is true)
            {
                _monitor.LogOnce($"Using special attack {specialAttackId}", LogLevel.Trace);
                return true;
            }

            return false;
        }

        internal string GetSpecialAttackName(string specialAttackId)
        {
            if (_registeredSpecialAttackNames.ContainsKey(specialAttackId) is false)
            {
                return null;
            }

            return _registeredSpecialAttackNames[specialAttackId]();
        }

        internal string GetSpecialAttackDescription(string specialAttackId)
        {
            if (_registeredSpecialAttackDescriptions.ContainsKey(specialAttackId) is false)
            {
                return null;
            }

            return _registeredSpecialAttackDescriptions[specialAttackId]();
        }

        internal int GetSpecialAttackCooldown(string specialAttackId)
        {
            if (_registeredSpecialAttackCooldowns.ContainsKey(specialAttackId) is false)
            {
                return 0;
            }

            return _registeredSpecialAttackCooldowns[specialAttackId]();
        }

        private KeyValuePair<bool, string> GenerateResponsePair(bool wasSuccessful, string responseText)
        {
            return new KeyValuePair<bool, string>(wasSuccessful, responseText);
        }

        private string GetSpecialAttackId(IManifest callerManifest, string name)
        {
            return $"{callerManifest.UniqueID}/{name}";
        }

        public KeyValuePair<bool, string> PlaySound(IManifest callerManifest, ISound sound, Vector2 position)
        {
            if (sound is null)
            {
                return GenerateResponsePair(false, "Given ISound is not a valid!");
            }

            if (Toolkit.PlaySound((Sound)sound, callerManifest.UniqueID, position) is false)
            {
                return GenerateResponsePair(false, "Failed to play ISound, see log for details!");
            }

            return GenerateResponsePair(true, $"Played sound {sound.Name} at {position}");
        }

        public KeyValuePair<bool, string> SetChargePercentage(IManifest callerManifest, Slingshot slingshot, float percentage)
        {
            if (Bow.IsValid(slingshot) is false)
            {
                return GenerateResponsePair(false, "Given slingshot object is not a valid Archery.WeaponModel!");
            }

            Bow.SetSlingshotChargeTime(slingshot, percentage);

            return GenerateResponsePair(true, $"Set Archery.WeaponModel's charge percentage to {percentage}");
        }

        public KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, BasicProjectile projectile, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false)
        {
            if (Bow.IsValid(slingshot) is false)
            {
                _monitor.Log("Given slingshot object is not a valid Archery.WeaponModel!", LogLevel.Trace);
                return new KeyValuePair<bool, BasicProjectile>(false, null);
            }

            var arrow = Bow.PerformFire(projectile, null, slingshot, location, who, suppressFiringSound);
            if (arrow is null)
            {
                _monitor.Log("Bow.PerformFire returned null!", LogLevel.Trace);
                return new KeyValuePair<bool, BasicProjectile>(false, null);
            }

            SetChargePercentage(callerManifest, slingshot, 0f);

            return new KeyValuePair<bool, BasicProjectile>(true, arrow);
        }

        public KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, string ammoId, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false)
        {
            if (Bow.IsValid(slingshot) is false)
            {
                _monitor.Log("Given slingshot object is not a valid Archery.WeaponModel!", LogLevel.Trace);
                return new KeyValuePair<bool, BasicProjectile>(false, null);
            }

            var arrow = Bow.PerformFire(Archery.modelManager.GetSpecificModel<AmmoModel>(ammoId), slingshot, location, who, suppressFiringSound);
            if (arrow is null)
            {
                arrow = Bow.PerformFire(slingshot, location, who, suppressFiringSound);

                if (arrow is null)
                {
                    _monitor.Log("Bow.PerformFire returned null!", LogLevel.Trace);
                    return new KeyValuePair<bool, BasicProjectile>(false, null);
                }
            }

            return new KeyValuePair<bool, BasicProjectile>(true, arrow);
        }

        public KeyValuePair<bool, BasicProjectile> PerformFire(IManifest callerManifest, Slingshot slingshot, GameLocation location, Farmer who, bool suppressFiringSound = false)
        {
            return PerformFire(callerManifest, ammoId: null, slingshot, location, who, suppressFiringSound);
        }

        public KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, WeaponType whichWeaponTypeCanUse, Func<string> getDisplayName, Func<string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            _registeredSpecialAttackNames[id] = getDisplayName;
            _registeredSpecialAttackDescriptions[id] = getDescription;
            _registeredSpecialAttackCooldowns[id] = getCooldownMilliseconds;
            _registeredSpecialAttackFilter[id] = whichWeaponTypeCanUse;
            _registeredSpecialAttackMethods[id] = specialAttackHandler;

            _monitor.Log($"The mod {callerManifest.Name} registered a special attack {name} with the type {whichWeaponTypeCanUse}", LogLevel.Info);
            return GenerateResponsePair(true, $"Registered the special attack method for {name} with the type {whichWeaponTypeCanUse}.");
        }

        public KeyValuePair<bool, string> DeregisterSpecialAttack(IManifest callerManifest, string name)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            if (_registeredSpecialAttackMethods.ContainsKey(id) is false)
            {
                return GenerateResponsePair(false, $"There were no registered special attack methods under {id}.");
            }

            _registeredSpecialAttackNames.Remove(id);
            _registeredSpecialAttackDescriptions.Remove(id);
            _registeredSpecialAttackCooldowns.Remove(id);
            _registeredSpecialAttackFilter.Remove(id);
            _registeredSpecialAttackMethods.Remove(id);

            return GenerateResponsePair(true, $"Unregistered the special attack method {id}.");
        }
    }
}
