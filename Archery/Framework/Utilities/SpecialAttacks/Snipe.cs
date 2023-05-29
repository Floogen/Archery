using Archery.Framework.Interfaces.Internal;
using StardewValley;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Utilities.SpecialAttacks
{
    public class Snipe
    {
        private static float _defaultCriticalChance = 1f;
        private static int _defaultCooldownInMilliseconds = 3000;

        internal static string GetDescription(List<object> arguments)
        {
            string criticalChanceStatement = string.Empty;

            var criticalChance = GetCriticalChance(arguments);
            if (criticalChance < 1f && criticalChance > 0f)
            {
                criticalChanceStatement = $" Has an increased chance of landing a critical hit.";
            }
            else if (criticalChance >= 1f)
            {
                criticalChanceStatement = " Guaranteed to critical hit.";
            }

            return $"Instantly fires the projectile with increased speed.{criticalChanceStatement}";
        }

        private static float GetCriticalChance(List<object> arguments)
        {
            var chance = _defaultCriticalChance;
            if (arguments is not null && arguments.Count > 0)
            {
                try
                {
                    chance = Convert.ToSingle(arguments[0]);
                }
                catch (Exception ex)
                {
                    Archery.monitor.LogOnce($"Failed to process critical chance argument for PeacefulEnd.Archery/Snipe! See the log for details.", StardewModdingAPI.LogLevel.Error);
                    Archery.monitor.LogOnce($"Failed to process critical chance argument for PeacefulEnd.Archery/Snipe:\n{ex}", StardewModdingAPI.LogLevel.Trace);
                }
            }

            return chance;
        }

        internal static int GetCooldown(List<object> arguments)
        {
            var duration = _defaultCooldownInMilliseconds;
            if (arguments is not null && arguments.Count > 1)
            {
                try
                {
                    duration = Convert.ToInt32(arguments[1]);
                }
                catch (Exception ex)
                {
                    Archery.monitor.LogOnce($"Failed to process cooldown argument for PeacefulEnd.Archery/Snipe! See the log for details.", StardewModdingAPI.LogLevel.Error);
                    Archery.monitor.LogOnce($"Failed to process cooldown argument for PeacefulEnd.Archery/Snipe:\n{ex}", StardewModdingAPI.LogLevel.Trace);
                }
            }

            return duration;
        }

        internal static bool HandleSpecialAttack(ISpecialAttack specialAttack)
        {
            var slingshot = specialAttack.Slingshot;

            var weaponData = Archery.internalApi.GetWeaponData(Archery.manifest, slingshot);
            if (weaponData is null)
            {
                return false;
            }
            else if (weaponData.WeaponType is WeaponType.Crossbow && weaponData.AmmoInMagazine == 0)
            {
                Game1.showRedMessage("The crossbow must be loaded before using the special attack!");
                return false;
            }

            var currentChargeTime = slingshot.GetSlingshotChargeTime();
            if (currentChargeTime < 0.7f)
            {
                Archery.internalApi.SetChargePercentage(Archery.manifest, slingshot, 0.7f);
            }
            else if (currentChargeTime >= 1f)
            {
                var projectileObject = Archery.internalApi.PerformFire(Archery.manifest, slingshot, specialAttack.Location, specialAttack.Farmer);
                if (projectileObject is not null && projectileObject is BasicProjectile projectile)
                {
                    // Get the internal projectile data
                    var projectileData = Archery.internalApi.GetProjectileData(Archery.manifest, projectile);
                    if (projectileData is null)
                    {
                        return false;
                    }

                    // Double the velocity
                    projectileData.Velocity *= 2;

                    // Guarantee critical hit
                    projectileData.CriticalChance = GetCriticalChance(specialAttack.Arguments);

                    // Set the internal projectile data
                    Archery.internalApi.SetProjectileData(Archery.manifest, projectile, projectileData);
                }

                return false;
            }

            return true;
        }
    }
}
