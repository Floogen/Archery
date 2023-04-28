using Archery.Framework.Interfaces;
using Archery.Framework.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Reflection;
using System.Threading;
using static Archery.Framework.Interfaces.IFashionSenseApi;

namespace Archery.Framework
{
    public class Archery : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static ApiManager apiManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            apiManager = new ApiManager(monitor);

            // Hook into the game events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("PeacefulEnd.FashionSense") && apiManager.HookIntoFashionSense(Helper))
            {
                apiManager.GetFashionSenseApi().RegisterAppearanceDrawOverride(IFashionSenseApi.Type.Sleeves, ModManifest, DrawBow);
            }
        }

        private bool DrawBow(IDrawTool drawTool)
        {
            Farmer who = drawTool.Farmer;
            float layerDepth = drawTool.LayerDepthSnapshot;

            // Handle drawing slingshot
            if (who.usingSlingshot is false || who.CurrentTool is not Slingshot)
            {
                return false;
            }
            monitor.LogOnce($"[{DateTime.Now.ToString("T")}] {drawTool.AnimationFrame.frame}", LogLevel.Debug);

            drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + drawTool.Origin + drawTool.PositionOffset + who.armOffset, new Rectangle(drawTool.FarmerSourceRectangle.X + (drawTool.AnimationFrame.secondaryArm ? 192 : 96), drawTool.FarmerSourceRectangle.Y, drawTool.FarmerSourceRectangle.Width, drawTool.FarmerSourceRectangle.Height), drawTool.OverrideColor, drawTool.Rotation, drawTool.Origin, 4f * drawTool.Scale, drawTool.AnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

            Slingshot slingshot = who.CurrentTool as Slingshot;
            Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value)));
            int mouseX = point.X;
            int y = point.Y;
            int backArmDistance = slingshot.GetBackArmDistance(who);

            Vector2 shoot_origin = slingshot.GetShootOrigin(who);
            float frontArmRotation = (float)Math.Atan2((float)y - shoot_origin.Y, (float)mouseX - shoot_origin.X) + (float)Math.PI;
            if (Game1.options.useLegacySlingshotFiring is false)
            {
                frontArmRotation -= (float)Math.PI;
                if (frontArmRotation < 0f)
                {
                    frontArmRotation += (float)Math.PI * 2f;
                }
            }

            switch (drawTool.FacingDirection)
            {
                case 0:
                    drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(4f + frontArmRotation * 8f, -44f), new Rectangle(173, 238, 9, 14), Color.White, 0f, new Vector2(4f, 11f), 4f * drawTool.Scale, SpriteEffects.None, layerDepth + ((drawTool.FacingDirection != 0) ? 5.9E-05f : (-0.0005f)));
                    break;
                case 1:
                    {
                        drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(52 - backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(8f, 3f), 4f * drawTool.Scale, SpriteEffects.None, layerDepth + ((drawTool.FacingDirection != 0) ? 5.9E-05f : 0f));
                        drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(36f, -44f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation, new Vector2(0f, 3f), 4f * drawTool.Scale, SpriteEffects.None, layerDepth + ((drawTool.FacingDirection != 0) ? 1E-08f : 0f));
                        int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI / 2f) * -68.0);
                        int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI / 2f) * -68.0);
                        Utility.drawLineWithScreenCoordinates((int)(drawTool.Position.X + 52f - (float)backArmDistance), (int)(drawTool.Position.Y - 32f - 4f), (int)(drawTool.Position.X + 32f + (float)(slingshotAttachX / 2)), (int)(drawTool.Position.Y - 32f - 12f + (float)(slingshotAttachY / 2)), drawTool.SpriteBatch, Color.White);
                        break;
                    }
                case 3:
                    {
                        drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(40 + backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(9f, 4f), 4f * drawTool.Scale, SpriteEffects.FlipHorizontally, layerDepth + ((drawTool.FacingDirection != 0) ? 5.9E-05f : 0f));
                        drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(24f, -40f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation + (float)Math.PI, new Vector2(8f, 3f), 4f * drawTool.Scale, SpriteEffects.FlipHorizontally, layerDepth + ((drawTool.FacingDirection != 0) ? 1E-08f : 0f));
                        int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
                        int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
                        Utility.drawLineWithScreenCoordinates((int)(drawTool.Position.X + 4f + (float)backArmDistance), (int)(drawTool.Position.Y - 32f - 8f), (int)(drawTool.Position.X + 26f + (float)slingshotAttachX * 4f / 10f), (int)(drawTool.Position.Y - 32f - 8f + (float)slingshotAttachY * 4f / 10f), drawTool.SpriteBatch, Color.White);
                        break;
                    }
                case 2:
                    drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(4f, -32 - backArmDistance / 2), new Rectangle(148, 244, 4, 4), Color.White, 0f, Vector2.Zero, 4f * drawTool.Scale, SpriteEffects.None, layerDepth + ((drawTool.FacingDirection != 0) ? 5.9E-05f : 0f));
                    Utility.drawLineWithScreenCoordinates((int)(drawTool.Position.X + 16f), (int)(drawTool.Position.Y - 28f - (float)(backArmDistance / 2)), (int)(drawTool.Position.X + 44f - frontArmRotation * 10f), (int)(drawTool.Position.Y - 16f - 8f), drawTool.SpriteBatch, Color.White);
                    Utility.drawLineWithScreenCoordinates((int)(drawTool.Position.X + 16f), (int)(drawTool.Position.Y - 28f - (float)(backArmDistance / 2)), (int)(drawTool.Position.X + 56f - frontArmRotation * 10f), (int)(drawTool.Position.Y - 16f - 8f), drawTool.SpriteBatch, Color.White);
                    //drawTool.SpriteBatch.Draw(drawTool.BaseTexture, drawTool.Position + new Vector2(44f - frontArmRotation * 10f, -16f), new Rectangle(167, 235, 7, 9), Color.White, 0f, new Vector2(3f, 5f), 4f * drawTool.Scale, SpriteEffects.None, layerDepth + ((drawTool.FacingDirection != 0) ? 5.9E-05f : 0f));
                    break;
            }

            return true;
        }
    }
}
