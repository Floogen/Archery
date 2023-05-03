﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Utilities
{
    internal class Toolkit
    {
        internal const string ARENA_MAP_NAME = "Custom_PeacefulEnd.Archery.Arena";
        internal static Texture2D _recolorableBaseTexture;
        internal static Color _hitboxColor = new Color(216, 55, 0, 210);

        internal static float IncrementAndGetLayerDepth(ref float layerDepth)
        {
            layerDepth += 0.00001f;
            return layerDepth;
        }

        internal static void TeleportToArena(string command, string[] args)
        {
            // Create the arena if needed
            if (Game1.locations.Any(l => l.Name == ARENA_MAP_NAME) is false)
            {
                var arena = new GameLocation(Archery.assetManager.arenaMapPath, ARENA_MAP_NAME);
                Game1.locations.Add(arena);

                // Add the braziers
                List<Vector2> brazierTiles = new List<Vector2>()
                {
                    new Vector2(11, 15),
                    new Vector2(21, 15),
                    new Vector2(11, 23),
                    new Vector2(21, 23),
                    new Vector2(9, 18),
                    new Vector2(23, 18)
                };

                foreach (var tile in brazierTiles)
                {
                    if (arena.objects.ContainsKey(tile))
                    {
                        continue;
                    }

                    Torch torch = new Torch(tile, 149, bigCraftable: true);
                    torch.shakeTimer = 25;
                    if (torch.placementAction(arena, (int)tile.X * 64, (int)tile.Y * 64, null))
                    {
                        var actualTorch = arena.getObjectAtTile((int)tile.X, (int)tile.Y);
                        actualTorch.IsOn = true;
                        actualTorch.initializeLightSource(actualTorch.TileLocation);
                    }
                }
            }

            // Warp the farmer to the arena
            Game1.warpFarmer(ARENA_MAP_NAME, 16, 19, false);
        }

        internal static void DrawHitBox(SpriteBatch b, Rectangle bounds, float rotation = 0f)
        {
            if (_recolorableBaseTexture is null)
            {
                _recolorableBaseTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                _recolorableBaseTexture.SetData(new[] { Color.White });
            }

            b.Draw(_recolorableBaseTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(bounds.X, bounds.Y)), bounds, _hitboxColor, rotation, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}