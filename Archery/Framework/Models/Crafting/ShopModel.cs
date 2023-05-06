using Archery.Framework.Models.Generic;
using System;

namespace Archery.Framework.Models.Crafting
{
    public class ShopModel : QueryableModel
    {
        public string Owner { get; set; }
        public string Context { get; set; }

        public int Stock { get; set; } = 1;
        public int Price { get; set; }

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

            if (Stock < 0 && HasInfiniteStock() is false)
            {
                return false;
            }

            return true;
        }
    }
}
