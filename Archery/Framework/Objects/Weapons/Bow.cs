using Archery.Framework.Models.Enums;
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
        internal static int ActiveCooldown = 0;
        internal static float CooldownAdditiveScale = 0;

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

        public static Slingshot CreateRecipe(WeaponModel weaponModel)
        {
            var recipe = CreateInstance(weaponModel);
            recipe.modData[ModDataKeys.RECIPE_FLAG] = true.ToString();

            return recipe;
        }

        internal static bool Use(Tool tool, GameLocation location, int x, int y, Farmer who)
        {
            return false;
        }

        internal static bool IsLoaded(Tool tool)
        {
            return Bow.GetLoaded(tool) > 0;
        }

        internal static int GetLoaded(Tool tool)
        {
            if (Bow.IsValid(tool) is true && tool.modData.TryGetValue(ModDataKeys.IS_LOADED_FLAG, out var rawLoadedAmmoCount) && int.TryParse(rawLoadedAmmoCount, out int parsedLoadedAmmoCount))
            {
                return parsedLoadedAmmoCount;
            }

            return 0;
        }

        internal static void SetLoaded(Tool tool, int ammoCount)
        {
            if (tool is not null)
            {
                tool.modData[ModDataKeys.IS_LOADED_FLAG] = ammoCount.ToString();
            }
        }

        internal static bool CanUseSpecialAttack(Tool tool)
        {
            if (Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel && weaponModel.SpecialAttack is not null && Bow.IsUsingSpecialAttack(tool) is false && ActiveCooldown <= 0 && Game1.activeClickableMenu is null)
            {
                return true;
            }

            return false;
        }

        internal static bool IsUsingSpecialAttack(Tool tool)
        {
            if (Bow.IsValid(tool) && tool.modData.TryGetValue(ModDataKeys.IS_USING_SPECIAL_ATTACK_FLAG, out string rawIsUsingSpecial) && bool.TryParse(rawIsUsingSpecial, out bool parsedIsUsingSpecial))
            {
                return parsedIsUsingSpecial;
            }

            return false;
        }

        internal static void SetUsingSpecialAttack(Tool tool, bool state)
        {
            if (Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                if (state)
                {
                    Bow.CooldownAdditiveScale = 2f;
                    Bow.ActiveCooldown = Archery.internalApi.GetSpecialAttackCooldown(weaponModel.SpecialAttack.Id);
                }

                tool.modData[ModDataKeys.IS_USING_SPECIAL_ATTACK_FLAG] = state.ToString();
            }
        }

        internal static bool CanThisBeAttached(Tool tool, Object item)
        {
            if (Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel && weaponModel.UsesInternalAmmo() is false)
            {
                if (item is null || (Arrow.GetModel<AmmoModel>(item) is AmmoModel ammoModel && weaponModel.IsValidAmmoType(ammoModel.Type)))
                {
                    return true;
                }
            }

            return false;
        }

        internal static float GetSlingshotChargeTime(Tool tool)
        {
            if (Bow.IsValid(tool) is true && tool is Slingshot slingshot && Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                var requiredChargingTime = weaponModel.ChargeTimeRequiredMilliseconds;
                var pullStartTimeInMilliseconds = slingshot.pullStartTime * 1000;

                return Utility.Clamp((float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds - pullStartTimeInMilliseconds) / (double)requiredChargingTime), 0f, 1f);
            }

            return 0f;
        }

        internal static void SetSlingshotChargeTime(Tool tool, float percentage)
        {
            if (Bow.IsValid(tool) is true && tool is Slingshot slingshot && Bow.GetModel<WeaponModel>(tool) is WeaponModel weaponModel)
            {
                var currentMilliseconds = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                var requiredChargingTime = weaponModel.ChargeTimeRequiredMilliseconds;

                percentage = Utility.Clamp(percentage, 0f, 1f);
                slingshot.pullStartTime = Math.Abs(currentMilliseconds - (percentage * requiredChargingTime)) / 1000;
            }
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

            // Update the special attack cooldown, if applicable
            if (ActiveCooldown >= 0)
            {
                ActiveCooldown -= time.ElapsedGameTime.Milliseconds;
            }
            if (CooldownAdditiveScale >= 0)
            {
                CooldownAdditiveScale -= 0.1f;
            }

            // Skip if the weapon isn't the current tool
            if (who.CurrentTool != slingshot)
            {
                return;
            }

            lastUser = who;
            finishEvent.Poll();

            if (who.usingSlingshot is false && who.CurrentTool == slingshot && Toolkit.AreSpecialAttackButtonsPressed() && Bow.CanUseSpecialAttack(slingshot) is true)
            {
                SetUsingSpecialAttack(slingshot, true);
                slingshot.pullStartTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
            }
            else if (who.usingSlingshot is false && Bow.IsUsingSpecialAttack(slingshot) is false)
            {
                return;
            }

            if (who.IsLocalPlayer)
            {
                var currentChargeTime = slingshot.GetSlingshotChargeTime();
                if (weaponModel.Type is WeaponType.Crossbow && Bow.IsLoaded(slingshot) is false && currentChargeTime >= 1f)
                {
                    Toolkit.SuppressToolButtons();

                    var currentAmmoCount = Bow.GetAmmoCount(slingshot);
                    Bow.SetLoaded(slingshot, currentAmmoCount < weaponModel.AmmoCountOnReload ? currentAmmoCount : weaponModel.AmmoCountOnReload);
                    return;
                }

                SlingshotPatch.UpdateAimPosReversePatch(slingshot);
                int mouseX = slingshot.aimPos.X;
                int mouseY = slingshot.aimPos.Y;

                Game1.debugOutput = "playerPos: " + who.getStandingPosition().ToString() + ", mousePos: " + mouseX + ", " + mouseY;
                slingshot.mouseDragAmount++;

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

                if (canPlaySound && (currentChargeTime >= 1f || Bow.IsLoaded(slingshot)))
                {
                    Toolkit.PlaySound(weaponModel.FinishChargingSound, weaponModel.Id, who.getStandingPosition());
                    canPlaySound = false;
                }

                if (!slingshot.CanAutoFire())
                {
                    slingshot.lastClickX = mouseX;
                    slingshot.lastClickY = mouseY;
                }

                if (Bow.IsUsingSpecialAttack(slingshot))
                {
                    Bow.PerformSpecial(weaponModel, slingshot, time, who.currentLocation, who);
                    return;
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

        internal static bool PerformFire(Slingshot slingshot, GameLocation location, Farmer who)
        {
            var weaponModel = Bow.GetModel<WeaponModel>(slingshot);
            var ammoModel = Arrow.GetModel<AmmoModel>(Bow.GetAmmoItem(slingshot));
            if (weaponModel is null)
            {
                return false;
            }

            if (Bow.GetAmmoCount(slingshot) > 0 && ammoModel is not null)
            {
                SlingshotPatch.UpdateAimPosReversePatch(slingshot);

                int mouseX = slingshot.aimPos.X;
                int mouseY = slingshot.aimPos.Y;

                Vector2 shoot_origin = slingshot.GetShootOrigin(who);
                Vector2 v = Utility.getVelocityTowardPoint(slingshot.GetShootOrigin(who), slingshot.AdjustForHeight(new Vector2(mouseX, mouseY)), weaponModel.ProjectileSpeed * (1f + who.weaponSpeedModifier));

                // Handle Crossbow ammo loaded
                if (weaponModel.Type is WeaponType.Crossbow)
                {
                    if (Bow.IsLoaded(slingshot) is false || Toolkit.AreToolButtonSuppressed() is true)
                    {
                        return false;
                    }

                    Bow.SetLoaded(slingshot, Bow.GetLoaded(slingshot) - 1);
                }
                else if (slingshot.GetSlingshotChargeTime() < 1f)
                {
                    return false;
                }

                // Get the ammo to be used
                if (weaponModel.UsesInternalAmmo() is false && (weaponModel.ShouldAlwaysConsumeAmmo() || Game1.random.NextDouble() < weaponModel.ConsumeAmmoChance))
                {
                    slingshot.attachments[0].Stack--;
                    if (slingshot.attachments[0].Stack <= 0)
                    {
                        slingshot.attachments[0] = null;
                    }
                }

                v.X *= -1f;
                v.Y *= -1f;

                int weaponBaseDamageAndAmmoAdditive = weaponModel.DamageRange.Get(Game1.random) + ammoModel.BaseDamage;
                var arrow = new ArrowProjectile(weaponModel, ammoModel, who, (int)(weaponBaseDamageAndAmmoAdditive * (1f + who.attackIncreaseModifier)), 0f, 0f - v.X, 0f - v.Y, shoot_origin, String.Empty, String.Empty, explode: false, damagesMonsters: true, location, spriteFromObjectSheet: true)
                {
                    IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
                };
                arrow.startingRotation.Value = Bow.GetFrontArmRotation(who, slingshot);

                location.projectiles.Add(arrow);

                // Play firing sound
                Toolkit.PlaySound(weaponModel.FiringSound, weaponModel.Id, shoot_origin);
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
            }

            Archery.modHelper.Reflection.GetField<bool>(slingshot, "canPlaySound").SetValue(true);

            return true;
        }

        internal static void PerformSpecial(WeaponModel weaponModel, Slingshot slingshot, GameTime time, GameLocation currentLocation, Farmer who)
        {
            // Set the required farmer flags
            who.UsingTool = true;
            who.CanMove = false;

            if (Archery.internalApi.HandleSpecialAttack($"{Archery.manifest.UniqueID}/Snapshot", weaponModel.SpecialAttack.Generate(slingshot, time, currentLocation, who)) is false)
            {
                // Reset the required farmer flags
                who.UsingTool = false;
                who.CanMove = true;

                Bow.SetUsingSpecialAttack(slingshot, false);
            }
        }

        internal static bool Draw(IDrawTool drawTool)
        {
            Farmer who = drawTool.Farmer;

            if (who.UsingTool is false || Bow.IsValid(who.CurrentTool) is false)
            {
                return false;
            }
            else if (Toolkit.AreToolButtonSuppressed())
            {
                return true;
            }

            // Get the required models
            var ammoItem = Bow.GetAmmoItem(who.CurrentTool);

            var bowModel = Bow.GetModel<WeaponModel>(who.CurrentTool);
            var ammoModel = Arrow.GetModel<AmmoModel>(ammoItem);

            if (bowModel is null)
            {
                return false;
            }

            // Get slingshot and related data
            Slingshot slingshot = who.CurrentTool as Slingshot;

            float frontArmRotation = GetFrontArmRotation(who, slingshot);

            // Get the layer depth
            float layerDepth = drawTool.LayerDepthSnapshot;

            // Get the arrow and bow sprites
            var ammoSprite = ammoModel is not null ? ammoModel.GetSpriteFromDirection(who) : null;
            var bowSprite = bowModel.GetSpriteFromDirection(who);
            if (bowSprite is null)
            {
                return false;
            }
            var bowSpriteDirection = bowModel.GetSpriteDirectionFromGivenDirection(who);

            var frontArmSprite = bowSprite.GetArmSprite(ArmType.Front);
            var backArmSprite = bowSprite.GetArmSprite(ArmType.Back);

            // Determine if arrow should be drawn
            bool shouldDrawArrow = ammoItem is not null && ammoItem.Stack > 0 && bowSprite.HideAmmo is false;

            // Establish the offsets
            var originOffset = Vector2.Zero;
            var specialOffset = Vector2.Zero;
            var baseOffset = drawTool.Position + drawTool.Origin + drawTool.PositionOffset + who.armOffset;

            // Get the flip effect
            var flipEffect = who.FacingDirection == Game1.left ? SpriteEffects.FlipVertically : SpriteEffects.None;
            var bowFlipOverride = bowSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : bowSprite.GetSpriteEffects();
            var arrowFlipOverride = ammoSprite is null || ammoSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : ammoSprite.GetSpriteEffects();
            var frontArmFlipOverride = frontArmSprite is null || frontArmSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : frontArmSprite.GetSpriteEffects();
            var backArmFlipOverride = backArmSprite is null || backArmSprite.GetSpriteEffects() is SpriteEffects.None ? flipEffect : backArmSprite.GetSpriteEffects();

            switch (who.FacingDirection)
            {
                case Game1.down:
                    // Draw the back arm
                    if (backArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + backArmSprite.Offset, backArmSprite.Source, drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, backArmSprite.Scale * drawTool.Scale, backArmFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the bow
                    specialOffset = new Vector2(4f - frontArmRotation * 2f, 0f);
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset + bowSprite.Offset, bowSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin, bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + bowSprite.AmmoOffset, ammoSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin + new Vector2(0f, -16f) + bowSprite.AmmoOffset, ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    if (frontArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + frontArmSprite.Offset, frontArmSprite.Source, drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, frontArmSprite.Scale * drawTool.Scale, frontArmFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    return true;
                case Game1.right:
                case Game1.left:
                    originOffset = who.FacingDirection == Game1.left ? new Vector2(0f, -16f) : new Vector2(0f, -16f);
                    specialOffset = who.FacingDirection == Game1.left ? new Vector2(56f, -60f) : new Vector2(8f, -60f);

                    // Draw the back arm
                    if (backArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + backArmSprite.Offset, backArmSprite.Source, drawTool.OverrideColor, backArmSprite.DisableRotation ? 0f : frontArmRotation, drawTool.Origin + originOffset, backArmSprite.Scale * drawTool.Scale, backArmFlipOverride, 5.9E-05f);
                    }

                    // Draw the bow
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset, bowSprite.Source, Color.White, bowSprite.DisableRotation ? 0f : frontArmRotation, new Vector2(0, bowSprite.Source.Height / 2f) - new Vector2(bowSprite.Offset.X, bowSprite.Offset.Y * (bowSpriteDirection == Direction.Sideways && who.FacingDirection == Game1.left ? -1 : 1)), bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + bowSprite.AmmoOffset, ammoSprite.Source, Color.White, ammoSprite.DisableRotation ? 0f : frontArmRotation, drawTool.Origin + new Vector2(-13f, -32f) + bowSprite.AmmoOffset, ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    if (frontArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + frontArmSprite.Offset, frontArmSprite.Source, drawTool.OverrideColor, frontArmSprite.DisableRotation ? 0f : frontArmRotation, new Vector2(0, frontArmSprite.Source.Height / 2f), frontArmSprite.Scale * drawTool.Scale, frontArmFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    return true;
                case Game1.up:
                    // Draw the back arm
                    if (backArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + backArmSprite.Offset, backArmSprite.Source, drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, backArmSprite.Scale * drawTool.Scale, backArmFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the bow
                    specialOffset = new Vector2((frontArmRotation - 6f) * 4f, 0f);
                    drawTool.SpriteBatch.Draw(bowModel.Texture, baseOffset + specialOffset + bowSprite.Offset, bowSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin, bowSprite.Scale * drawTool.Scale, bowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the arrow
                    if (shouldDrawArrow && ammoSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(ammoModel.Texture, baseOffset + specialOffset + bowSprite.AmmoOffset, ammoSprite.Source, Color.White, drawTool.Rotation, drawTool.Origin + new Vector2(-13f, -32f) + bowSprite.AmmoOffset, ammoSprite.Scale * drawTool.Scale, arrowFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    if (frontArmSprite is not null)
                    {
                        drawTool.SpriteBatch.Draw(bowModel.GetArmsTexture(), baseOffset + specialOffset + frontArmSprite.Offset, frontArmSprite.Source, drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, frontArmSprite.Scale * drawTool.Scale, frontArmFlipOverride, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));
                    }

                    return true;
                default:
                    return false;
            }
        }

        internal static float GetFrontArmRotation(Farmer who, Slingshot slingshot)
        {
            Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value)));
            int mouseX = point.X;
            int mouseY = point.Y;

            Vector2 shoot_origin = slingshot.GetShootOrigin(who);
            float frontArmRotation = (float)Math.Atan2((float)mouseY - shoot_origin.Y, (float)mouseX - shoot_origin.X) + (float)Math.PI;

            frontArmRotation -= (float)Math.PI;
            if (frontArmRotation < 0f)
            {
                frontArmRotation += (float)Math.PI * 2f;
            }

            return frontArmRotation;
        }
    }
}
