using Archery.Framework.Models;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace Archery.Framework.Objects
{
    internal class InstancedObject
    {
        public static string GetKey(Item item)
        {
            if (item is not null)
            {
                if (item.modData.ContainsKey(ModDataKeys.WEAPON_FLAG))
                {
                    return ModDataKeys.WEAPON_FLAG;
                }
                else if (item.modData.ContainsKey(ModDataKeys.AMMO_FLAG))
                {
                    return ModDataKeys.AMMO_FLAG;
                }
            }

            return String.Empty;
        }

        public static bool IsValid(Item item)
        {
            if (item is not null && item.modData.ContainsKey(GetKey(item)))
            {
                return true;
            }

            return false;
        }

        public static string GetInternalId(Item item)
        {
            if (IsValid(item) is false || item.modData.TryGetValue(GetKey(item), out var id) is false)
            {
                return String.Empty;
            }

            return id;
        }

        internal static T GetModel<T>(Item item) where T : BaseModel
        {
            if (IsValid(item) is true)
            {
                var id = GetInternalId(item);
                if (Archery.modelManager.GetSpecificModel<BaseModel>(id) is BaseModel baseModel && baseModel is not null)
                {
                    return (T)Archery.modelManager.GetSpecificModel<BaseModel>(id);
                }
            }

            return null;
        }

        internal static string GetName(Item item)
        {
            if (IsValid(item) is true)
            {
                var model = GetModel<BaseModel>(item);
                if (model is not null)
                {
                    return model.Name;
                }
            }

            return "DEFAULT NAME";
        }

        internal static string GetDescription(Item item)
        {
            if (IsValid(item) is true)
            {
                var model = GetModel<BaseModel>(item);
                if (model is not null)
                {
                    return model.Name;
                }
            }

            return "DEFAULT DESCRIPTION";
        }
    }
}
