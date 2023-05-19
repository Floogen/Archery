using Archery.Framework.Interfaces.Internal;
using Archery.Framework.Models.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Archery.Framework.Models.Weapons
{
    public class WeaponModel : BaseModel
    {
        public WeaponType Type { get; set; }

        public RandomRange DamageRange { get; set; }
        public float ProjectileSpeed { get; set; } = 1f;
        public float Knockback { get; set; } = 1f;
        public float ChargeTimeRequiredMilliseconds { get; set; } = 1000f;
        public float ConsumeAmmoChance { get; set; } = 1f;

        public string InternalAmmoId { get; set; }

        // Only used by WeaponType.Crossbow
        public int MagazineSize { get; set; } = 1;

        public Sound StartChargingSound { get; set; }
        public Sound FinishChargingSound { get; set; }
        public Sound FiringSound { get; set; }

        public SpecialAttackModel SpecialAttack { get; set; }

        internal Texture2D ArmsTexture { get; set; }
        internal Texture2D RecoloredArmsTexture { get; set; }

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
                    return true;
            }
        }

        internal Texture2D GetArmsTexture()
        {
            if (ArmsTexture is null)
            {
                switch (Type)
                {
                    case WeaponType.Crossbow:
                        return Archery.assetManager.recoloredCrossbowArmsTexture;
                    default:
                    case WeaponType.Bow:
                        return Archery.assetManager.recoloredArmsTexture;
                }
            }

            return RecoloredArmsTexture;
        }

        internal bool UsesInternalAmmo()
        {
            return string.IsNullOrEmpty(InternalAmmoId) is false && Archery.modelManager.DoesModelExist<AmmoModel>(InternalAmmoId);
        }

        internal bool ShouldAlwaysConsumeAmmo()
        {
            return ConsumeAmmoChance >= 1f;
        }

        internal override void SetId(IContentPack contentPack)
        {
            Id = string.Concat(contentPack.Manifest.UniqueID, "/", Type, "/", Name);

            base.SetId(contentPack);
        }
    }
}
