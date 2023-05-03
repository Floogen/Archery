using Microsoft.Xna.Framework;

namespace Archery.Framework.Models.Display
{
    public class ItemSpriteModel
    {
        public Rectangle Source { get; set; }
        public float Scale { get; set; } = 4f;

        public bool FlipHorizontally { get; set; }
        public bool FlipVertically { get; set; }
    }
}
