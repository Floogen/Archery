namespace Archery.Framework.Models.Crafting
{
    public class IngredientModel
    {
        public string Id { get; set; }
        public int Amount { get; set; }

        internal bool IsValid()
        {
            return GetObjectId() is not null && Amount > 0;
        }

        // When SDV v1.6 is released, we'll want to support QualifiedItemIds
        internal int? GetObjectId()
        {
            int? actualId = null;
            if (int.TryParse(Id, out int parsedId))
            {
                actualId = parsedId;
            }
            else if (Archery.apiManager.GetJsonAssetsApi() is not null)
            {
                int jsonObjectId = Archery.apiManager.GetJsonAssetsApi().GetObjectId(Id);
                if (jsonObjectId != -1)
                {
                    actualId = jsonObjectId;
                }
            }

            return actualId;
        }
    }
}