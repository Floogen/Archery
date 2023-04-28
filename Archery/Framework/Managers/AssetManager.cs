using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archery.Framework.Managers
{
    internal class AssetManager
    {
        internal string assetFolderPath;

        // UI textures
        internal readonly Texture2D bowArmsTexture;
        internal readonly Texture2D baseBowTexture;
        internal readonly Texture2D iconBowTexture;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the assets
            bowArmsTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BowArms.png"));
            baseBowTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BaseBow.png"));
            iconBowTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BowIcon.png"));
        }
    }
}
