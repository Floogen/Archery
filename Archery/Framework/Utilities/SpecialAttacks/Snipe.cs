using Archery.Framework.Interfaces.Internal;
using StardewValley;
using StardewValley.Projectiles;

namespace Archery.Framework.Utilities.SpecialAttacks
{
    public class Snipe
    {
        internal static bool HandleSpecialAttack(ISpecialAttack specialAttack)
        {
            var slingshot = specialAttack.Slingshot;

            var weaponDataResponse = Archery.internalApi.GetWeaponData(Archery.manifest, slingshot);
            if (weaponDataResponse.Key is false)
            {
                return false;
            }
            else if (weaponDataResponse.Value.WeaponType is WeaponType.Crossbow && weaponDataResponse.Value.AmmoInMagazine == 0)
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
                var firedResponse = Archery.internalApi.PerformFire(Archery.manifest, slingshot, specialAttack.Location, specialAttack.Farmer);
                if (firedResponse.Key is true && firedResponse.Value is BasicProjectile projectile)
                {
                    // Get the internal projectile data
                    var dataResponse = Archery.internalApi.GetProjectileData(Archery.manifest, projectile);
                    if (dataResponse.Key is false)
                    {
                        return false;
                    }

                    var projectileData = dataResponse.Value;

                    // Double the velocity
                    projectileData.Velocity *= 2;

                    // Guarantee critical hit
                    projectileData.CriticalChance = 1f;

                    // Set the internal projectile data
                    Archery.internalApi.SetProjectileData(Archery.manifest, projectile, projectileData);
                }

                return false;
            }

            return true;
        }
    }
}
