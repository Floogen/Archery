using System;

namespace Archery.Framework.Models.Generic
{
    public class RandomRange
    {
        public int Min { get; set; }
        public int Max { get; set; }

        internal int Get(Random random)
        {
            return random.Next(Min, Max + 1);
        }
    }
}
