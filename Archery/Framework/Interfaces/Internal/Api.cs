using Archery.Framework.Models.Generic;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Projectiles;
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
        internal static IModHelper _helper;
        private Dictionary<string, Func<ISpecialAttack, bool>> _registeredSpecialAttackMethods;
        private Dictionary<string, SpecialAttack> _registeredSpecialAttackData;
        private Dictionary<string, Func<IEnchantment, bool>> _registeredEnchantmentMethods;
        private Dictionary<string, Enchantment> _registeredEnchantmentData;

        public event EventHandler<WeaponFiredEventArgs> OnWeaponFired;
        public event EventHandler<WeaponChargeEventArgs> OnWeaponCharging;
        public event EventHandler<WeaponChargeEventArgs> OnWeaponCharged;
        public event EventHandler<CrossbowLoadedEventArgs> OnCrossbowLoaded;
        public event EventHandler<AmmoChangedEventArgs> OnAmmoChanged;
        public event EventHandler<AmmoHitMonsterEventArgs> OnAmmoHitMonster;


        internal Api(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
            _registeredSpecialAttackMethods = new Dictionary<string, Func<ISpecialAttack, bool>>();
            _registeredSpecialAttackData = new Dictionary<string, SpecialAttack>();
            _registeredEnchantmentMethods = new Dictionary<string, Func<IEnchantment, bool>>();
            _registeredEnchantmentData = new Dictionary<string, Enchantment>();
        }

        private KeyValuePair<bool, string> GenerateResponsePair(bool wasSuccessful, string responseText)
        {
            return new KeyValuePair<bool, string>(wasSuccessful, responseText);
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

        #region Special attacks internals
        internal bool HandleSpecialAttack(WeaponType weaponType, string specialAttackId, ISpecialAttack specialAttack)
        {
            if (_registeredSpecialAttackMethods.ContainsKey(specialAttackId) is false)
            {
                return false;
            }

            var registeredWeaponTypeFilter = _registeredSpecialAttackData[specialAttackId].WeaponType;
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
            if (_registeredSpecialAttackData.ContainsKey(specialAttackId) is false)
            {
                return null;
            }
            var specialAttackData = _registeredSpecialAttackData[specialAttackId];

            return specialAttackData.GetName(specialAttackData.Arguments);
        }

        internal string GetSpecialAttackDescription(string specialAttackId)
        {
            if (_registeredSpecialAttackData.ContainsKey(specialAttackId) is false)
            {
                return null;
            }
            var specialAttackData = _registeredSpecialAttackData[specialAttackId];

            return specialAttackData.GetDescription(specialAttackData.Arguments);
        }

        internal int GetSpecialAttackCooldown(string specialAttackId)
        {
            if (_registeredSpecialAttackData.ContainsKey(specialAttackId) is false)
            {
                return 0;
            }

            return _registeredSpecialAttackData[specialAttackId].GetCooldownInMilliseconds();
        }

        private string GetSpecialAttackId(IManifest callerManifest, string name)
        {
            return $"{callerManifest.UniqueID}/{name}";
        }
        #endregion

        #region Enchantment internals
        internal bool HandleEnchantment(AmmoType ammoType, string enchantmentId, IEnchantment enchantment)
        {
            if (_registeredEnchantmentMethods.ContainsKey(enchantmentId) is false)
            {
                return false;
            }

            var registeredAmmoTypeFilter = _registeredEnchantmentData[enchantmentId].AmmoType;
            if (registeredAmmoTypeFilter != ammoType && registeredAmmoTypeFilter != AmmoType.Any)
            {
                return false;
            }

            var specialAttackMethod = _registeredEnchantmentMethods[enchantmentId];
            if (specialAttackMethod(enchantment) is true)
            {
                _monitor.LogOnce($"Using special attack {enchantmentId}", LogLevel.Trace);
                return true;
            }

            return false;
        }

        internal string GetEnchantmentName(string enchantmentId)
        {
            if (_registeredEnchantmentData.ContainsKey(enchantmentId) is false)
            {
                return null;
            }
            var enchantmentData = _registeredEnchantmentData[enchantmentId];

            return enchantmentData.GetName(enchantmentData.Arguments);
        }

        internal string GetEnchantmentDescription(string enchantmentId)
        {
            if (_registeredEnchantmentData.ContainsKey(enchantmentId) is false)
            {
                return null;
            }
            var enchantmentData = _registeredEnchantmentData[enchantmentId];

            return enchantmentData.GetDescription(enchantmentData.Arguments);
        }

        internal TriggerType GetEnchantmentTriggerType(string enchantmentId)
        {
            if (_registeredEnchantmentData.ContainsKey(enchantmentId) is false)
            {
                return TriggerType.Unknown;
            }

            return _registeredEnchantmentData[enchantmentId].TriggerType;
        }

        private string GetEnchantmentId(IManifest callerManifest, string name)
        {
            return $"{callerManifest.UniqueID}/{name}";
        }
        #endregion

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

        public KeyValuePair<bool, IWeaponData> GetWeaponData(IManifest callerManifest, Slingshot slingshot)
        {
            if (slingshot is null)
            {
                return new KeyValuePair<bool, IWeaponData>(false, null);
            }

            if (Bow.IsValid(slingshot) is false)
            {
                return new KeyValuePair<bool, IWeaponData>(false, null);
            }

            return new KeyValuePair<bool, IWeaponData>(true, Bow.GetData(slingshot));
        }

        public KeyValuePair<bool, IProjectileData> GetProjectileData(IManifest callerManifest, BasicProjectile projectile)
        {
            if (projectile is null)
            {
                return new KeyValuePair<bool, IProjectileData>(false, null);
            }

            if (projectile is not ArrowProjectile arrowProjectile)
            {
                return new KeyValuePair<bool, IProjectileData>(false, null);
            }

            return new KeyValuePair<bool, IProjectileData>(true, arrowProjectile.GetData());
        }

        public KeyValuePair<bool, string> SetProjectileData(IManifest callerManifest, BasicProjectile projectile, IProjectileData data)
        {
            if (projectile is null)
            {
                return GenerateResponsePair(false, "Given projectile is null!");
            }

            if (projectile is not ArrowProjectile arrowProjectile)
            {
                return GenerateResponsePair(false, "Given projectile is not a ArrowProjectile!");
            }

            arrowProjectile.Override(data);

            return GenerateResponsePair(true, $"Overrode the projectile with the given IProjectile");
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

        public KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, WeaponType whichWeaponTypeCanUse, Func<List<object>, string> getDisplayName, Func<List<object>, string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            _registeredSpecialAttackMethods[id] = specialAttackHandler;
            _registeredSpecialAttackData[id] = new SpecialAttack()
            {
                WeaponType = whichWeaponTypeCanUse,
                GetName = getDisplayName,
                GetDescription = getDescription,
                GetCooldownInMilliseconds = getCooldownMilliseconds
            };

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

            _registeredSpecialAttackMethods.Remove(id);
            _registeredSpecialAttackData.Remove(id);

            return GenerateResponsePair(true, $"Unregistered the special attack method {id}.");
        }

        public KeyValuePair<bool, string> RegisterEnchantment(IManifest callerManifest, string name, AmmoType whichAmmoTypeCanUse, TriggerType triggerType, Func<List<object>, string> getDisplayName, Func<List<object>, string> getDescription, Func<IEnchantment, bool> enchantmentHandler)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            _registeredEnchantmentMethods[id] = enchantmentHandler;
            _registeredEnchantmentData[id] = new Enchantment()
            {
                AmmoType = whichAmmoTypeCanUse,
                TriggerType = triggerType,
                GetName = getDisplayName,
                GetDescription = getDescription
            };

            _monitor.Log($"The mod {callerManifest.Name} registered an enchantment {name} with the type {whichAmmoTypeCanUse}", LogLevel.Info);
            return GenerateResponsePair(true, $"Registered the enchantment method for {name} with the type {whichAmmoTypeCanUse}.");
        }

        public KeyValuePair<bool, string> DeregisterEnchantment(IManifest callerManifest, string name)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            if (_registeredEnchantmentMethods.ContainsKey(id) is false)
            {
                return GenerateResponsePair(false, $"There were no registered enchantment methods under {id}.");
            }

            _registeredEnchantmentMethods.Remove(id);
            _registeredEnchantmentData.Remove(id);

            return GenerateResponsePair(true, $"Unregistered the enchantment method {id}.");
        }
    }
}
