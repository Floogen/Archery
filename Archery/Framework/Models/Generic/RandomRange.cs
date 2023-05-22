using Archery.Framework.Interfaces.Internal;
using System;

namespace Archery.Framework.Models.Generic
{
    public class RandomRange : IRandomRange
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public int Get(Random random, int minOffset = 0, int maxOffset = 0)
        {
            return random.Next(Min + minOffset, Max + maxOffset + 1);
        }
    }
}
