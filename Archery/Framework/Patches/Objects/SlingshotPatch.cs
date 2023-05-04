using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Projectiles;
using Archery.Framework.Objects.Weapons;
using Archery.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

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
            harmony.Patch(AccessTools.Method(_object, nameof(Slingshot.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Slingshot.PerformFire), new[] { typeof(GameLocation), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(PerformFirePrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Slingshot.beginUsing), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), postfix: new HarmonyMethod(GetType(), nameof(BeginUsingPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Slingshot.canThisBeAttached), new[] { typeof(Object) }), postfix: new HarmonyMethod(GetType(), nameof(CanThisBeAttachedPostfix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Slingshot.GetSlingshotChargeTime), null), postfix: new HarmonyMethod(GetType(), nameof(GetSlingshotChargeTimePostfix)));

            harmony.CreateReversePatcher(AccessTools.Method(_object, "updateAimPos", null), new HarmonyMethod(GetType(), nameof(UpdateAimPosReversePatch))).Patch();
        }

        private static bool DrawInMenuPrefix(Slingshot __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (Bow.GetModel<WeaponModel>(__instance) is WeaponModel weaponModel && weaponModel is not null)
            {
                spriteBatch.Draw(weaponModel.Texture, location + new Vector2(34f, 32f) * scaleSize, weaponModel.Icon.Source, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, weaponModel.Icon.Scale, SpriteEffects.None, layerDepth);

                int ammoCount = Bow.GetAmmoCount(__instance);
                if (drawStackNumber != 0 && ammoCount > 0)
                {
                    Utility.drawTinyDigits(ammoCount, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(ammoCount, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
                }
                return false;
            }

            return true;
        }

        private static bool PerformFirePrefix(Slingshot __instance, ref bool ___canPlaySound, GameLocation location, Farmer who)
        {
            var weaponModel = Bow.GetModel<WeaponModel>(__instance);
            var ammoModel = Arrow.GetModel<AmmoModel>(Bow.GetAmmoItem(__instance));
            if (weaponModel is null || ammoModel is null)
            {
                return true;
            }

            // TODO: Clean SlingshotPatch.PerformFirePrefix up
            if (Bow.GetAmmoCount(__instance) > 0)
            {
                UpdateAimPosReversePatch(__instance);

                int mouseX = __instance.aimPos.X;
                int mouseY = __instance.aimPos.Y;
                int backArmDistance = __instance.GetBackArmDistance(who);

                Vector2 shoot_origin = __instance.GetShootOrigin(who);
                Vector2 v = Utility.getVelocityTowardPoint(__instance.GetShootOrigin(who), __instance.AdjustForHeight(new Vector2(mouseX, mouseY)), weaponModel.ProjectileSpeed * (1f + who.weaponSpeedModifier));

                if (backArmDistance > 4 && !___canPlaySound)
                {
                    // Get the ammo to be used
                    __instance.attachments[0].Stack--;
                    if (__instance.attachments[0].Stack <= 0)
                    {
                        __instance.attachments[0] = null;
                    }

                    // TODO: Implement firing and collision sounds
                    string collisionSound = "hammer";

                    if (!Game1.options.useLegacySlingshotFiring)
                    {
                        v.X *= -1f;
                        v.Y *= -1f;
                    }

                    int weaponBaseDamageAndAmmoAdditive = weaponModel.DamageRange.Get(Game1.random) + ammoModel.BaseDamage;
                    var arrow = new ArrowProjectile(ammoModel, who, (int)(weaponBaseDamageAndAmmoAdditive * (1f + who.attackIncreaseModifier)), 0, 0, 0f, 0f - v.X, 0f - v.Y, shoot_origin, collisionSound, "", explode: false, damagesMonsters: true, location, spriteFromObjectSheet: true)
                    {
                        IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
                    };
                    arrow.startingRotation.Value = Bow.GetFrontArmRotation(who, __instance);

                    location.projectiles.Add(arrow);

                    // Play firing sound
                    Toolkit.PlaySound(weaponModel.FiringSound, weaponModel.Id, shoot_origin);
                }
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
            }
            ___canPlaySound = true;

            return false;
        }

        private static void BeginUsingPostfix(Slingshot __instance, GameLocation location, int x, int y, Farmer who)
        {
            var weaponModel = Bow.GetModel<WeaponModel>(__instance);
            if (weaponModel is not null)
            {
                // Play charging sound
                Toolkit.PlaySound(weaponModel.ChargingSound, weaponModel.Id, who.getStandingPosition());
            }
        }

        private static void CanThisBeAttachedPostfix(Slingshot __instance, ref bool __result, Object o)
        {
            if (Bow.IsValid(__instance))
            {
                __result = Bow.CanThisBeAttached(__instance, o);
            }
        }

        private static void GetSlingshotChargeTimePostfix(Slingshot __instance, ref float __result)
        {
            if (Bow.IsValid(__instance))
            {
                __result = Bow.GetSlingshotChargeTimePostfix(__instance);
            }
        }

        private static void UpdateAimPosReversePatch(Slingshot __instance)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
