using Archery.Framework.Interfaces;
using Archery.Framework.Managers;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Weapons;
using Archery.Framework.Patches.Objects;
using Archery.Framework.Utilities;
using FishingTrawler.Framework.Patches.Objects;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using System.Linq;

namespace Archery
{
    public class Archery : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        // Managers
        internal static ApiManager apiManager;
        internal static AssetManager assetManager;
        internal static ModelManager modelManager;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);
            modelManager = new ModelManager(monitor);

            // Load our Harmony patches
            try
            {
                var harmony = new Harmony(ModManifest.UniqueID);

                // Apply Object patches
                new ObjectPatch(monitor, modHelper).Apply(harmony);
                new ToolPatch(monitor, modHelper).Apply(harmony);
                new SlingshotPatch(monitor, modHelper).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("archery_get_bow", "Gives a random bow, unless a bow id is given.\n\nUsage: archery_get_bow [bow_id]", Toolkit.GiveBow);
            helper.ConsoleCommands.Add("archery_get_arrow", "Gives a random arrow, unless an arrow id is given.\n\nUsage: archery_get_arrow [arrow_id]", Toolkit.GiveArrow);
            helper.ConsoleCommands.Add("archery_arena", "Teleports to an arena for debugging bows.\n\nUsage: archery_arena", Toolkit.TeleportToArena);
            helper.ConsoleCommands.Add("archery_reload", "Reloads all Archery content packs. Can specify a manifest unique ID to only reload that pack.\n\nUsage: archery_reload [manifest_unique_id]", ReloadArchery);

            // Hook into the game events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("PeacefulEnd.FashionSense") && apiManager.HookIntoFashionSense(Helper))
            {
                apiManager.GetFashionSenseApi().SetSpriteDirtyTriggered += OnVanillaRecolorMethodTriggered;

                apiManager.GetFashionSenseApi().RegisterAppearanceDrawOverride(IFashionSenseApi.Type.Sleeves, ModManifest, Bow.Draw);
            }

            // Load any owned content packs
            LoadContentPacks();
        }

        private void OnVanillaRecolorMethodTriggered(object sender, EventArgs e)
        {
            RendereringHelper.RecolorBowArms(Game1.player);
        }

        private void ReloadArchery(string command, string[] args)
        {
            var packFilter = args.Length > 0 ? args[0] : null;
            LoadContentPacks(packId: packFilter);
        }

        private void LoadContentPacks(bool silent = false, string packId = null)
        {
            modelManager.Reset(packId);

            // Load owned content packs
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned().Where(c => String.IsNullOrEmpty(packId) is true || c.Manifest.UniqueID.Equals(packId, StringComparison.OrdinalIgnoreCase)))
            {
                Monitor.Log($"Loading data from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", silent ? LogLevel.Trace : LogLevel.Debug);

                // Load Weapons
                Monitor.Log($"Loading weapons from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddWeaponContentPacks(contentPack);

                // Load Ammo
                Monitor.Log($"Loading ammo from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Trace);
                AddAmmoContentPacks(contentPack);
            }
        }

        private void AddWeaponContentPacks(IContentPack contentPack)
        {
            string folderKeyword = "Weapons";
            string fileKeyword = "weapon";

            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, folderKeyword));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No {folderKeyword} folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hairFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (hairFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under {folderKeyword} for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the hairs
                foreach (var textureFolder in hairFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, $"{fileKeyword}.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a {fileKeyword}.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, $"{fileKeyword}.json");

                    // Parse the model and assign it the content pack's owner
                    WeaponModel weaponModel = contentPack.ReadJsonFile<WeaponModel>(modelPath);
                    weaponModel.ContentPack = contentPack;
                    weaponModel.Id = String.Concat(contentPack.Manifest.UniqueID, "/", weaponModel.Type, "/", weaponModel.Name);

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(weaponModel.Name))
                    {
                        Monitor.Log($"Unable to add {fileKeyword} from {weaponModel.ContentPack.Manifest.Author}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Verify that a hairstyle with the name doesn't exist in this pack
                    if (modelManager.GetSpecificModel<WeaponModel>(weaponModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add {fileKeyword} from {contentPack.Manifest.Name}: This pack already contains a hairstyle with the name of {weaponModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, $"{fileKeyword}.png")))
                    {
                        Monitor.Log($"Unable to add {fileKeyword} for {weaponModel.Name} from {contentPack.Manifest.Name}: No associated {fileKeyword}.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    weaponModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, $"{fileKeyword}.png")).Name);

                    // Track the model
                    modelManager.AddModel(weaponModel);

                    // Log it
                    Monitor.Log(weaponModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading {fileKeyword} from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }

        private void AddAmmoContentPacks(IContentPack contentPack)
        {
            string folderKeyword = "Ammo";
            string fileKeyword = "ammo";

            try
            {
                var directoryPath = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, folderKeyword));
                if (!directoryPath.Exists)
                {
                    Monitor.Log($"No {folderKeyword} folder found for the content pack {contentPack.Manifest.Name}", LogLevel.Trace);
                    return;
                }

                var hairFolders = directoryPath.GetDirectories("*", SearchOption.AllDirectories);
                if (hairFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under {folderKeyword} for the content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                    return;
                }

                // Load in the hairs
                foreach (var textureFolder in hairFolders)
                {
                    if (!File.Exists(Path.Combine(textureFolder.FullName, $"{fileKeyword}.json")))
                    {
                        if (textureFolder.GetDirectories().Count() == 0)
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a {fileKeyword}.json under {textureFolder.Name}", LogLevel.Warn);
                        }

                        continue;
                    }

                    var parentFolderName = textureFolder.Parent.FullName.Replace(contentPack.DirectoryPath + Path.DirectorySeparatorChar, String.Empty);
                    var modelPath = Path.Combine(parentFolderName, textureFolder.Name, $"{fileKeyword}.json");

                    // Parse the model and assign it the content pack's owner
                    AmmoModel ammoModel = contentPack.ReadJsonFile<AmmoModel>(modelPath);
                    ammoModel.ContentPack = contentPack;
                    ammoModel.Id = String.Concat(contentPack.Manifest.UniqueID, "/", ammoModel.Type, "/", ammoModel.Name);

                    // Verify the required Name property is set
                    if (String.IsNullOrEmpty(ammoModel.Name))
                    {
                        Monitor.Log($"Unable to add {fileKeyword} from {ammoModel.ContentPack.Manifest.Author}: Missing the Name property", LogLevel.Warn);
                        continue;
                    }

                    // Verify that a hairstyle with the name doesn't exist in this pack
                    if (modelManager.GetSpecificModel<WeaponModel>(ammoModel.Id) != null)
                    {
                        Monitor.Log($"Unable to add {fileKeyword} from {contentPack.Manifest.Name}: This pack already contains a hairstyle with the name of {ammoModel.Name}", LogLevel.Warn);
                        continue;
                    }

                    // Verify we are given a texture and if so, track it
                    if (!File.Exists(Path.Combine(textureFolder.FullName, $"{fileKeyword}.png")))
                    {
                        Monitor.Log($"Unable to add {fileKeyword} for {ammoModel.Name} from {contentPack.Manifest.Name}: No associated {fileKeyword}.png given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the texture
                    ammoModel.Texture = contentPack.ModContent.Load<Texture2D>(contentPack.ModContent.GetInternalAssetName(Path.Combine(parentFolderName, textureFolder.Name, $"{fileKeyword}.png")).Name);

                    // Track the model
                    modelManager.AddModel(ammoModel);

                    // Log it
                    Monitor.Log(ammoModel.ToString(), LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error loading {fileKeyword} from content pack {contentPack.Manifest.Name}: {ex}", LogLevel.Error);
            }
        }
    }
}
