using Archery.Framework.Models.Ammo;
using Archery.Framework.Models.Display;
using Archery.Framework.Models.Enums;
using StardewModdingAPI;
using System;
using System.Linq;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }
        public DebrisModel Debris { get; set; }

        public int BaseDamage { get; set; }
        public float BreakChance { get; set; } = 1.0f;

        internal WorldSpriteModel GetSpriteFromDirection(int direction)
        {
            if (DirectionalSprites is null)
            {
                return null;
            }

            var spritesInGivenDirection = DirectionalSprites.GetSpritesFromDirection((Direction)direction);
            if (spritesInGivenDirection.Count == 0)
            {
                spritesInGivenDirection = DirectionalSprites.GetSpritesFromDirection(Direction.Any);
            }

            return spritesInGivenDirection.FirstOrDefault();
        }

        internal bool CanBreak()
        {
            return BreakChance > 0;
        }

        internal bool ShouldAlwaysBreak()
        {
            return BreakChance >= 1f;
        }

        internal override void SetId(IContentPack contentPack)
        {
            Id = String.Concat(contentPack.Manifest.UniqueID, "/", Type, "/", Name);
        }
    }
}
