using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction;

/// <summary>The import pipeline's deconstruction of a position into its coordinates.</summary>
internal static class PositionComponents
{
    /// <summary>The coordinates of one position.</summary>
    extension<TSpace>(Position16<TSpace> position)
        where TSpace : ISpatialSpace
    {
        /// <summary>Splits the position into its coordinates.</summary>
        /// <param name="x">The horizontal coordinate.</param>
        /// <param name="y">The vertical coordinate.</param>
        internal void Deconstruct(out ushort x, out ushort y)
        {
            x = position.X;
            y = position.Y;
        }
    }
}
