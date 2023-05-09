using Archery.Framework.Objects.Items;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace Archery.Framework.Patches.Objects
{
    internal class ItemPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(Item);

        public ItemPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Item.canStackWith), new[] { typeof(ISalable) }), postfix: new HarmonyMethod(GetType(), nameof(CanStackWithPostfix)));
        }


        private static void CanStackWithPostfix(Item __instance, ref bool __result, ISalable other)
        {
            if (Arrow.IsValid(__instance))
            {
                var actualItem = other as Item;
                if (Arrow.IsValid(actualItem) && Arrow.GetInternalId(__instance) == Arrow.GetInternalId(actualItem))
                {
                    __result = true;
                    return;
                }

                __result = false;
            }
        }
    }
}