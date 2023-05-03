using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;

namespace Archery.Framework.Patches.Objects
{
    internal class ToolPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Tool);

        public ToolPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "get_description", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));
            harmony.Patch(AccessTools.Method(typeof(Item), nameof(Item.canBeTrashed), null), postfix: new HarmonyMethod(GetType(), nameof(CanBeTrashedPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Tool.beginUsing), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(BeginUsingPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.tickUpdate), new[] { typeof(GameTime), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(TickUpdatePrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.DoFunction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(DoFunctionPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Tool.endUsing), new[] { typeof(GameLocation), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(EndUsingPrefix)));
        }
        private static void GetNamePostfix(Tool __instance, ref string __result)
        {
            if (Bow.IsValid(__instance))
            {
                __result = Bow.GetName(__instance);
                return;
            }
        }

        private static void GetDescriptionPostfix(Tool __instance, ref string __result)
        {
            if (Bow.IsValid(__instance))
            {
                __result = Bow.GetDescription(__instance);
                return;
            }
        }

        private static void CanBeTrashedPostfix(Tool __instance, ref bool __result)
        {
            if (Bow.IsValid(__instance))
            {
                __result = true;
                return;
            }
        }

        private static bool BeginUsingPrefix(Tool __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            if (Bow.IsValid(__instance))
            {
                __result = true;
                return Bow.Use(__instance, location, x, y, who);
            }

            return true;
        }

        private static bool TickUpdatePrefix(Tool __instance, ref Farmer ___lastUser, GameTime time, Farmer who)
        {
            // TODO: Implement special attack cooldown here?
            return true;
        }

        private static bool DoFunctionPrefix(Tool __instance, ref Farmer ___lastUser, GameLocation location, int x, int y, int power, Farmer who)
        {
            if (Bow.IsValid(__instance))
            {
                return false;
            }

            return true;
        }

        private static bool EndUsingPrefix(Tool __instance, GameLocation location, Farmer who)
        {
            if (Bow.IsValid(__instance))
            {
                who.forceCanMove();
                return false;
            }

            return true;
        }
    }
}
