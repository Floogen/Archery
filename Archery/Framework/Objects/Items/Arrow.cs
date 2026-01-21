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

        public static Object CreateRecipe(AmmoModel ammoModel)
        {
            var recipe = CreateInstance(ammoModel);
            recipe.modData[ModDataKeys.RECIPE_FLAG] = true.ToString();

            return recipe;
        }
    }
}
