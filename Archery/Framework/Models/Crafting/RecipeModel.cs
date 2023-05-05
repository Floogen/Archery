using Archery.Framework.Models.Generic;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Models.Crafting
{
    public class RecipeModel
    {
        public List<IngredientModel> Ingredients { get; set; }
        public int OutputAmount { get; set; }
        public RandomRange ExtraOutputRange { get; set; }
        public string UnlockCondition { get; set; }

        internal bool IsValid()
        {
            if (OutputAmount < 0)
            {
                return false;
            }

            return true;
        }

        internal int GetOutputStack()
        {
            return OutputAmount + (ExtraOutputRange is null ? 0 : ExtraOutputRange.Get(Game1.random));
        }

        internal List<IngredientModel> GetValidIngredients()
        {
            return Ingredients is null ? new List<IngredientModel>() : Ingredients.Where(i => i.GetObjectId() is not null).GroupBy(i => i.Id).Select(i => i.First()).ToList();
        }

        internal bool HasRequiredIngredients(List<Item> items)
        {
            var getActualIngredients = GetValidIngredients();
            if (getActualIngredients.Count == 0)
            {
                return true;
            }

            foreach (var ingredient in getActualIngredients)
            {
                var id = ingredient.GetObjectId();
                if (id is null)
                {
                    continue;
                }

                if (items.Any(i => i.ParentSheetIndex == id && i.Stack >= ingredient.Amount) is false)
                {
                    return false;
                }
            }

            return true;
        }

        internal bool HasRequirements(Farmer who)
        {
            return GameStateQuery.CheckConditions(UnlockCondition, target_farmer: who);
        }
    }
}
