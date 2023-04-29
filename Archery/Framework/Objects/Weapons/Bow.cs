using StardewValley.Tools;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archery.Framework.Utilities;
using static Archery.Framework.Interfaces.IFashionSenseApi;
using Archery.Framework.Managers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.Monsters;

namespace Archery.Framework.Objects.Weapons
{
    internal class Bow
    {
        public static Slingshot CreateInstance()
        {
            var bow = new Slingshot();
            bow.modData[ModDataKeys.BOW_WEAPON_FLAG] = true.ToString();

            return bow;
        }

        public static bool IsValid(Tool tool)
        {
            if (tool is not null && tool.modData.ContainsKey(ModDataKeys.BOW_WEAPON_FLAG))
            {
                return true;
            }

            return false;
        }

        internal static bool Use(Tool tool, GameLocation location, int x, int y, Farmer who)
        {
            return false;
        }

        internal static bool Draw(IDrawTool drawTool)
        {
            Farmer who = drawTool.Farmer;

            if (who.UsingTool is false || IsValid(who.CurrentTool) is false)
            {
                return false;
            }

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
            int bowFrame = GetBowFrame(who.FacingDirection, currentChargePercentage);
            switch (who.FacingDirection)
            {
                case Game1.down:
                    // Draw the back arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the bow
                    specialOffset = new Vector2(4f - frontArmRotation * 2f, 0f);
                    drawTool.SpriteBatch.Draw(Archery.assetManager.baseBowTexture, baseOffset + specialOffset, new Rectangle(bowFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(0, 32, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                case Game1.right:
                case Game1.left:
                    var flipEffect = who.FacingDirection == Game1.left ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    originOffset = who.FacingDirection == Game1.left ? new Vector2(0f, -16f) : new Vector2(0f, -16f);
                    specialOffset = who.FacingDirection == Game1.left ? new Vector2(56f, -60f) : new Vector2(8f, -60f);

                    // Draw the back arm
                    // TODO: Get player's layer depth via IDrawTool
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(48, 32, 16, 32), drawTool.OverrideColor, frontArmRotation, drawTool.Origin + originOffset, 4f * drawTool.Scale, flipEffect, 5.9E-05f);

                    // Draw the bow
                    drawTool.SpriteBatch.Draw(Archery.assetManager.baseBowTexture, baseOffset + specialOffset + (who.FacingDirection == Game1.left ? new Vector2(-8f, 0f) : new Vector2(8f, 0f)), new Rectangle(bowFrame, 0, 16, 32), drawTool.OverrideColor, frontArmRotation, drawTool.Origin + originOffset, 4f * drawTool.Scale, flipEffect, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the front arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 32, 16, 32), drawTool.OverrideColor, frontArmRotation, drawTool.Origin + originOffset, 4f * drawTool.Scale, flipEffect, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                case Game1.up:
                    // Draw the back arm
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(movingArmStartingFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the bow
                    specialOffset = new Vector2((frontArmRotation - 6f) * 4f, 0f);
                    drawTool.SpriteBatch.Draw(Archery.assetManager.baseBowTexture, baseOffset + specialOffset, new Rectangle(bowFrame, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    drawTool.SpriteBatch.Draw(Archery.assetManager.recoloredArmsTexture, baseOffset + specialOffset, new Rectangle(0, 32, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Toolkit.IncrementAndGetLayerDepth(ref layerDepth));

                    return true;
                default:
                    return false;
            }
        }

        // TODO: Have the BowModel determine the frame value based on the charge percentage
        private static int GetBowFrame(int facingDirection, float bowChargePercentage)
        {
            int bowFrame = 0;
            int startingOffset = 0;
            switch (facingDirection)
            {
                case Game1.down:
                    break;
                case Game1.right:
                case Game1.left:
                    bowFrame = (bowChargePercentage > 0.8f ? 2 : bowChargePercentage > 0.5f ? 1 : 0);
                    startingOffset = 16;
                    break;
                case Game1.up:
                    startingOffset = 64;
                    break;
            }

            return (bowFrame * 16) + startingOffset;
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

        private static float GetFrontArmRotation(Farmer who, Slingshot slingshot)
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
