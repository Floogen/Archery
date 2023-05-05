namespace Archery.Framework.Models.Crafting
{
    public class IngredientModel
    {
        public int Id { get; set; }
        public int Amount { get; set; }

        internal bool IsValid()
        {
            return GetObjectId() is not null && Amount > 0;
        }

        // When SDV v1.6 is released, we'll want to support QualifiedItemIds
        internal int? GetObjectId()
        {
            return Id;
        }
    }
}
