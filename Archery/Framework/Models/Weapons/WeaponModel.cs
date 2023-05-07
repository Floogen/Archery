using Archery.Framework.Models.Enums;
using Archery.Framework.Models.Generic;
using StardewModdingAPI;
using System;

namespace Archery.Framework.Models.Weapons
{
    public class WeaponModel : BaseModel
    {
        public WeaponType Type { get; set; }

        public RandomRange DamageRange { get; set; }
        public float ProjectileSpeed { get; set; }
        public float Knockback { get; set; }
        public float ChargeTimeRequiredMilliseconds { get; set; } = 1000f;
        public float ConsumeAmmoChance { get; set; } = 1f;

        public string InternalAmmoId { get; set; }

        public Sound StartChargingSound { get; set; }
        public Sound FinishChargingSound { get; set; }
        public Sound FiringSound { get; set; }

        internal bool IsValidAmmoType(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Arrow:
                    return Type is WeaponType.Bow;
                case AmmoType.Bolt:
                    return Type is WeaponType.Crossbow;
                case AmmoType.Pellet:
                    return Type is WeaponType.Slingshot;
                default:
                    return false;
            }
        }

        internal bool UsesInternalAmmo()
        {
            return String.IsNullOrEmpty(InternalAmmoId) is false && Archery.modelManager.DoesModelExist<AmmoModel>(InternalAmmoId);
        }

        internal bool ShouldAlwaysConsumeAmmo()
        {
            return ConsumeAmmoChance >= 1f;
        }

        internal override void SetId(IContentPack contentPack)
        {
            Id = String.Concat(contentPack.Manifest.UniqueID, "/", Type, "/", Name);

            base.SetId(contentPack);
        }
    }
}
