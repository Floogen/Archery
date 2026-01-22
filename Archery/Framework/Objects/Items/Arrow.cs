using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects.Items
{
    internal class Arrow : InstancedObject
    {
        public static Object CreateInstance(AmmoModel ammoModel, int stackCount = 1)
        {
            return ItemRegistry.Create<Object>(ammoModel.Id);
        }
    }
}
