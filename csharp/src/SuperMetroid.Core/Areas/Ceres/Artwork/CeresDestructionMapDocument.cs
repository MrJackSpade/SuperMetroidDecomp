namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable three-slice Mode 7 map schema for Ceres destruction; each cell is an eight-bit character index without ordinary BG palette, priority, or flip attributes.</summary>
[SpatialComponents("External format: the destruction-maps JSON stores its declared grid dimensions as width/height integers.")]
public sealed record CeresDestructionMapDocument
{
    /// <summary>Map schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Required width of each row-major view in Mode 7 map cells: 32.</summary>
    public required int Width { get; init; }
    /// <summary>Required height of each row-major view in Mode 7 map cells: 24.</summary>
    public required int Height { get; init; }
    /// <summary>Native source offsets $600, $900 and $C00, in that order.</summary>
    public required int[][] Views { get; init; }
}
