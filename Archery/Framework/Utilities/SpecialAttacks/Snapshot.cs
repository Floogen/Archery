using Archery.Framework.Interfaces.Internal;

namespace Archery.Framework.Utilities.SpecialAttacks
{
    public class Snapshot
    {
        internal static bool HandleSpecialAttack(ISpecialAttack specialAttack)
        {
            var slingshot = specialAttack.Slingshot;

            int shotsFired = 0;
            if (slingshot.modData.TryGetValue(ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT, out string rawShotsFired) is false || int.TryParse(rawShotsFired, out shotsFired) is false)
            {
                slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = shotsFired.ToString();
            }

            var currentChargeTime = slingshot.GetSlingshotChargeTime();
            if (currentChargeTime < 0.5f)
            {
                Archery.internalApi.SetChargePercentage(Archery.manifest, slingshot, 0.5f);
            }
            else if (currentChargeTime >= 1f)
            {
                Archery.internalApi.PerformFire(Archery.manifest, slingshot, specialAttack.Location, specialAttack.Farmer);
                shotsFired++;
            }
            slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = shotsFired.ToString();

            if (shotsFired >= 2)
            {
                slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = 0.ToString();

                return false;
            }

            return true;
        }
    }
}
