using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using Archery.Framework.Objects.Weapons;
using StardewValley.Tools;

namespace Archery.Framework.Patches.Objects
{
    internal class SlingshotPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Slingshot);

        public SlingshotPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
        }

        private static bool DrawInMenuPrefix(Slingshot __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (Bow.IsValid(__instance))
            {
                spriteBatch.Draw(Archery.assetManager.iconBowTexture, location + new Vector2(34f, 32f) * scaleSize, new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
                return false;
            }

            return true;
        }
    }
}
