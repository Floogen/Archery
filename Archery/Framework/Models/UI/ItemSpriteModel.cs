using Archery.Framework.Models.Generic;
using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Objects.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;

namespace Archery.Framework.Models.Display
{
    public class ItemSpriteModel
    {
        public Rectangle Source { get; set; }
        public float Scale { get; set; } = 4f;
        public Vector2 Offset { get; set; }

        public bool FlipHorizontally { get; set; }
        public bool FlipVertically { get; set; }

        // Only used with overworld sprites 
        public bool DisableRotation { get; set; }

        public List<Condition> Conditions { get; set; } = new List<Condition>();


        public SpriteEffects GetSpriteEffects()
        {
            if (FlipHorizontally && FlipVertically)
            {
                return SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
            else if (FlipHorizontally)
            {
                return SpriteEffects.FlipHorizontally;
            }
            else if (FlipVertically)
            {
                return SpriteEffects.FlipVertically;
            }

            return SpriteEffects.None;
        }

        internal bool AreConditionsValid(Farmer who)
        {
            Archery.conditionManager.Track(this);

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
                else if (condition.Name is Condition.Type.IsLoaded)
                {
                    passedCheck = condition.IsValid(Bow.IsLoaded(who.CurrentTool));
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
