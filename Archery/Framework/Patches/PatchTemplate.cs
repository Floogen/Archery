using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archery.Framework.Patches
{
    internal abstract class PatchTemplate
    {
        internal static IMonitor _monitor;
        internal static IModHelper _helper;

        internal PatchTemplate(IMonitor modMonitor, IModHelper modHelper)
        {
            _monitor = modMonitor;
            _helper = modHelper;
        }

        internal abstract void Apply(Harmony harmony);
    }
}
