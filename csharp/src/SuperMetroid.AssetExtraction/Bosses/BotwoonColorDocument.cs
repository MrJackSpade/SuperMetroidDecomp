using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Editable JSON schema for Botwoon's eight health-dependent sprite-palette images.</summary>
public sealed record BotwoonColorDocument
{
    /// <summary>Gets the Botwoon color schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets each of the eight health bands' sixteen RGB5 colors, keyed by the band's installed key (native <c>$B3:971B-$981A</c>).</summary>
    public required Dictionary<string, PaletteRgb5[]> Health { get; init; }
}
