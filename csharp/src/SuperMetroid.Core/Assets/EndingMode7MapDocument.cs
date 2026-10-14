namespace SuperMetroid.Core.Assets;

/// <summary>Editable eight-bit Mode-7 tilemap schema shared by 128x64 ending backdrops and the full 128x128 post-credits beam/icon map.</summary>
[SpatialComponents("External format: the ending Mode 7 map JSON stores its declared grid dimensions as width/height integers.")]
public sealed record EndingMode7MapDocument
{
    /// <summary>Schema revision, currently 1 for both ending backdrop and reward-icon maps.</summary>
    public required int Version { get; init; }
    /// <summary>Number of tile indices per map row; both supported map forms require 128.</summary>
    public required int Width { get; init; }
    /// <summary>Number of map rows: 64 for a repeated ending backdrop half-map, or 128 for the reward-icon map.</summary>
    public required int Height { get; init; }
    /// <summary>Exactly Width times Height row-ordered tile indices in the range 0..255, selecting tiles from the separate 256-tile character atlas.</summary>
    public required int[] Tiles { get; init; }
}
