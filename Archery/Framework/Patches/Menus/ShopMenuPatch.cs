using Archery.Framework.Models;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Patches.Objects
{
    internal class ShopMenuPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(ShopMenu);
        private static string _shopOwner;

        public ShopMenuPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, "tryToPurchaseItem", new[] { typeof(ISalable), typeof(ISalable), typeof(int), typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(TryToPurchaseItemPostfix)));
        }

        private static void TryToPurchaseItemPostfix(ShopMenu __instance, ISalable item, ref ISalable held_item, int stockToBuy, int x, int y)
        {
            Item itemForSale = item as Item;
            if (InstancedObject.IsValid(itemForSale) && Bow.GetModel<BaseModel>(itemForSale) is BaseModel model)
            {
                if (InstancedObject.IsRecipe(itemForSale))
                {
                    try
                    {
                        Game1.player.craftingRecipes.Add(model.Id, 0);
                        Game1.playSound("newRecipe");
                    }
                    catch (Exception)
                    {
                        _monitor.Log($"Failed to learn custom recipe {model.Id} in shop {_shopOwner} at {__instance.ShopId}!");
                    }

                    held_item = null;
                    __instance.heldItem = null;
                }
            }
        }
    }
}