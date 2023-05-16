using Archery.Framework.Models.Crafting;
using Archery.Framework.Models.Display;
using Archery.Framework.Models.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Archery.Framework.Models
{
    public class BaseModel
    {
        public string DisplayName { get { return string.IsNullOrEmpty(_displayName) ? Name : _displayName; } set { _displayName = value; } }
        private string _displayName;
        public string Name { get; set; }
        public string Description { get; set; }

        // Used additively by both weapons and ammo
        public float CriticalChance { get; set; }
        public float CriticalDamageMultiplier { get; set; } = 1f;

        public ItemSpriteModel Icon { get; set; }
        public DirectionalSpriteModel DirectionalSprites { get; set; }

        public RecipeModel Recipe { get; set; }
        public ShopModel Shop { get; set; }
        public List<string> Filter { get; set; }

        internal string Id { get; set; }
        internal IContentPack ContentPack { get; set; }
        internal Texture2D Texture { get; set; }
        internal string TexturePath { get; set; }
        internal ITranslationHelper Translations { get; set; }


        internal bool IsFilterDefined()
        {
            if (Filter is null || Filter.Count == 0)
            {
                return false;
            }

            return true;
        }

        internal bool IsWithinFilter(string id)
        {
            if (IsFilterDefined() is false)
            {
                return false;
            }

            foreach (var filterId in Filter)
            {
                var regex = new Regex(filterId);
                if (regex.IsMatch(id))
                {
                    return true;
                }
            }

            return false;
        }

        internal Direction GetSpriteDirectionFromGivenDirection(Farmer who)
        {
            if (who is null || DirectionalSprites is null)
            {
                return Direction.Any;
            }

            return DirectionalSprites.GetActualDirection((Direction)who.FacingDirection);
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
            foreach (var sprite in sprites.Where(s => s is not null))
            {
                if (sprite.AreConditionsValid(who))
                {
                    return sprite;
                }
            }
            Archery.conditionManager.Reset(sprites);

            return sprites.FirstOrDefault();
        }

        internal virtual void SetId(IContentPack contentPack)
        {
            if (Recipe is not null)
            {
                Recipe.ParentId = Id;
            }
        }

        internal string GetTranslation(string text)
        {
            if (Translations is not null && Translations.GetTranslations().Any(t => t.Key == text))
            {
                return Translations.Get(text);
            }

            return text;
        }
    }
}
