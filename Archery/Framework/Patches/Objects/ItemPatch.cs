using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
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
            harmony.Patch(AccessTools.Method(_object, "get_Name", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Item.addToStack), new[] { typeof(Item) }), prefix: new HarmonyMethod(GetType(), nameof(AddToStackPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Item.canStackWith), new[] { typeof(ISalable) }), postfix: new HarmonyMethod(GetType(), nameof(CanStackWithPostfix)));
        }

        private static void GetNamePostfix(Item __instance, ref string __result)
        {
            if (Bow.GetModel<WeaponModel>(__instance) is WeaponModel weaponModel && weaponModel is not null)
            {
                __result = weaponModel.Id;
            }
        }

        private static bool AddToStackPrefix(Object __instance, ref int __result, Item otherStack)
        {
            if (Arrow.IsValid(__instance))
            {
                if (Arrow.IsValid(otherStack) && Arrow.GetInternalId(__instance) == Arrow.GetInternalId(otherStack))
                {
                    return true;
                }

                __result = otherStack.Stack;
                return false;
            }

            return true;
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