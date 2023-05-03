using Archery.Framework.Models;
using Archery.Framework.Models.Weapons;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace Archery.Framework.Managers
{
    internal class ModelManager
    {
        private IMonitor _monitor;
        private List<ContentPackBase> _contentPacks;
        private Dictionary<string, WeaponModel> _idToWeaponModels;
        private Dictionary<string, AmmoModel> _idToAmmoModels;

        public ModelManager(IMonitor monitor)
        {
            _monitor = monitor;

            _contentPacks = new List<ContentPackBase>();
            _idToWeaponModels = new Dictionary<string, WeaponModel>();
            _idToAmmoModels = new Dictionary<string, AmmoModel>();
        }
    }
}
