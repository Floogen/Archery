using Archery.Framework.Models.Ammo;
using Archery.Framework.Models.Enums;
using StardewModdingAPI;
using System;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }
        public DebrisModel Debris { get; set; }

        public int BaseDamage { get; set; }
        public float BreakChance { get; set; } = 1.0f;

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
