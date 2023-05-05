using Archery.Framework.Models.Crafting;
using Archery.Framework.Models.Display;
using Archery.Framework.Models.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Models
{
    public class BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ItemSpriteModel Icon { get; set; }
        public DirectionalSpriteModel DirectionalSprites { get; set; }

        public RecipeModel Recipe { get; set; }

        internal string Id { get; set; }
        internal IContentPack ContentPack { get; set; }
        internal Texture2D Texture { get; set; }
        internal string TexturePath { get; set; }

        internal bool CanCraftThis(Farmer who, List<Item> items)
        {
            if (Recipe is null || Recipe.IsValid() is false)
            {
                return false;
            }

            return Recipe.HasRequiredIngredients(items);
        }

        internal WorldSpriteModel GetSpriteFromDirection(Farmer who)
        {
            if (who is null || DirectionalSprites is null)
            {
                return null;
            }

            var spritesInGivenDirection = DirectionalSprites.GetSpritesFromDirection((Direction)who.FacingDirection);
            if (spritesInGivenDirection.Count == 0)
            {
                spritesInGivenDirection = DirectionalSprites.GetSpritesFromDirection(Direction.Any);
            }

            return GetValidOrDefaultSprite(who, spritesInGivenDirection);
        }

        internal WorldSpriteModel GetValidOrDefaultSprite(Farmer who, List<WorldSpriteModel> sprites)
        {
            foreach (var sprite in sprites)
            {
                if (sprite.AreConditionsValid(who))
                {
                    return sprite;
                }
            }

            return sprites.FirstOrDefault();
        }

        internal virtual void SetId(IContentPack contentPack)
        {

        }
    }
}
