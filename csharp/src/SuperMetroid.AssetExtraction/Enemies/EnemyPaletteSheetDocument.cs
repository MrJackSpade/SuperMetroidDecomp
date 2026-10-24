using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Editable JSON schema for one enemy's sixteen-color palette; room selection, destination assignment, and animation are separate engine behavior.</summary>
public sealed record EnemyPaletteSheetDocument
{
    /// <summary>Schema revision, which must be 1 for this loader.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly sixteen non-null RGB5 colors in native palette order, with each red, green, and blue channel in 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
