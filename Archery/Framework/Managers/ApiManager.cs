using Archery.Framework.Interfaces;
using Archery.Framework.Interfaces.Internal;
using Archery.Framework.Models.Crafting;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using Leclair.Stardew.BetterCrafting;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using System;
using System.Linq;

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

        public void RegisterNativeSpecialAttacks()
        {
            Archery.internalApi.RegisterSpecialAttack(Archery.manifest, "Snapshot", WeaponType.Bow, () => "Snapshot", () => "Fires two arrows in quick succession.", () => 3000, SnapshotSpecialAttack);
            Archery.internalApi.RegisterSpecialAttack(Archery.manifest, "Snipe", WeaponType.Bow, () => "Snipe", () => "Instantly fires an arrow with increased speed. Guaranteed to critical hit.", () => 3000, SnipeSpecialAttack);
        }

        private bool SnapshotSpecialAttack(ISpecialAttack specialAttack)
        {
            var slingshot = specialAttack.Slingshot;

            int shotsFired = 0;
            if (slingshot.modData.TryGetValue(ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT, out string rawShotsFired) is false || int.TryParse(rawShotsFired, out shotsFired) is false)
            {
                slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = shotsFired.ToString();
            }

            var currentChargeTime = slingshot.GetSlingshotChargeTime();
            if (currentChargeTime < 0.5f)
            {
                Archery.internalApi.SetChargePercentage(Archery.manifest, slingshot, 0.5f);
            }
            else if (currentChargeTime >= 1f)
            {
                Archery.internalApi.PerformFire(Archery.manifest, slingshot, specialAttack.Location, specialAttack.Farmer);
                shotsFired++;
            }
            slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = shotsFired.ToString();

            if (shotsFired >= 2)
            {
                slingshot.modData[ModDataKeys.SPECIAL_ATTACK_SNAPSHOT_COUNT] = 0.ToString();

                return false;
            }

            return true;
        }

        private bool SnipeSpecialAttack(ISpecialAttack specialAttack)
        {
            var slingshot = specialAttack.Slingshot;

            var currentChargeTime = slingshot.GetSlingshotChargeTime();
            if (currentChargeTime < 0.7f)
            {
                Archery.internalApi.SetChargePercentage(Archery.manifest, slingshot, 0.7f);
            }
            else if (currentChargeTime >= 1f)
            {
                var firedResponse = Archery.internalApi.PerformFire(Archery.manifest, slingshot, specialAttack.Location, specialAttack.Farmer);
                if (firedResponse.Key is true && firedResponse.Value is BasicProjectile projectile)
                {
                    // Get the internal projectile data
                    var dataResponse = Archery.internalApi.GetProjectileData(Archery.manifest, projectile);
                    if (dataResponse.Key is false)
                    {
                        return false;
                    }

                    var projectileData = dataResponse.Value;

                    // Double the velocity
                    projectileData.Velocity *= 2;

                    // Guarantee critical hit
                    projectileData.CriticalChance = 1f;

                    // Set the internal projectile data
                    Archery.internalApi.SetProjectileData(Archery.manifest, projectile, projectileData);
                }

                return false;
            }

            return true;
        }
    }
}