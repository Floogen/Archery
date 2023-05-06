using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Projectiles;
using Archery.Framework.Patches.Objects;
using Archery.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using System;
using static Archery.Framework.Interfaces.IFashionSenseApi;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects.Weapons
{
    internal class Bow : InstancedObject
    {
        public static Slingshot CreateInstance(WeaponModel weaponModel)
        {
            var bow = new Slingshot();
            bow.modData[ModDataKeys.WEAPON_FLAG] = weaponModel.Id;

            // Hide attachment slot from bows with internal ammo
            if (weaponModel.UsesInternalAmmo())
            {
                bow.numAttachmentSlots.Value = 0;
            }

            return bow;
        }

        internal static bool Use(Tool tool, GameLocation location, int x, int y, Farmer who)
        {
            return false;
        }

        internal static bool CanThisBeAttached(Tool tool, Object item)
        {
            if (Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                if (item is null || (Arrow.GetModel<AmmoModel>(item) is AmmoModel ammoModel && weaponModel.IsValidAmmoType(ammoModel.Type)))
                {
                    return true;
                }
            }

            return false;
        }

        internal static float GetSlingshotChargeTimePostfix(Tool tool)
        {
            if (Bow.IsValid(tool) is true && tool is Slingshot slingshot && Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                var requiredChargingTime = weaponModel.ChargeTimeRequiredMilliseconds;
                var pullStartTimeInMilliseconds = slingshot.pullStartTime * 1000;

                return Utility.Clamp((float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds - pullStartTimeInMilliseconds) / (double)requiredChargingTime), 0f, 1f);
            }

            return 0f;
        }

        internal static Object GetAmmoItem(Tool tool)
        {
            if (Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                if (tool is Slingshot slingshot && slingshot.attachments is not null && slingshot.attachments[0] is not null)
                {
                    return slingshot.attachments[0];
                }
                else if (weaponModel.UsesInternalAmmo())
                {
                    return Arrow.CreateInstance(Archery.modelManager.GetSpecificModel<AmmoModel>(weaponModel.InternalAmmoId));
                }
            }

            return null;
        }

        internal static int GetAmmoCount(Tool tool)
        {
            if (GetAmmoItem(tool) is Object attachment && attachment is not null)
            {
                return attachment.Stack;
            }

            return 0;
        }

        internal static void TickUpdate(Slingshot slingshot, ref bool canPlaySound, ref Farmer lastUser, NetEvent0 finishEvent, GameTime time, Farmer who)
        {
            var weaponModel = Bow.GetModel<WeaponModel>(slingshot);
            if (weaponModel is null)
            {
                return;
            }

            lastUser = who;
            finishEvent.Poll();
            if (!who.usingSlingshot)
            {
                return;
            }

            if (who.IsLocalPlayer)
            {
                SlingshotPatch.UpdateAimPosReversePatch(slingshot);
                int mouseX = slingshot.aimPos.X;
                int mouseY = slingshot.aimPos.Y;

                Game1.debugOutput = "playerPos: " + who.getStandingPosition().ToString() + ", mousePos: " + mouseX + ", " + mouseY;
                slingshot.mouseDragAmount++;
                if (!Game1.options.useLegacySlingshotFiring)
                {
                    Vector2 shoot_origin = slingshot.GetShootOrigin(who);
                    Vector2 aim_offset = slingshot.AdjustForHeight(new Vector2(mouseX, mouseY)) - shoot_origin;
                    if (Math.Abs(aim_offset.X) > Math.Abs(aim_offset.Y))
                    {
                        if (aim_offset.X < 0f)
                        {
                            who.faceDirection(3);
                        }

                        if (aim_offset.X > 0f)
                        {
                            who.faceDirection(1);
                        }
                    }
                    else
                    {
                        if (aim_offset.Y < 0f)
                        {
                            who.faceDirection(0);
                        }

                        if (aim_offset.Y > 0f)
                        {
                            who.faceDirection(2);
                        }
                    }
                }
                else
                {
                    who.faceGeneralDirection(new Vector2(mouseX, mouseY), 0, opposite: true);
                }

                if (!Game1.options.useLegacySlingshotFiring)
                {
                    if (canPlaySound && slingshot.GetSlingshotChargeTime() >= 1f)
                    {
                        Toolkit.PlaySound(weaponModel.FinishChargingSound, weaponModel.Id, who.getStandingPosition());
                        canPlaySound = false;
                    }
                }
                else if (canPlaySound && (Math.Abs(mouseX - slingshot.lastClickX) > 8 || Math.Abs(mouseY - slingshot.lastClickY) > 8) && slingshot.mouseDragAmount > 4)
                {
                    who.currentLocation.playSound("slingshot");
                    canPlaySound = false;
                }

                if (!slingshot.CanAutoFire())
                {
                    slingshot.lastClickX = mouseX;
                    slingshot.lastClickY = mouseY;
                }

                if (Game1.options.useLegacySlingshotFiring)
                {
                    Game1.mouseCursor = -1;
                }

                if (slingshot.CanAutoFire())
                {
                    bool first_fire = false;
                    if (slingshot.GetBackArmDistance(who) >= 20 && slingshot.nextAutoFire < 0f)
                    {
                        slingshot.nextAutoFire = 0f;
                        first_fire = true;
                    }
                    if (slingshot.nextAutoFire > 0f || first_fire)
                    {
                        slingshot.nextAutoFire -= (float)time.ElapsedGameTime.TotalSeconds;
                        if (slingshot.nextAutoFire <= 0f)
                        {
                            slingshot.PerformFire(who.currentLocation, who);
                            slingshot.nextAutoFire = slingshot.GetAutoFireRate();
                        }
                    }
                }
            }

            int offset = ((who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0));
            who.FarmerSprite.setCurrentFrame(42 + offset);
        }

        internal static void PerformFire(Slingshot slingshot, ref bool canPlaySound, GameLocation location, Farmer who)
        {
            var weaponModel = Bow.GetModel<WeaponModel>(slingshot);
            var ammoModel = Arrow.GetModel<AmmoModel>(Bow.GetAmmoItem(slingshot));
            if (weaponModel is null || ammoModel is null)
            {
                return;
            }

            // TODO: Clean Bow.PerformFire up
            if (Bow.GetAmmoCount(slingshot) > 0)
            {
                SlingshotPatch.UpdateAimPosReversePatch(slingshot);

                int mouseX = slingshot.aimPos.X;
                int mouseY = slingshot.aimPos.Y;
                int backArmDistance = slingshot.GetBackArmDistance(who);

                Vector2 shoot_origin = slingshot.GetShootOrigin(who);
                Vector2 v = Utility.getVelocityTowardPoint(slingshot.GetShootOrigin(who), slingshot.AdjustForHeight(new Vector2(mouseX, mouseY)), weaponModel.ProjectileSpeed * (1f + who.weaponSpeedModifier));

                if (backArmDistance > 4 && !canPlaySound)
                {
                    // Get the ammo to be used
                    if (weaponModel.UsesInternalAmmo() is false && (weaponModel.ShouldAlwaysConsumeAmmo() || Game1.random.NextDouble() < weaponModel.ConsumeAmmoChance))
                    {
                        slingshot.attachments[0].Stack--;
                        if (slingshot.attachments[0].Stack <= 0)
                        {
                            slingshot.attachments[0] = null;
                        }
                    }

                    if (!Game1.options.useLegacySlingshotFiring)
                    {
                        v.X *= -1f;
                        v.Y *= -1f;
                    }

                    int weaponBaseDamageAndAmmoAdditive = weaponModel.DamageRange.Get(Game1.random) + ammoModel.BaseDamage;
                    var arrow = new ArrowProjectile(ammoModel, who, (int)(weaponBaseDamageAndAmmoAdditive * (1f + who.attackIncreaseModifier)), 0, 0, 0f, 0f - v.X, 0f - v.Y, shoot_origin, String.Empty, String.Empty, explode: false, damagesMonsters: true, location, spriteFromObjectSheet: true)
                    {
                        IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
                    };
                    arrow.startingRotation.Value = Bow.GetFrontArmRotation(who, slingshot);

                    location.projectiles.Add(arrow);

                    // Play firing sound
                    Toolkit.PlaySound(weaponModel.FiringSound, weaponModel.Id, shoot_origin);
                }
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
            }

            // TODO: Need to handle Slingshot.tickUpdate to not play the default sound
            canPlaySound = true;
        }

        internal static bool Draw(IDrawTool drawTool)
        {
            Farmer who = drawTool.Farmer;

            if (who.UsingTool is false || Bow.IsValid(who.CurrentTool) is false)
            {
                return false;
            }

            // Get the required models
            var ammoItem = Bow.GetAmmoItem(who.CurrentTool);

            var bowModel = Bow.GetModel<WeaponModel>(who.CurrentTool);
            var ammoModel = Arrow.GetModel<AmmoModel>(ammoItem);

            if (bowModel is null || ammoModel is null)
            {
                return false;
            }

            bool shouldDrawArrow = ammoItem is not null && ammoItem.Stack > 0;

            // Get slingshot and related data
            Slingshot slingshot = who.CurrentTool as Slingshot;

            float currentChargePercentage = slingshot.GetSlingshotChargeTime();
            float frontArmRotation = GetFrontArmRotation(who, slingshot);

            // Get the layer depth
            float layerDepth = drawTool.LayerDepthSnapshot;

            // Establish the offsets
            var originOffset = Vector2.Zero;
            var specialOffset = Vector2.Zero;
            var baseOffset = drawTool.Position + drawTool.Origin + drawTool.PositionOffset + who.armOffset;

            int movingArmStartingFrame = GetMovingArmFrame(who.FacingDirection, currentChargePercentage);
            int arrowFrame = GetArrowFrame(who.FacingDirection, currentChargePercentage);

            // Get the arrow and bow sprites
            var ammoSprite = ammoModel.GetSpriteFromDirection(who);
            var bowSprite = bowModel.GetSpriteFromDirection(who);

            // Get the flip effect
            var flipEffect = who.FacingDirection == Game1.left ? SpriteEffects.FlipVertically : SpriteEffects.None;
            var bowFlipOverride = bowSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : bowSprite.GetSpriteEffects();
            var arrowFlipOverride = ammoSprite is null || ammoSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : ammoSprite.GetSpriteEffects();

            switch (who.FacingDirection)
            {
                case Game1.down:
                    // Draw the back arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the bow
                    specialOffset = new Vector2(4f - frontArmRotation * 2f, 0f);
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset + bowSprite.Offset, bowSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin, bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + ammoSprite.Offset, ammoSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin + new Vector2(0f, -16f), ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(0, 32, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                case Game1.right:
                case Game1.left:
                    originOffset = who.FacingDirection == Game1.left ? new Vector2(0f, -16f) : new Vector2(0f, -16f);
                    specialOffset = who.FacingDirection == Game1.left ? new Vector2(56f, -60f) : new Vector2(8f, -60f);

                    // Draw the back arm
                    // TODO: Get player's layer depth via IDrawTool
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(48, 32, 16, 32), drawTool.OverrideColor, frontArmRotation, drawTool.Origin + originOffset, 4f * drawTool.Scale, flipEffect, 5.9E-05f);

                    // Draw the bow
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset + bowSprite.Offset + (who.FacingDirection == Game1.left ? new Vector2(-8f, 0f) : new Vector2(8f, 0f)), bowSprite.Source, Color.White, frontArmRotation, drawTool.Origin + originOffset, bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + ammoSprite.Offset, ammoSprite.Source, Color.White, frontArmRotation, drawTool.Origin + new Vector2(-13f + arrowFrame, -32f), ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 32, 16, 32), drawTool.OverrideColor, frontArmRotation, drawTool.Origin + originOffset, 4f * drawTool.Scale, flipEffect, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                case Game1.up:
                    // Draw the back arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the bow
                    specialOffset = new Vector2((frontArmRotation - 6f) * 4f, 0f);
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset + bowSprite.Offset, bowSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin, bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + ammoSprite.Offset, ammoSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin + new Vector2(-13f + arrowFrame, -32f), ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(0, 32, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                default:
                    return false;
            }
        }

        // TODO: Have the BowModel determine the frame value based on the charge percentage
        private static int GetArrowFrame(int facingDirection, float bowChargePercentage)
        {
            int bowFrame = 0;
            switch (facingDirection)
            {
                case Game1.down:
                    break;
                case Game1.right:
                case Game1.left:
                    bowFrame = (bowChargePercentage > 0.8f ? 2 : bowChargePercentage > 0.5f ? 1 : 0);
                    break;
                case Game1.up:
                    break;
            }

            return bowFrame;
        }

        private static int GetMovingArmFrame(int facingDirection, float bowChargePercentage)
        {
            int armFrame = 0;
            int startingOffset = 0;
            switch (facingDirection)
            {
                case Game1.down:
                    armFrame = (bowChargePercentage > 0.8f ? 2 : bowChargePercentage > 0.5f ? 1 : 0);
                    break;
                case Game1.right:
                case Game1.left:
                    armFrame = (bowChargePercentage > 0.8f ? 2 : bowChargePercentage > 0.5f ? 1 : 0);
                    startingOffset = 64;
                    break;
                case Game1.up:
                    armFrame = (bowChargePercentage > 0.8f ? 2 : bowChargePercentage > 0.5f ? 1 : 0);
                    startingOffset = 64;
                    break;
            }

            return (armFrame * 16) + startingOffset;
        }

        internal static float GetFrontArmRotation(Farmer who, Slingshot slingshot)
        {
            Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value)));
            int mouseX = point.X;
            int mouseY = point.Y;

            Vector2 shoot_origin = slingshot.GetShootOrigin(who);
            float frontArmRotation = (float)Math.Atan2((float)mouseY - shoot_origin.Y, (float)mouseX - shoot_origin.X) + (float)Math.PI;
            if (Game1.options.useLegacySlingshotFiring is false)
            {
                frontArmRotation -= (float)Math.PI;
                if (frontArmRotation < 0f)
                {
                    frontArmRotation += (float)Math.PI * 2f;
                }
            }

            return frontArmRotation;
        }
    }
}
