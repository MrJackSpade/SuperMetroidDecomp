using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The import's Samus full-body cycle colour rows (#1637), import data only the import pre-colours her shades from.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Samus's Speed Booster, Screw Attack and shinespark cycle colour rows, which only the import reads to pre-colour her shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record SamusFullBodyCycleColorDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Suit order: Power, Varia, Gravity; each contains four 16-color shades.</summary>
    public required PaletteRgb5[][][] SpeedBooster { get; init; }
    /// <summary>Gets Power, Varia, and Gravity Screw Attack palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] ScrewAttack { get; init; }
    /// <summary>Gets Power, Varia, and Gravity stored-shine palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] StoredShine { get; init; }
    /// <summary>Gets Power, Varia, and Gravity active-shinespark palettes, each with four 16-color shades.</summary>
    public required PaletteRgb5[][][] ActiveShinespark { get; init; }
}
