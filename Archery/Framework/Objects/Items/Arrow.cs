using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects.Items
{
    internal class Arrow : InstancedObject
    {
        private const int ARROW_BASE_ID = 26;

        public static Object CreateInstance(AmmoModel ammoModel)
        {
            var arrow = new Object(ARROW_BASE_ID, 1);
            arrow.modData[ModDataKeys.AMMO_FLAG] = ammoModel.Id;

            return arrow;
        }
    }
}
