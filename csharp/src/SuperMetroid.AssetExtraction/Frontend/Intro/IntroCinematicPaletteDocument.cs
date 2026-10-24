using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Editable full-CGRAM color schema for opening narration, portrait, object, and cross-fade inks; scene phases and fade timing are not document content.</summary>
public sealed record IntroCinematicPaletteDocument
{
    /// <summary>Schema revision required to equal <see cref="IntroCinematicPaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 non-null RGB5 colors in CGRAM index order, including every sixteen-color row's slot zero; each red, green, and blue channel is 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
