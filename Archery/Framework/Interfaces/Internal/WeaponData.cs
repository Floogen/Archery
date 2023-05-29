namespace Archery.Framework.Interfaces.Internal
{
    public class WeaponData : IWeaponData
    {
        public string WeaponId { get; init; }
        public WeaponType WeaponType { get; init; }
        public int? MagazineSize { get; init; }
        public int? AmmoInMagazine { get; set; }
    }
}
