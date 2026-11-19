using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Editable full-CGRAM color schema for opening narration, portrait, object, and cross-fade inks; scene phases and fade timing are not document content.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the opening's colours, which only the import reads to pre-colour the opening's sheets; the installed IntroBackdropDocument holds its backdrops; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its ImportDataFiles), which the installer deletes before it publishes.")]
public sealed record IntroCinematicPaletteDocument
{
    /// <summary>Schema revision required to equal <see cref="IntroCinematicPaletteFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 non-null RGB5 colors in CGRAM index order, including every sixteen-color row's slot zero; each red, green, and blue channel is 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
