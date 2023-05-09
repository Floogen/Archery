using Archery.Framework.Models.Enums;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects.Items
{
    internal class Arrow : InstancedObject
    {
        private const int ARROW_BASE_ID = 590;
        private const int BOLT_BASE_ID = 669;

        public static Object CreateInstance(AmmoModel ammoModel, int stackCount = 1)
        {
            var arrow = new Object(ammoModel.Type is AmmoType.Arrow ? ARROW_BASE_ID : BOLT_BASE_ID, stackCount);
            arrow.modData[ModDataKeys.AMMO_FLAG] = ammoModel.Id;

            return arrow;
        }

        public static Object CreateRecipe(AmmoModel ammoModel)
        {
            var recipe = CreateInstance(ammoModel);
            recipe.modData[ModDataKeys.RECIPE_FLAG] = true.ToString();

            return recipe;
        }
    }
}
