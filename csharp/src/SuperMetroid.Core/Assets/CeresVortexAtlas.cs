namespace SuperMetroid.Core.Assets;

/// <summary>Atlas tile identities used to calculate the Ceres vortex core and edge.</summary>
internal static class CeresVortexAtlas
{
    /// <summary>Tile1C0, first large vortex tile; eleven large pieces are packed
    /// eight per two-row atlas strip and rendered as a three-column grid.</summary>
    internal const int Core = 0x1c0;
    /// <summary>Tile1E6, small lower-edge drawing repeated across the final grid cell.</summary>
    internal const int LowerEdge = 0x1e6;
}
