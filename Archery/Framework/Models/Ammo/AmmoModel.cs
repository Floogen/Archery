using Archery.Framework.Models.Ammo;
using Archery.Framework.Models.Display;
using Archery.Framework.Models.Enums;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }
        public DebrisModel Debris { get; set; }
        public List<WorldSpriteModel> Sprite { get; set; }

        public int BaseDamage { get; set; }
        public float BreakChance { get; set; } = 1.0f;

        internal WorldSpriteModel GetSpriteFromDirection(int direction)
        {
            if (Sprite is null || Sprite.Count == 0)
            {
                return null;
            }

            var selectedSprite = Sprite.FirstOrDefault(s => (int)s.Direction == direction);
            if (selectedSprite is null)
            {
                selectedSprite = Sprite.FirstOrDefault(s => s.Direction == Direction.All);
            }

            return selectedSprite;
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
