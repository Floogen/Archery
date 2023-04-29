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
