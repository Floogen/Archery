using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using static StardewValley.FarmerSprite;

namespace Archery.Framework.Interfaces
{
    public interface IDynamicGameAssetsApi
    {
        /// <summary>
        /// Spawn a DGA item, referenced with its full ID ("mod.id/ItemId").
        /// Some items, such as crafting recipes or crops, don't have an item representation.
        /// </summary>
        /// <param name="fullId">The full ID of the item to spawn.</param>
        /// <returns></returns>
        object SpawnDGAItem(string fullId);
    }
}
