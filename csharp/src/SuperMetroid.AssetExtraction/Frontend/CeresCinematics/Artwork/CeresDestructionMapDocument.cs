using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable three-slice Mode 7 map schema for Ceres destruction; each cell is an eight-bit character index without ordinary BG palette, priority, or flip attributes.</summary>
public sealed partial record CeresDestructionMapDocument
{
    /// <summary>Map schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Required width of each row-major view in Mode 7 map cells: <see cref="CeresDestructionMapFormat.MapWidth"/>.</summary>
    public required int Width { get; init; }
    /// <summary>Required height of each row-major view in Mode 7 map cells: <see cref="CeresDestructionMapFormat.MapHeight"/>.</summary>
    public required int Height { get; init; }
    /// <summary>Native source offsets $600, $900 and $C00, in that order.</summary>
    public required int[][] Views { get; init; }
}
