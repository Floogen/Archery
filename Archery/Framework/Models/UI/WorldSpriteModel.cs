using Archery.Framework.Models.Enums;
using Archery.Framework.Models.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Models.Display
{
    public class WorldSpriteModel : ItemSpriteModel
    {
        // Only used by WeaponModel
        public List<ArmSpriteModel> Arms { get; set; } = new List<ArmSpriteModel>();

        // Only used by WeaponModel
        public bool HideAmmo { get; set; }
        public Vector2 AmmoOffset { get; set; }

        internal ArmSpriteModel GetArmSprite(ArmType armType)
        {
            return Arms.FirstOrDefault(a => a.Type == armType);
        }
    }
}
