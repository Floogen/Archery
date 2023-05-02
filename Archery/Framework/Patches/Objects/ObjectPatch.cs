using Archery.Framework.Objects.Items;
using Archery.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FishingTrawler.Framework.Patches.Objects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Object);

        public ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "getDescription", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Object.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
        }

        private static void GetNamePostfix(Object __instance, ref string __result)
        {
            if (Arrow.IsValid(__instance))
            {
                __result = Arrow.GetName(__instance);
                return;
            }
        }

        private static void GetDescriptionPostfix(Object __instance, ref string __result)
        {
            if (Arrow.IsValid(__instance))
            {
                __result = Game1.parseText(Arrow.GetDescription(__instance), Game1.smallFont, System.Math.Max(272, (int)Game1.dialogueFont.MeasureString((__instance.DisplayName == null) ? "" : __instance.DisplayName).X));
                return;
            }
        }

        private static bool DrawInMenuPrefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, ref Color color, bool drawShadow)
        {
            if (Arrow.IsValid(__instance))
            {
                //spriteBatch.Draw(Archery.assetManager.GetArrowTexture(MOD_DATA_VALUE_HERE), location + new Vector2(32f, 32f) * scaleSize, new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);

                return false;
            }

            return true;
        }
    }
}