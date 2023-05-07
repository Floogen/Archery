using Archery.Framework.Models.Enums;
using Archery.Framework.Models.Generic;
using Archery.Framework.Models.UI;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Models.Display
{
    public class WorldSpriteModel : ItemSpriteModel
    {
        // Only used by WeaponModel
        public List<ArmSpriteModel> Arms { get; set; } = new List<ArmSpriteModel>();

        public List<Condition> Conditions { get; set; } = new List<Condition>();

        internal ArmSpriteModel GetArmSprite(ArmType armType)
        {
            return Arms.FirstOrDefault(a => a.Type == armType);
        }

        internal bool AreConditionsValid(Farmer who)
        {
            bool isValid = true;

            foreach (Condition condition in Conditions)
            {
                var passedCheck = false;

                // Check the conditions
                if (condition.Name is Condition.Type.CurrentChargingPercentage && who.CurrentTool is Slingshot slingshot && slingshot is not null)
                {
                    passedCheck = condition.IsValid(slingshot.GetSlingshotChargeTime());
                }
                else if (condition.Name is Condition.Type.IsUsingSpecificArrow && Arrow.GetModel<AmmoModel>(Bow.GetAmmoItem(who.CurrentTool)) is AmmoModel ammo && ammo is not null)
                {
                    passedCheck = condition.IsValid(ammo.Id);
                }

                // If the condition is independent and is true, then skip rest of evaluations
                if (condition.Independent && passedCheck)
                {
                    isValid = true;
                    break;
                }
                else if (isValid)
                {
                    isValid = passedCheck;
                }
            }

            return isValid;
        }
    }
}
