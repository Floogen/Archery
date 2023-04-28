using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archery.Framework.Utilities
{
    internal class Toolkit
    {
        internal static float IncrementAndGetLayerDepth(ref float layerDepth)
        {
            layerDepth += 0.00001f;
            return layerDepth;
        }
    }
}
