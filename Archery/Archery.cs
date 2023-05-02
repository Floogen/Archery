using Archery.Framework.Interfaces;
using Archery.Framework.Managers;
using Archery.Framework.Objects.Weapons;
using Archery.Framework.Patches.Objects;
using Archery.Framework.Utilities;
using FishingTrawler.Framework.Patches.Objects;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

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

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;

            // Load managers
            apiManager = new ApiManager(monitor);
            assetManager = new AssetManager(modHelper);

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
            helper.ConsoleCommands.Add("archery_get_bow", "Gives a basic bow.\n\nUsage: archery_get_bow", delegate { Game1.player.addItemByMenuIfNecessary(Bow.CreateInstance()); });
            helper.ConsoleCommands.Add("archery_arena", "Gives a basic bow.\n\nUsage: archery_arena", Toolkit.TeleportToArena);

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
        }

        private void OnVanillaRecolorMethodTriggered(object sender, EventArgs e)
        {
            RendereringHelper.RecolorBowArms(Game1.player);
        }
    }
}
