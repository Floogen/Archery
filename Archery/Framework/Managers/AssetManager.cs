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

        // Base textures
        internal readonly Texture2D baseArmsTexture;
        internal readonly Texture2D baseBowTexture;
        internal readonly Texture2D iconBowTexture;

        // Recolored textures
        internal Texture2D recoloredArmsTexture;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the assets
            baseArmsTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BowArms.png"));
            recoloredArmsTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BowArms.png"));
            baseBowTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BaseBow.png"));
            iconBowTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "BowIcon.png"));
        }
    }
}
