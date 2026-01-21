using Archery.Framework.Models;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using Archery.Framework.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using System;
using System.Linq;

namespace Archery.Framework.Patches.ItemTypeDefinitions
{
    internal class WeaponDataDefinitionPatch : PatchTemplate
    {
        private readonly System.Type _object = typeof(WeaponDataDefinition);

        public WeaponDataDefinitionPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(WeaponDataDefinition.CreateItem), new[] { typeof(ParsedItemData) }), postfix: new HarmonyMethod(GetType(), nameof(CreateItemPostfix)));
        }

        private static void CreateItemPostfix(WeaponDataDefinition __instance, ParsedItemData data, ref Item __result)
        {
            if (data == null)
            {
                return;
            }

            if (Archery.modelManager.GetAllModels().FirstOrDefault(m => m is WeaponModel && m.Id.Equals(data.ItemId, StringComparison.OrdinalIgnoreCase)) is WeaponModel weaponModel && weaponModel is not null)
            {
                var bow = new Slingshot();
                bow.modData[ModDataKeys.WEAPON_FLAG] = weaponModel.Id;

                // Hide attachment slot from bows with internal ammo
                if (weaponModel.UsesInternalAmmo())
                {
                    bow.numAttachmentSlots.Value = 0;
                }

                __result = bow;
            }
        }
    }
}