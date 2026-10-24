using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>JSON schema for complete pause, file-select, and area-map palettes.</summary>
public sealed record MapStaticPalettesDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="MapStaticPalettesFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the 256 editable RGB5 colors used by the pause screen.</summary>
    public required PaletteRgb5[] Pause { get; init; }

    /// <summary>Gets the 256 editable RGB5 colors used by file select.</summary>
    public required PaletteRgb5[] FileSelect { get; init; }

    /// <summary>Gets the six 256-color Zebes world-map palettes keyed by area name.</summary>
    public required Dictionary<string, PaletteRgb5[]> World { get; init; }
}
