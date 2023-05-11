using Microsoft.Xna.Framework;

namespace Archery.Framework.Models.Ammo
{
    public class ArrowTailModel
    {
        public Rectangle Source { get; set; }
        public Vector2 Offset { get; set; }
        public int Amount { get; set; } = 4;
        public int SpawnIntervalInMilliseconds { get; set; } = 50;

        public float? ScaleStep { get; set; }
        public float SpacingStep { get; set; } = 0.25f;
        public float? AlphaStep { get; set; } = 0.1f;
    }
}
