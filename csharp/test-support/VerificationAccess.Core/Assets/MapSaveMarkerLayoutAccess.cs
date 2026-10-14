using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    /// <summary>Counts explicitly stored X and Y coordinates of save-marker layout edits.</summary>
    extension(MapSaveMarkerLayout self)
    {
        /// <summary>Counts each non-null coordinate component separately across marker overrides.</summary>
        internal int StoredCoordinateComponentCount => self.coordinateOverrides.Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}
