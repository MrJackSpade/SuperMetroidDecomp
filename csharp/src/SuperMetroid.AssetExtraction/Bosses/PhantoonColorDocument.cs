using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>JSON schema for Phantoon and Wrecked Ship power-transition colors.</summary>
public sealed record PhantoonColorDocument
{
    /// <summary>Gets the schema version required by <see cref="PhantoonColorFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets each of the eight health bands' sixteen editable RGB5 colors, keyed by the band's installed key.</summary>
    public required Dictionary<string, PaletteRgb5[]> HealthBands { get; init; }

    /// <summary>Gets the sixteen-color target used while Phantoon fades out.</summary>
    public required PaletteRgb5[] FadeOut { get; init; }

    /// <summary>Gets each powered part's sixteen target colors, keyed by the part's installed key.</summary>
    public required Dictionary<string, PaletteRgb5[]> PowerOn { get; init; }
}
