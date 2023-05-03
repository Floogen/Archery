using Archery.Framework.Interfaces;
using StardewModdingAPI;

namespace Archery.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IFashionSenseApi _fashionSenseApi;
        private IDynamicGameAssetsApi dynamicGameAssetsApi;

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
    }
}