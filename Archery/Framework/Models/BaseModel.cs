using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Archery.Framework.Models
{
    public class BaseModel
    {
        internal string Id { get; set; }
        internal IContentPack ContentPack { get; set; }
        internal Texture2D Texture { get; set; }
    }
}
