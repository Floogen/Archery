using Archery.Framework.Interfaces;
using StardewModdingAPI;

namespace Archery.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IFashionSenseApi _fashionSenseApi;

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
    }
}