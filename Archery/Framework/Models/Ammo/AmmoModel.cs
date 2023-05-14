using Archery.Framework.Interfaces.Internal;
using Archery.Framework.Models.Ammo;
using Archery.Framework.Models.Display;
using Archery.Framework.Models.Generic;
using Microsoft.Xna.Framework;
using SolidFoundations.Framework.Models.ContentPack;
using StardewModdingAPI;
using System;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }
        public Rectangle? CollisionBox { get; set; }

        public DebrisModel Debris { get; set; }
        public ItemSpriteModel ProjectileSprite { get; set; }
        public ArrowTailModel Tail { get; set; }
        public ExplosionModel Explosion { get; set; }

        public int BaseDamage { get; set; }
        public float BreakChance { get; set; } = 1.0f;
        public int MaxTravelDistance { get; set; } = -1;

        public Sound ImpactSound { get; set; }
        public Light Light { get; set; }
        public RandomRange BounceCountRange { get; set; }

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

            base.SetId(contentPack);
        }
    }
}
