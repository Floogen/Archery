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
            Slingshot slingshot = who.CurrentTool as Slingshot;

            int backArmDistance = slingshot.GetBackArmDistance(who);
            float frontArmRotation = GetFrontArmRotation(who, slingshot);

            // Get the layer depth
            float layerDepth = drawTool.LayerDepthSnapshot;

            // Establish the offsets
            var specialOffset = Vector2.Zero;
            var baseOffset = drawTool.Position + drawTool.Origin + drawTool.PositionOffset + who.armOffset;

            switch (who.FacingDirection)
            {
                case Game1.down:
                    // Draw the back arm
                    specialOffset = new Vector2(0f, -backArmDistance / 2.5f);
                    drawTool.SpriteBatch.Draw(Archery.assetManager.bowArmsTexture, baseOffset + specialOffset, new Rectangle(0, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

                    // Draw the bow
                    specialOffset = new Vector2(frontArmRotation * 2f, 0f);
                    drawTool.SpriteBatch.Draw(Archery.assetManager.baseBowTexture, baseOffset + specialOffset, new Rectangle(0, 0, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.001f);

                    // Draw the front arm
                    specialOffset = Vector2.Zero;
                    drawTool.SpriteBatch.Draw(Archery.assetManager.bowArmsTexture, baseOffset + specialOffset, new Rectangle(0, 32, 16, 32), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 0.001f);
                    break;
            }

            return true;
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
