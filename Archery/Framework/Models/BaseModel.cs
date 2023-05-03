using Archery.Framework.Models.Display;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Archery.Framework.Models
{
    public class BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SpriteModel Icon { get; set; }

        internal string Id { get; set; }
        internal IContentPack ContentPack { get; set; }
        internal Texture2D Texture { get; set; }
        internal string TexturePath { get; set; }

        internal virtual void SetId(IContentPack contentPack)
        {

        }
    }
}
