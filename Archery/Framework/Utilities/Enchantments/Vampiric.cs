﻿using Archery.Framework.Interfaces.Internal;
using StardewValley;
using System;
using System.Collections.Generic;

namespace Archery.Framework.Utilities.Enchantments
{
    public class Vampiric
    {
        private static float _defaultPercentage = 0.1f;

        internal static string GetDescription(List<object> arguments)
        {
            return $"Restores {GetPercentage(arguments)}% of the damage done as health.";
        }

        internal static bool HandleEnchantment(IEnchantment enchantment)
        {
            if (enchantment.Farmer is null || enchantment.Monster is null || enchantment.DamageDone is null)
            {
                return false;
            }
            var farmer = enchantment.Farmer;

            // Take 10% of the damage done and restore the farmer's health
            var amountToHeal = enchantment.DamageDone.Value / GetPercentage(enchantment.Arguments);
            farmer.health = amountToHeal + farmer.health > farmer.maxHealth ? farmer.maxHealth : amountToHeal + farmer.health;

            return false;
        }

        private static int GetPercentage(List<object> arguments)
        {
            var percentage = _defaultPercentage;
            if (arguments is not null && arguments.Count > 0)
            {
                try
                {
                    percentage = (float)arguments[0];
                }
                catch (Exception ex)
                {
                    Archery.monitor.LogOnce($"Failed to process percentage argument for Archery/PeacefulEnd.Archery/Vampiric! See the log for details.", StardewModdingAPI.LogLevel.Error);
                    Archery.monitor.LogOnce($"Failed to process percentage argument for Archery/PeacefulEnd.Archery/Vampiric:\n{ex}", StardewModdingAPI.LogLevel.Trace);
                }
            }
            return (int)(Utility.Clamp(percentage, 0, 1) * 100);
        }
    }
}
