namespace Archery.Framework.Models.Crafting
{
    public class IngredientModel
    {
        public object Id { get; set; }
        public int Amount { get; set; }

        // When SDV v1.6 is released, we'll want to support QualifiedItemIds
        internal int? GetObjectId()
        {
            if (Id is string && int.TryParse((string)Id, out var id))
            {
                return id;
            }
            else if (Id is int)
            {
                return (int)Id;
            }

            return null;
        }
    }
}
