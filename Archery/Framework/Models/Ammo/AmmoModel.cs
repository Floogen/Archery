using Archery.Framework.Models.Enums;

namespace Archery.Framework.Models.Weapons
{
    public class AmmoModel : BaseModel
    {
        public AmmoType Type { get; set; }

        public int BaseDamage { get; set; }
    }
}
