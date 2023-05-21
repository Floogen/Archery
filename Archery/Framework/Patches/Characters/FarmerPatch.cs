using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using HarmonyLib;
using Microsoft.Xna.Framework;
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
            harmony.Patch(AccessTools.Method(_object, nameof(Farmer.Update), new[] { typeof(GameTime), typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(UpdatePostfix)));
        }

        private static void IsCarringPostfix(Farmer __instance, ref Object __result)
        {
            if (Arrow.GetModel<AmmoModel>(__result) is AmmoModel arrowModel && arrowModel is not null)
            {
                __result = null;
            }
        }

        private static void UpdatePostfix(Farmer __instance, GameTime time, GameLocation location)
        {
            // Update the special attack cooldown, if applicable
            if (Bow.ActiveCooldown >= 0)
            {
                Bow.ActiveCooldown -= time.ElapsedGameTime.Milliseconds;
                if (Bow.ActiveCooldown <= 0)
                {
                    Bow.CooldownAdditiveScale = 0.5f;
                }
            }

            if (Bow.CooldownAdditiveScale >= 0)
            {
                Bow.CooldownAdditiveScale -= 0.03f;
            }
        }
    }
}
