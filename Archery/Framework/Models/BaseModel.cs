using Archery.Framework.Models.Display;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Archery.Framework.Models
{
    public class BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SpriteModel Sprite { get; set; }

        internal string Id { get; set; }
        internal IContentPack ContentPack { get; set; }
        internal Texture2D Texture { get; set; }
    }
}
