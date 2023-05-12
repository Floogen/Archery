﻿using Archery.Framework.Objects.Weapons;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using static Archery.Framework.Interfaces.Internal.IApi;

namespace Archery.Framework.Interfaces.Internal
{
    public interface IApi
    {
        KeyValuePair<bool, string> SetChargePercentage(IManifest callerManifest, Slingshot slingshot, float percentage);
        KeyValuePair<bool, string> PerformFire(IManifest callerManifest, Slingshot slingshot, GameLocation location, Farmer who);
        KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, Func<string> getDisplayName, Func<string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler);
        KeyValuePair<bool, string> DeregisterSpecialAttack(IManifest callerManifest, string name);

        public interface ISpecialAttack
        {
            public Slingshot Slingshot { get; init; }
            public GameTime Time { get; init; }
            public GameLocation Location { get; init; }
            public Farmer Farmer { get; init; }

            public List<object> Arguments { get; init; }
        }
    }

    public class Api : IApi
    {
        private IMonitor _monitor;
        private Dictionary<string, Func<ISpecialAttack, bool>> _registeredSpecialAttackMethods;
        private Dictionary<string, Func<string>> _registeredSpecialAttackNames;
        private Dictionary<string, Func<string>> _registeredSpecialAttackDescriptions;
        private Dictionary<string, Func<int>> _registeredSpecialAttackCooldowns;

        internal Api(IMonitor monitor)
        {
            _monitor = monitor;
            _registeredSpecialAttackMethods = new Dictionary<string, Func<ISpecialAttack, bool>>();
            _registeredSpecialAttackNames = new Dictionary<string, Func<string>>();
            _registeredSpecialAttackDescriptions = new Dictionary<string, Func<string>>();
            _registeredSpecialAttackCooldowns = new Dictionary<string, Func<int>>();
        }

        internal bool HandleSpecialAttack(string specialAttackId, ISpecialAttack specialAttack)
        {
            if (_registeredSpecialAttackMethods.ContainsKey(specialAttackId) is false)
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

        public KeyValuePair<bool, string> SetChargePercentage(IManifest callerManifest, Slingshot slingshot, float percentage)
        {
            if (Bow.IsValid(slingshot) is false)
            {
                return GenerateResponsePair(false, "Given slingshot object is not a valid Archery.WeaponModel!");
            }

            Bow.SetSlingshotChargeTime(slingshot, percentage);

            return GenerateResponsePair(true, $"Set Archery.WeaponModel's charge percentage to {percentage}");
        }

        public KeyValuePair<bool, string> PerformFire(IManifest callerManifest, Slingshot slingshot, GameLocation location, Farmer who)
        {
            if (Bow.IsValid(slingshot) is false)
            {
                return GenerateResponsePair(false, "Given slingshot object is not a valid Archery.WeaponModel!");
            }

            if (Bow.PerformFire(slingshot, location, who) is false)
            {
                return GenerateResponsePair(false, "Bow.PerformFire returned false.");
            }
            SetChargePercentage(callerManifest, slingshot, 0f);

            return GenerateResponsePair(true, "Bow.PerformFire returned true.");
        }

        public KeyValuePair<bool, string> RegisterSpecialAttack(IManifest callerManifest, string name, Func<string> getDisplayName, Func<string> getDescription, Func<int> getCooldownMilliseconds, Func<ISpecialAttack, bool> specialAttackHandler)
        {
            string id = GetSpecialAttackId(callerManifest, name);
            _registeredSpecialAttackNames[id] = getDisplayName;
            _registeredSpecialAttackDescriptions[id] = getDescription;
            _registeredSpecialAttackCooldowns[id] = getCooldownMilliseconds;
            _registeredSpecialAttackMethods[id] = specialAttackHandler;

            _monitor.Log($"The mod {callerManifest.Name} registered a special attack: {name}", LogLevel.Info);
            return GenerateResponsePair(true, $"Registered the special attack method for {name}.");
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
            _registeredSpecialAttackMethods.Remove(id);

            return GenerateResponsePair(true, $"Unregistered the special attack method {id}.");
        }
    }
}