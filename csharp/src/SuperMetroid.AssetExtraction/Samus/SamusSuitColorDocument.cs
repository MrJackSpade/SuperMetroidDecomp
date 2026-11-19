using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The import's Power, Varia, and Gravity normal body colour rows (#1637), import data only the import pre-colours her sheets and shades from.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the Power, Varia and Gravity Suits' colour rows, which only the import reads to pre-colour Samus's sheets and suit shades; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportColourFiles), which the installer deletes before it publishes.")]
public sealed record SamusSuitColorDocument
{
    /// <summary>Gets the schema version required by <see cref="SamusSuitColorFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the sixteen editable Power-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Power { get; init; }

    /// <summary>Gets the sixteen editable Varia-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Varia { get; init; }

    /// <summary>Gets the sixteen editable Gravity-suit RGB5 colors.</summary>
    public required PaletteRgb5[] Gravity { get; init; }
}
