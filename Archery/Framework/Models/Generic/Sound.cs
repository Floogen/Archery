using Archery.Framework.Interfaces.Internal;
using StardewValley;
using System;

namespace Archery.Framework.Models.Generic
{
    public class Sound : ISound
    {
        public string Name { get; set; }
        public int Pitch { get; set; } = -1;
        public IRandomRange PitchRandomness { get; set; }
        private float _volume { get; set; } = 1f;
        public float Volume { get { return _volume; } set { _volume = Utility.Clamp(value, 0f, 1f); } }
        public float MaxDistance { get; set; } = 1024f;

        internal bool IsValid()
        {
            try
            {
                Game1.soundBank.GetCue(Name);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
