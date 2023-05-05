using Archery.Framework.Models.Enums;
using StardewModdingAPI.Enums;
using StardewValley;
using System;

namespace Archery.Framework.Models.Crafting
{
    public class RequirementModel
    {
        public RequirementType Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        internal bool IsValid()
        {
            if (Type is RequirementType.Unknown || String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(Value))
            {
                return false;
            }

            return true;
        }

        internal bool DoesFarmerPass(Farmer who)
        {
            if (IsValid() is false || Enum.TryParse<SkillType>(Name, out var skillType) is false)
            {
                return true;
            }
            else if (who is null || Type is RequirementType.MustBePurchased)
            {
                return false;
            }

            bool hasRequiredSkill = false;
            switch (Type)
            {
                case RequirementType.RelationshipHearts:
                    if (int.TryParse(Value, out int requiredHearts) && who.getFriendshipHeartLevelForNPC(Name) >= requiredHearts)
                    {
                        hasRequiredSkill = true;
                    }
                    break;
                case RequirementType.SkillLevel:
                    if (int.TryParse(Value, out int requiredLevel) && who.GetSkillLevel((int)skillType) >= requiredLevel)
                    {
                        hasRequiredSkill = true;
                    }
                    break;
            }

            return hasRequiredSkill;
        }
    }
}
