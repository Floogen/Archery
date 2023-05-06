﻿using Archery.Framework.Models;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

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
            harmony.Patch(AccessTools.Constructor(_object, new[] { typeof(List<ISalable>), typeof(int), typeof(string), typeof(Func<ISalable, Farmer, int, bool>), typeof(Func<ISalable, bool>), typeof(string) }), postfix: new HarmonyMethod(GetType(), nameof(ShopMenuPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(ShopMenu.setItemPriceAndStock), new[] { typeof(Dictionary<ISalable, int[]>) }), postfix: new HarmonyMethod(GetType(), nameof(SetItemPriceAndStockPostfix)));
        }

        private static void ShopMenuPostfix(ShopMenu __instance, ref List<ISalable> ___forSale, ref Dictionary<ISalable, int[]> ___itemPriceAndStock, List<ISalable> itemsForSale, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        {
            _shopOwner = who;
            if (who is null && String.IsNullOrEmpty(__instance.storeContext))
            {
                return;
            }

            HandleCustomStock(__instance);
        }

        private static void SetItemPriceAndStockPostfix(ShopMenu __instance, Dictionary<ISalable, int[]> new_stock)
        {
            HandleCustomStock(__instance);
        }

        private static void HandleCustomStock(ShopMenu shopMenu)
        {
            // Add the weapons and ammo
            foreach (var model in Archery.modelManager.GetModelsForSale())
            {
                if (String.Equals(_shopOwner, model.Shop.Owner, StringComparison.OrdinalIgnoreCase) is false && String.Equals(shopMenu.storeContext, model.Shop.Context, StringComparison.OrdinalIgnoreCase) is false)
                {
                    continue;
                }
                else if (model.Shop.HasRequirements(Game1.player) is false)
                {
                    continue;
                }

                Item item;
                switch (model)
                {
                    case WeaponModel weaponModel:
                        item = Bow.CreateInstance(weaponModel);
                        break;
                    case AmmoModel ammoModel:
                        item = Arrow.CreateInstance(ammoModel);
                        break;
                    default:
                        continue;
                }
                item.Stack = model.Shop.HasInfiniteStock() ? int.MaxValue : model.Shop.Stock;

                shopMenu.forSale.Add(item);
                shopMenu.itemPriceAndStock.Add(item, new int[2]
                {
                    model.Shop.Price,
                    item.Stack
                });
            }

            // Add the recipes
            foreach (var recipe in Archery.modelManager.GetRecipesForSale())
            {
                if (String.Equals(_shopOwner, recipe.Shop.Owner, StringComparison.OrdinalIgnoreCase) is false && String.Equals(shopMenu.storeContext, recipe.Shop.Context, StringComparison.OrdinalIgnoreCase) is false)
                {
                    continue;
                }
                else if (recipe.Shop.HasRequirements(Game1.player) is false)
                {
                    continue;
                }

                if (Game1.player.knowsRecipe(recipe.ParentId))
                {
                    continue;
                }

                Item item;
                switch (Archery.modelManager.GetSpecificModel<BaseModel>(recipe.ParentId))
                {
                    case WeaponModel weaponModel:
                        item = Bow.CreateInstance(weaponModel);
                        break;
                    case AmmoModel ammoModel:
                        item = Arrow.CreateInstance(ammoModel);
                        break;
                    default:
                        continue;
                }
                item.Stack = 1;

                shopMenu.forSale.Add(item);
                shopMenu.itemPriceAndStock.Add(item, new int[2]
                {
                    recipe.Shop.Price,
                    item.Stack
                });
            }
        }
    }
}