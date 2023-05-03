using Archery.Framework.Models;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Managers
{
    internal class ModelManager
    {
        private IMonitor _monitor;
        private List<BaseModel> _contentPackModels;

        public ModelManager(IMonitor monitor)
        {
            _monitor = monitor;

            Reset();
        }

        public void Reset(string packId = null)
        {
            if (String.IsNullOrEmpty(packId) is true)
            {
                _contentPackModels = new List<BaseModel>();
            }
            else
            {
                _contentPackModels = _contentPackModels.Where(a => a.ContentPack.Manifest.UniqueID.Equals(packId, StringComparison.OrdinalIgnoreCase) is false).ToList();
            }
        }

        internal void AddModel(BaseModel baseModel)
        {
            _contentPackModels.Add(baseModel);
        }

        internal T GetSpecificModel<T>(string modelId) where T : BaseModel
        {
            return (T)_contentPackModels.FirstOrDefault(t => String.Equals(t.Id, modelId, StringComparison.OrdinalIgnoreCase) && t is T);
        }
    }
}
