using Archery.Framework.Interfaces.Internal;

namespace Archery.Framework.Utilities.Enchantments
{
    public class Vampiric
    {
        internal static bool HandleEnchantment(IEnchantment enchantment)
        {
            if (enchantment.Farmer is null || enchantment.Monster is null || enchantment.DamageDone is null)
            {
                return false;
            }
            var farmer = enchantment.Farmer;

            // Take 10% of the damage done and restore the farmer's health
            var amountToHeal = enchantment.DamageDone.Value / 10;
            farmer.health = amountToHeal + farmer.health > farmer.maxHealth ? farmer.maxHealth : amountToHeal + farmer.health;

            return false;
        }
    }
}
