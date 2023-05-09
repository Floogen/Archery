using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace Archery.Framework.Patches.Characters
{
    internal class FarmerPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Farmer);

        public FarmerPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_ActiveObject", null), postfix: new HarmonyMethod(GetType(), nameof(IsCarringPostfix)));
        }

        private static void IsCarringPostfix(Farmer __instance, ref Object __result)
        {
            if (Arrow.GetModel<AmmoModel>(__result) is AmmoModel arrowModel && arrowModel is not null)
            {
                __result = null;
            }
        }
    }
}
