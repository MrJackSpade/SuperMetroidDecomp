using SuperMetroid.Core;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapSaveMarkerPositions"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    /// <summary>Counts explicitly stored X and Y coordinates of save-marker layout edits.</summary>
    extension(MapSaveMarkerPositions self)
    {
        /// <summary>Counts each non-null coordinate component separately across marker overrides.</summary>
        internal int StoredCoordinateComponentCount => self.coordinateOverrides.Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}
