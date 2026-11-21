using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The import's Samus full-body cycle colour rows (#1637), import data only the import pre-colours her shades from.</summary>
public sealed record SamusFullBodyCycleColorDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets each suit's Speed Booster palettes, keyed by the suit's name, each with four 16-color shades.</summary>
    public required Dictionary<string, PaletteRgb5[][]> SpeedBooster { get; init; }
    /// <summary>Gets each suit's Screw Attack palettes, keyed by the suit's name, each with four 16-color shades.</summary>
    public required Dictionary<string, PaletteRgb5[][]> ScrewAttack { get; init; }
    /// <summary>Gets each suit's stored-shine palettes, keyed by the suit's name, each with four 16-color shades.</summary>
    public required Dictionary<string, PaletteRgb5[][]> StoredShine { get; init; }
    /// <summary>Gets each suit's active-shinespark palettes, keyed by the suit's name, each with four 16-color shades.</summary>
    public required Dictionary<string, PaletteRgb5[][]> ActiveShinespark { get; init; }
}
