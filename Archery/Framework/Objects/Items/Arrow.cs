using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects.Items
{
    internal class Arrow
    {
        private const int ARROW_BASE_ID = 26;

        public static Object CreateInstance(AmmoModel ammoModel)
        {
            var arrow = new Object(ARROW_BASE_ID, 1);
            arrow.modData[ModDataKeys.ARROW_ITEM_FLAG] = ammoModel.Id;

            return arrow;
        }

        public static bool IsValid(Item item)
        {
            if (item is not null && item.modData.ContainsKey(ModDataKeys.ARROW_ITEM_FLAG))
            {
                return true;
            }

            return false;
        }

        public static string GetInternalId(Item item)
        {
            if (IsValid(item) is false || item.modData.TryGetValue(ModDataKeys.ARROW_ITEM_FLAG, out var id) is false)
            {
                return String.Empty;
            }

            return id;
        }

        internal static string GetName(Object instance)
        {
            // TODO: Implement grabbing the arrow name from content pack
            return "DEFAULT NAME";
        }

        internal static string GetDescription(Object instance)
        {
            // TODO: Implement grabbing the arrow description from content pack
            return "DEFAULT DESCRIPTION";
        }
    }
}
