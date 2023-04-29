using Archery.Framework.Objects.Projectiles;
using Archery.Framework.Objects.Weapons;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;

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

            harmony.CreateReversePatcher(AccessTools.Method(_object, "updateAimPos", null), new HarmonyMethod(GetType(), nameof(UpdateAimPosReversePatch))).Patch();
        }

        private static bool DrawInMenuPrefix(Slingshot __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (Bow.IsValid(__instance))
            {
                spriteBatch.Draw(Archery.assetManager.iconBowTexture, location + new Vector2(34f, 32f) * scaleSize, new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 3f, SpriteEffects.None, layerDepth);
                return false;
            }

            return true;
        }

        private static bool PerformFirePrefix(Slingshot __instance, ref bool ___canPlaySound, GameLocation location, Farmer who)
        {
            if (Bow.IsValid(__instance) is false)
            {
                return true;
            }

            if (__instance.attachments[0] != null)
            {
                UpdateAimPosReversePatch(__instance);

                int mouseX = __instance.aimPos.X;
                int mouseY = __instance.aimPos.Y;
                int backArmDistance = __instance.GetBackArmDistance(who);
                Vector2 shoot_origin = __instance.GetShootOrigin(who);
                Vector2 v = Utility.getVelocityTowardPoint(__instance.GetShootOrigin(who), __instance.AdjustForHeight(new Vector2(mouseX, mouseY)), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.weaponSpeedModifier));
                if (backArmDistance > 4 && !___canPlaySound)
                {
                    StardewValley.Object ammunition = (StardewValley.Object)__instance.attachments[0].getOne();
                    __instance.attachments[0].Stack--;
                    if (__instance.attachments[0].Stack <= 0)
                    {
                        __instance.attachments[0] = null;
                    }
                    int damage = 1;
                    BasicProjectile.onCollisionBehavior collisionBehavior = null;
                    string collisionSound = "hammer";
                    float damageMod = 1f;
                    if (__instance.InitialParentTileIndex == 33)
                    {
                        damageMod = 2f;
                    }
                    else if (__instance.InitialParentTileIndex == 34)
                    {
                        damageMod = 4f;
                    }
                    switch (ammunition.ParentSheetIndex)
                    {
                        case 388:
                            damage = 2;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 390:
                            damage = 5;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 378:
                            damage = 10;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 380:
                            damage = 20;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 384:
                            damage = 30;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 382:
                            damage = 15;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 386:
                            damage = 50;
                            ammunition.ParentSheetIndex++;
                            break;
                        case 441:
                            damage = 20;
                            collisionBehavior = BasicProjectile.explodeOnImpact;
                            collisionSound = "explosion";
                            break;
                    }
                    if (ammunition.Category == -5)
                    {
                        collisionSound = "slimedead";
                    }
                    if (!Game1.options.useLegacySlingshotFiring)
                    {
                        v.X *= -1f;
                        v.Y *= -1f;
                    }

                    switch (who.FacingDirection)
                    {
                        case Game1.left:
                            shoot_origin += new Vector2(0f, -64f);
                            break;
                        case Game1.down:
                            shoot_origin += new Vector2(-64f, 0f);
                            break;
                    }
                    var arrow = new Arrow((int)(damageMod * (float)(damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.attackIncreaseModifier)), ammunition.ParentSheetIndex, 0, 0, (float)(Math.PI / (double)(64f + (float)Game1.random.Next(-63, 64))), 0f - v.X, 0f - v.Y, shoot_origin, collisionSound, "", explode: false, damagesMonsters: true, location, who, spriteFromObjectSheet: true, collisionBehavior)
                    {
                        IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
                    };
                    arrow.startingRotation.Value = Bow.GetFrontArmRotation(who, __instance);

                    location.projectiles.Add(arrow);
                }
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
            }
            ___canPlaySound = true;

            return false;
        }

        private static void UpdateAimPosReversePatch(Slingshot __instance)
        {
            new NotImplementedException("It's a stub!");
        }
    }
}
