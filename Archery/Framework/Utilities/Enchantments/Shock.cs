using Archery.Framework.Interfaces.Internal;

namespace Archery.Framework.Utilities.Enchantments
{
    public class Shock
    {
        internal static bool HandleEnchantment(IEnchantment enchantment)
        {
            if (enchantment.Farmer is null || enchantment.Monster is null)
            {
                return false;
            }
            var monster = enchantment.Monster;

            // Stun the monster for 1 second
            monster.stunTime += 1000;

            return false;
        }
    }
}
