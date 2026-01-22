using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects;
using Archery.Framework.Objects.Items;
using Archery.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Archery.Framework.Patches.Objects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Object);

        public ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Constructor(_object, new[] { typeof(string), typeof(int), typeof(bool), typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(ObjectConstructorPostfix)));
            harmony.Patch(AccessTools.Constructor(_object, new[] { typeof(Vector2), typeof(string), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(ObjectConstructorWorldPostfix)));

            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "getDescription", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Object.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
        }

        private static void ObjectConstructorPostfix(Object __instance, string itemId, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
        {
            HandleCustomFields(__instance);
        }

        private static void ObjectConstructorWorldPostfix(Object __instance, Vector2 tileLocation, string itemId, bool isRecipe = false)
        {
            HandleCustomFields(__instance);
        }

        private static void HandleCustomFields(Object instance)
        {
            if (Game1.objectData.TryGetValue(instance.ItemId, out var data) is false || data is null || data.CustomFields is null)
            {
                return;
            }

            if (data.CustomFields.ContainsKey(ModDataKeys.WEAPON_FLAG))
            {
                instance.modData[ModDataKeys.WEAPON_FLAG] = data.CustomFields[ModDataKeys.WEAPON_FLAG];
            }
            else if (data.CustomFields.ContainsKey(ModDataKeys.AMMO_FLAG))
            {
                instance.modData[ModDataKeys.AMMO_FLAG] = data.CustomFields[ModDataKeys.AMMO_FLAG];
            }
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
                __result = Arrow.GetDescription(__instance);
                return;
            }
        }

        private static bool DrawInMenuPrefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, ref Color color, bool drawShadow)
        {
            if (Arrow.GetModel<AmmoModel>(__instance) is AmmoModel arrowModel && arrowModel is not null)
            {
                bool isRecipe = InstancedObject.IsRecipe(__instance);

                if (isRecipe)
                {
                    transparency = 0.5f;
                    scaleSize *= 0.75f;
                }

                var arrowIcon = arrowModel.GetIcon(Game1.player);
                if (arrowIcon is null)
                {
                    return false;
                }

                spriteBatch.Draw(arrowModel.Texture, location + (new Vector2(32f, 32f) + arrowIcon.Offset) * scaleSize, arrowIcon.Source, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, arrowIcon.Scale, arrowIcon.GetSpriteEffects(), layerDepth);

                if (drawStackNumber != 0 && __instance.Stack > 0 && __instance.Stack != int.MaxValue && isRecipe is false)
                {
                    Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
                }

                if (isRecipe)
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), color, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);
                }
                return false;
            }

            return true;
        }
    }
}