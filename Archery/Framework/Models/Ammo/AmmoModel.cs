using Archery.Framework.Models.Enums;
using StardewModdingAPI;
using System;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }

        public int BaseDamage { get; set; }

        internal override void SetId(IContentPack contentPack)
        {
            Id = String.Concat(contentPack.Manifest.UniqueID, "/", Type, "/", Name);
        }
    }
}
