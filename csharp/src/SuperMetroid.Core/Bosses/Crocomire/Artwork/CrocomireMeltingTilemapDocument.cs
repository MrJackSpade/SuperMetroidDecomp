namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>One ordered 16x16 visual tile layout; mechanics are not configurable.</summary>
[SpatialComponents("External format: the melting tilemap JSON stores its declared grid dimensions as width/height integers.")]
public sealed record CrocomireMeltingTilemapDocument
{
    /// <summary>Visual tile-layout revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Required schema width declaration of 16; this does not replace the consuming BG tilemap's 32-cell row stride.</summary>
    public required int Width { get; init; }
    /// <summary>Required schema height declaration of 16, establishing a 256-cell payload with the width.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly 256 cells in native linear source order, including blank margins; runtime placement consumes them as eight 32-cell BG2 rows.</summary>
    public required CrocomireMeltingTilemapCell[] Cells { get; init; }
}
