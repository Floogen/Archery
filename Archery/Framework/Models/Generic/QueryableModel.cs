using StardewValley;

namespace Archery.Framework.Models.Generic
{
    public class QueryableModel
    {
        public string UnlockCondition { get; set; }

        internal bool HasRequirements(Farmer who)
        {
            return GameStateQuery.CheckConditions(UnlockCondition, player: who);
        }
    }
}
