using Microsoft.Xna.Framework;

namespace Archery.Framework.Interfaces.Internal
{
    internal class ProjectileData : IProjectileData
    {
        public string AmmoId { get; init; }
        public Vector2? Position { get; set; }
        public Vector2? Velocity { get; set; }
        public float? InitialSpeed { get; init; }
        public float? Rotation { get; set; }
        public int? BaseDamage { get; set; }
        public float? BreakChance { get; set; }
        public float? CriticalChance { get; set; }
        public float? CriticalDamageMultiplier { get; set; }
        public bool? DoesExplodeOnImpact { get; set; }
        public int? ExplosionRadius { get; set; }
        public int? ExplosionDamage { get; set; }
    }
}
