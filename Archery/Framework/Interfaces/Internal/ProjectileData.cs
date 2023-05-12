using Microsoft.Xna.Framework;

namespace Archery.Framework.Interfaces.Internal
{
    internal class ProjectileData : IProjectileData
    {
        public string AmmoId { get; set; }
        public Vector2? Velocity { get; set; }
        public int? BaseDamage { get; set; }
        public float? CriticalChance { get; set; }
        public float? CriticalDamageMultiplier { get; set; }
    }
}
