using Archery.Framework.Interfaces.Internal;
using Archery.Framework.Models.Ammo;
using Archery.Framework.Models.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

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
        public List<WeightedModel> WeightedInternalAmmoIds { get; set; }

        // Only used by WeaponType.Crossbow
        public int MagazineSize { get; set; } = 1;

        public bool CanAutoFire { get; set; }
        public float AutoFireRateMilliseconds { get; set; } = 300f;

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
            return string.IsNullOrEmpty(GetInternalAmmoId()) is false;
        }

        internal string GetInternalAmmoId()
        {
            string selectedInternalAmmoId = null;
            if (string.IsNullOrEmpty(InternalAmmoId) is false)
            {
                selectedInternalAmmoId = InternalAmmoId;
            }
            else if (WeightedInternalAmmoIds is not null && WeightedInternalAmmoIds.Count > 0)
            {
                var weightedSelection = WeightedInternalAmmoIds.Where(v => v.ChanceWeight > Game1.random.NextDouble()).ToList();
                if (weightedSelection.Count > 0)
                {
                    var randomWeightedSelection = Game1.random.Next(0, weightedSelection.Count());
                    selectedInternalAmmoId = weightedSelection[randomWeightedSelection].Id;
                }
            }

            // Validate the selected internal ammo ID exists
            if (Archery.modelManager.DoesModelExist<AmmoModel>(selectedInternalAmmoId) is false)
            {
                return null;
            }

            return selectedInternalAmmoId;
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
