using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Editable JSON schema for Botwoon's eight health-dependent sprite-palette images.</summary>
public sealed record BotwoonColorDocument
{
    /// <summary>Gets the Botwoon color schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets eight ordered rows of sixteen RGB5 colors corresponding to native <c>$B3:971B-$981A</c>.</summary>
    public required PaletteRgb5[][] Health { get; init; }
}
