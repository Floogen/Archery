using Archery.Framework.Interfaces;
using Leclair.Stardew.BetterCrafting;
using StardewModdingAPI;
using Archery.Framework.Models.Crafting;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using Archery.Framework.Models.Weapons;

namespace Archery.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IFashionSenseApi _fashionSenseApi;
        private IDynamicGameAssetsApi dynamicGameAssetsApi;
        private IBetterCraftingApi betterCraftingApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoFashionSense(IModHelper helper)
        {
            _fashionSenseApi = helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");

            if (_fashionSenseApi is null)
            {
                _monitor.Log("Failed to hook into PeacefulEnd.FashionSense.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into PeacefulEnd.FashionSense.", LogLevel.Debug);
            return true;
        }

        public IFashionSenseApi GetFashionSenseApi()
        {
            return _fashionSenseApi;
        }

        internal bool HookIntoDynamicGameAssets(IModHelper helper)
        {
            dynamicGameAssetsApi = helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");

            if (dynamicGameAssetsApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.DynamicGameAssets.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.DynamicGameAssets.", LogLevel.Debug);
            return true;
        }

        public IDynamicGameAssetsApi GetDynamicGameAssetsApi()
        {
            return dynamicGameAssetsApi;
        }

        internal bool HookIntoBetterCrafting(IModHelper helper)
        {
            betterCraftingApi = helper.ModRegistry.GetApi<IBetterCraftingApi>("leclair.bettercrafting");

            if (betterCraftingApi is null)
            {
                _monitor.Log("Failed to hook into leclair.bettercrafting.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into leclair.bettercrafting.", LogLevel.Debug);
            return true;
        }

        public IBetterCraftingApi GetBetterCraftingApi()
        {
            return betterCraftingApi;
        }

        public void SyncRecipesWithBetterCrafting()
        {
            // Create and add the recipe provider
            betterCraftingApi.AddRecipeProvider(new RecipeProvider(betterCraftingApi));

            // Get the available models IDs with valid recipes
            var validModelIds = Archery.modelManager.GetModelsWithValidRecipes().Select(m => m.Id);

            // Get a random bow ID that is unlocked
            var archeryIcon = String.Empty;

            var availableWeapons = Archery.modelManager.GetModelsWithValidRecipes().Where(m => m is WeaponModel && m.Recipe.HasRequirements(Game1.player)).ToList();
            if (availableWeapons.Count > 0)
            {
                archeryIcon = availableWeapons[Game1.random.Next(availableWeapons.Count)].Id;
            }

            // Create the Archery category
            betterCraftingApi.CreateDefaultCategory(false, "PeacefulEnd.Archery", () => "Archery", iconRecipe: archeryIcon);

            // Add to the Archery category
            betterCraftingApi.AddRecipesToDefaultCategory(false, "PeacefulEnd.Archery", validModelIds);
        }
    }
}