using Archery.Framework.Models.Generic;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
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

        internal static void PlaySound(Sound sound, string sourceId, Vector2 sourcePosition)
        {
            if (sound is null || sound.IsValid() is false || Game1.soundBank is null)
            {
                return;
            }

            int actualPitch = 1200;
            if (sound.Pitch != -1)
            {
                actualPitch = sound.Pitch + sound.PitchRandomness.Get(Game1.random);
            }

            try
            {
                ICue cue = Game1.soundBank.GetCue(sound.Name);
                cue.SetVariable("Pitch", actualPitch);

                var actualVolume = sound.Volume;
                if (sound.MaxDistance > 0)
                {
                    float distance = Vector2.Distance(sourcePosition, Game1.player.getStandingPosition());
                    actualVolume = Math.Min(1f, 1f - distance / sound.MaxDistance) * sound.Volume * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel);
                }
                cue.Volume = actualVolume;

                cue.Play();
                try
                {
                    if (!cue.IsPitchBeingControlledByRPC)
                    {
                        cue.Pitch = Utility.Lerp(-1f, 1f, actualPitch / 2400f);
                    }
                }
                catch (Exception ex)
                {
                    Archery.monitor.LogOnce($"Failed to play ({sound.Name}) given for {sourceId}: {ex}", StardewModdingAPI.LogLevel.Warn);
                }
            }
            catch (Exception ex2)
            {
                Archery.monitor.LogOnce($"Failed to play ({sound.Name}) given for {sourceId}: {ex2}", StardewModdingAPI.LogLevel.Warn);
            }
        }

        internal static void GiveBow(string command, string[] args)
        {
            var bowFilter = args.Length > 0 ? args[0] : null;

            WeaponModel weaponModel = null;
            if (string.IsNullOrEmpty(bowFilter) is false)
            {
                weaponModel = Archery.modelManager.GetSpecificModel<WeaponModel>(bowFilter);
            }
            else
            {
                weaponModel = Archery.modelManager.GetRandomWeaponModel(Models.Enums.WeaponType.Bow);
            }

            if (weaponModel is not null)
            {
                Game1.player.addItemByMenuIfNecessary(Bow.CreateInstance(weaponModel));
            }
        }

        internal static void GiveArrow(string command, string[] args)
        {
            var arrowFilter = args.Length > 0 ? args[0] : null;

            AmmoModel ammoModel = null;
            if (string.IsNullOrEmpty(arrowFilter) is false)
            {
                ammoModel = Archery.modelManager.GetSpecificModel<AmmoModel>(arrowFilter);
            }
            else
            {
                ammoModel = Archery.modelManager.GetRandomAmmoModel(Models.Enums.AmmoType.Arrow);
            }

            if (ammoModel is not null)
            {
                Game1.player.addItemByMenuIfNecessary(Arrow.CreateInstance(ammoModel, 999));
            }
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

                // Add the target dummies, if DGA and the relevant pack is installed
                var dgaAPI = Archery.apiManager.GetDynamicGameAssetsApi();
                if (dgaAPI is not null)
                {
                    List<Vector2> dummyTiles = new List<Vector2>()
                    {
                        new Vector2(16, 15),
                        new Vector2(13, 19),
                        new Vector2(20, 19),
                        new Vector2(17, 22)
                    };

                    foreach (var tile in dummyTiles)
                    {
                        var targetDummy = dgaAPI.SpawnDGAItem("PeacefulEnd.PracticeDummy/PracticeDummy") as StardewValley.Object;
                        if (arena.objects.ContainsKey(tile) || targetDummy is null)
                        {
                            continue;
                        }

                        targetDummy.placementAction(arena, (int)tile.X * 64, (int)tile.Y * 64, null);
                    }
                }
            }

            // Give the player bow and arrows
            GiveBow(null, new string[0]);
            GiveArrow(null, new string[0]);

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
