using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Archery.Framework.Models.Display
{
    public class ItemSpriteModel
    {
        public Rectangle Source { get; set; }
        public float Scale { get; set; } = 4f;

        public bool FlipHorizontally { get; set; }
        public bool FlipVertically { get; set; }

        public SpriteEffects GetSpriteEffects()
        {
            if (FlipHorizontally && FlipVertically)
            {
                return SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
            else if (FlipHorizontally)
            {
                return SpriteEffects.FlipHorizontally;
            }
            else if (FlipVertically)
            {
                return SpriteEffects.FlipVertically;
            }

            return SpriteEffects.None;
        }
    }
}
