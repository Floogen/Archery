using Archery.Framework.Models.Generic;
using System;

namespace Archery.Framework.Models.Crafting
{
    public class ShopModel : QueryableModel
    {
        public string Owner { get; set; }
        public string Context { get; set; }

        public int Stock { get; set; } = -1;
        internal int? RemainingStock { get; set; }
        public int Price { get; set; }

        internal int GetActualStock()
        {
            return RemainingStock is null ? Stock : RemainingStock.Value;
        }

        internal bool HasInfiniteStock()
        {
            return Stock == -1;
        }

        internal bool IsValid()
        {
            if (String.IsNullOrEmpty(Owner) && String.IsNullOrEmpty(Context))
            {
                return false;
            }

            if (HasInfiniteStock() is false)
            {
                if (Stock <= 0 || (RemainingStock is not null && RemainingStock <= 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
