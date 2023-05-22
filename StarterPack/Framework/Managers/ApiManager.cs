using StardewModdingAPI;
using StarterPack.Framework.Interfaces;

namespace StarterPack.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IArcheryApi _archeryApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoArchery(IModHelper helper)
        {
            _archeryApi = helper.ModRegistry.GetApi<IArcheryApi>("PeacefulEnd.Archery");

            if (_archeryApi is null)
            {
                _monitor.Log("Failed to hook into PeacefulEnd.Archery.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into PeacefulEnd.Archery.", LogLevel.Debug);
            return true;
        }

        public IArcheryApi GetArcheryApi()
        {
            return _archeryApi;
        }
    }
}